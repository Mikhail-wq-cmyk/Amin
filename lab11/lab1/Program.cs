using System.Text;
using Lab1;

class Program
{
    static void Main(string[] args)
    {
        int testNumber = 0;

        string sequencesFileName = $"sequences.{testNumber}.txt";
        string commandsFileName = $"commands.{testNumber}.txt";
        string outputFileName = $"genedata.{testNumber}.txt";

        if (!File.Exists(sequencesFileName) || !File.Exists(commandsFileName))
        {
            Console.WriteLine($"Не найдены входные файлы: '{sequencesFileName}' или '{commandsFileName}'");
            Console.WriteLine("Убедитесь, что файлы находятся в папке с программой:");
            Console.WriteLine($"- sequences.{testNumber}.txt");
            Console.WriteLine($"- commands.{testNumber}.txt");
            return;
        }

        List<GeneticData> geneticDataCollection = CommandHandler.LoadGeneticData(sequencesFileName);

        using var outputWriter = new StreamWriter(outputFileName, false, Encoding.UTF8);
        outputWriter.WriteLine("Malyshko Mikhail");
        outputWriter.WriteLine("Генетический поиск");
        outputWriter.WriteLine();

        int operationIndex = 1;
        foreach (var rawLine in File.ReadLines(commandsFileName))
        {
            if (string.IsNullOrWhiteSpace(rawLine))
                continue;

            CommandHandler.ExecuteCommand(outputWriter, rawLine, geneticDataCollection, ref operationIndex);
        }

        Console.WriteLine($"Готово. Результат записан в '{outputFileName}'");
    }
}