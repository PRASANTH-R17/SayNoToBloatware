namespace SNB.Cli.Console;

public static class ConsolePrompt
{
    public static void WriteLine(string message = "") => System.Console.WriteLine(message);

    public static void Write(string message) => System.Console.Write(message);

    public static string ReadLine() => System.Console.ReadLine() ?? string.Empty;

    public static void WaitForEnter(string message = "Press Enter to continue...")
    {
        WriteLine();
        Write(message);
        ReadLine();
    }

    public static int ReadIntInRange(string prompt, int min, int max)
    {
        while (true)
        {
            Write(prompt);
            var input = ReadLine().Trim();

            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            WriteLine($"Invalid selection. Enter {min}-{max}.");
        }
    }

    public static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Write(prompt);
            var input = ReadLine().Trim();

            if (input.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (input.Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            WriteLine("Please enter Y or N.");
        }
    }

    public static IReadOnlyList<int> ReadCommaSeparatedIndices(string prompt, int max)
    {
        while (true)
        {
            Write(prompt);
            var input = ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                WriteLine("No apps selected.");
                continue;
            }

            var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var indices = new List<int>();
            var invalid = false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out var value))
                {
                    WriteLine($"Invalid number: {part}");
                    invalid = true;
                    break;
                }

                if (value < 1 || value > max)
                {
                    WriteLine($"{value} is out of range");
                    invalid = true;
                    break;
                }

                indices.Add(value);
            }

            if (!invalid)
            {
                return indices.Distinct().OrderBy(i => i).ToList();
            }
        }
    }
}
