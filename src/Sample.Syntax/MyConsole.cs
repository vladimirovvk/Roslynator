using System;

namespace Sample
{
    internal static class MyConsole
    {
        public static void Write(string value, ConsoleColor color)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ForegroundColor = tmp;
        }

        public static void Write(object value, ConsoleColor color)
        {
            Write(value!.ToString()!, color);
        }

        public static void Write(string value)
        {
            Console.Write(value);
        }

        public static void WriteLine(string value, ConsoleColor color)
        {
            ConsoleColor tmp = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = tmp;
        }

        public static void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public static void WriteLine()
        {
            Console.WriteLine();
        }
    }
}
