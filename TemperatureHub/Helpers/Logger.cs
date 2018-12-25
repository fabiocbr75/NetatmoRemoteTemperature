namespace TemperatureHub.Helpers
{

    public static class Logger
    {
        public static void Info(string context, string message)
        {
#if DEBUG
            System.Console.WriteLine($"{context}-{message}");
#endif

        }

        public static void Error(string context, string message)
        {
#if DEBUG
            System.Console.WriteLine($"{context}-{message}");
#endif

        }

        public static void Warn(string context, string message)
        {
#if DEBUG
            System.Console.WriteLine(message);
#endif

        }

        public static void Message(string context, string message)
        {
            System.Console.WriteLine($"{context}-{message}");
        }

    }
}