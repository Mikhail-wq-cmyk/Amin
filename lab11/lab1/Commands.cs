using System.Text;

namespace Lab1;

public static class CommandHandler
{
    static string symbols = "ACDEFGHIKLMNPQRSTVWY";

    public static bool Checker(string aminoacid, StreamWriter output)
    {
        if (string.IsNullOrEmpty(aminoacid))
        {
            output.WriteLine("Аминокислотная строка пуста или null.");
            return false;
        }

        foreach (char symbol in aminoacid)
        {
            if (!symbols.Contains(symbol.ToString()))
            {
                output.WriteLine($"Недопустимый символ: {symbol}");
                return false;
            }
        }
        return true;
    }

    public static void ExecuteCommand(StreamWriter output, string commandLine, List<GeneticData> allData, ref int commandCounter)
    {
        string cleanLine = commandLine.Trim();
        string[] parts = cleanLine.Split('\t');
        string command = parts[0].Trim().ToLower();

        output.WriteLine();
        output.WriteLine(new string('-', 40));
        output.WriteLine($"{commandCounter:D3}\t{cleanLine}");

        if (command == "search" && parts.Length > 1)
        {
            PerformSearch(output, parts[1], allData);
        }
        else if (command == "diff" && parts.Length > 2)
        {
            CalculateDifference(output, parts[1], parts[2], allData);
        }
        else if (command == "mode" && parts.Length > 1)
        {
            FindMostCommonAminoAcid(output, parts[1], allData);
        }
        else
        {
            output.WriteLine("Invalid command");
        }

        commandCounter++;
    }

    private static void PerformSearch(StreamWriter output, string rleSequence, List<GeneticData> allData)
    {
        string sequence = DecodeRLE(rleSequence);
        bool found = false;

        foreach (GeneticData data in allData)
        {
            if (data.amino_acids.Contains(sequence))
            {
                output.WriteLine($"{data.organism}\t{data.protein}");
                found = true;
            }
        }

        if (!found)
        {
            output.WriteLine("NOT FOUND");
        }
    }

    private static void CalculateDifference(StreamWriter output, string firstProteinName, string secondProteinName, List<GeneticData> allData)
    {
        GeneticData data1 = FindProteinByName(allData, firstProteinName);
        GeneticData data2 = FindProteinByName(allData, secondProteinName);

        if (data1.protein == null || data2.protein == null)
        {
            string missing = "";
            if (data1.protein == null) missing += firstProteinName + " ";
            if (data2.protein == null) missing += secondProteinName + " ";
            output.WriteLine($"amino-acids difference: MISSING: {missing.Trim()}");
            return;
        }

        int diff = GetSequenceDifference(data1.amino_acids, data2.amino_acids);
        output.WriteLine($"amino-acids difference: {diff}");
    }

    private static void FindMostCommonAminoAcid(StreamWriter output, string proteinName, List<GeneticData> allData)
    {
        GeneticData dataItem = FindProteinByName(allData, proteinName);

        if (dataItem.protein == null)
        {
            output.WriteLine($"amino-acid occurs: MISSING: {proteinName}");
            return;
        }

        if (string.IsNullOrEmpty(dataItem.amino_acids))
        {
            output.WriteLine($"amino-acid occurs: ? 0");
            return;
        }

        Dictionary<char, int> countMap = new Dictionary<char, int>();
        foreach (char c in dataItem.amino_acids)
        {
            if (countMap.ContainsKey(c))
                countMap[c]++;
            else
                countMap[c] = 1;
        }
          
        


        char mostCommonChar = ' ';
        int maxCount = 0;

        foreach (var pair in countMap)
        {
            if (pair.Value > maxCount || (pair.Value == maxCount && pair.Key < mostCommonChar))
            {
                mostCommonChar = pair.Key;
                maxCount = pair.Value;
            }
        }

        output.WriteLine($"amino-acid occurs: {mostCommonChar} {maxCount}");
    }

    public static List<GeneticData> LoadGeneticData(string path)
    {
        List<GeneticData> result = new List<GeneticData>();
        int lineNumber = 0;

        foreach (string line in File.ReadLines(path))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split('\t');
            if (parts.Length < 3)
            {
                Console.WriteLine($"Предупреждение: строка {lineNumber} содержит меньше 3 колонок: {line}");
                continue;
            }

            string aminoAcidsCompressed = parts[2];

            if (string.IsNullOrWhiteSpace(aminoAcidsCompressed))
            {
                Console.WriteLine($"Предупреждение: строка {lineNumber} содержит пустую аминокислотную последовательность.");
                continue;
            }

            string aminoAcidsDecoded = DecodeRLE(aminoAcidsCompressed);

            if (!Checker(aminoAcidsDecoded, null))
            {
                Console.WriteLine($"Ошибка в строке {lineNumber}: недопустимая аминокислотная последовательность: {aminoAcidsDecoded}");
                continue;
            }

            result.Add(new GeneticData
            {
                protein = parts[0],
                organism = parts[1],
                amino_acids = aminoAcidsDecoded
            });
        }

        return result;
    }

    private static GeneticData FindProteinByName(List<GeneticData> allData, string proteinName)
    {
        foreach (GeneticData data in allData)
        {
            if (data.protein == proteinName)
            {
                return data;
            }
        }
        return new GeneticData();
    }

    private static string DecodeRLE(string compressed)
    {
        if (string.IsNullOrEmpty(compressed)) return "";
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < compressed.Length; i++)
        {
            if (char.IsDigit(compressed[i]) && i + 1 < compressed.Length)
            {
                int count = compressed[i] - '0';
                char aminoAcid = compressed[i + 1];

                for (int j = 0; j < count; j++)
                {
                    result.Append(aminoAcid);
                }
                i++;
            }
            else
            {
                result.Append(compressed[i]);
            }
        }
        return result.ToString();
    }

    private static int GetSequenceDifference(string seq1, string seq2)
    {
        int diffCount = 0;
        int maxLength = Math.Max(seq1.Length, seq2.Length);

        for (int i = 0; i < maxLength; i++)
        {
            char char1 = (i < seq1.Length) ? seq1[i] : ' ';
            char char2 = (i < seq2.Length) ? seq2[i] : ' ';

            if (char1 != char2)
            {
                diffCount++;
            }
        }

        return diffCount;
    }
}