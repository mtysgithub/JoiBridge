namespace JoiBridge;

public static class ConsoleExtensions
{
    public static void WriteLine(string value, ConsoleColor color)
    {
        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ForegroundColor = defaultColor;
    }

    public static void Write(string value, ConsoleColor color)
    {
        var defaultColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ForegroundColor = defaultColor;
    }
}