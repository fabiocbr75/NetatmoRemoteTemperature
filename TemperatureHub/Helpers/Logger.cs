using TemperatureHub.Models;

namespace TemperatureHub.Helpers
{

    public static class Logger
    {
        private static ISharedData _sharedData = null;
        public static void SetSharedData(ISharedData sharedData)
        {
            _sharedData = sharedData;
        }
        public static void Info(string context, string message)
        {
//#if DEBUG
            System.Console.WriteLine($"{context}-{message}");
//#endif

        }

        public static void Error(string context, string message)
        {
//#if DEBUG
            System.Console.WriteLine($"{context}-{message}");
//#endif
            if (_sharedData != null)
            {
                _sharedData.LogQueue.Enqueue((ErrorType: 0, Context: context, Message: message));
            }
        }

        public static void Warn(string context, string message)
        {
//#if DEBUG
            System.Console.WriteLine(message);
//#endif
            if (_sharedData != null)
            {
                _sharedData.LogQueue.Enqueue((ErrorType: 1, Context: context, Message: message));
            }
        }

        public static void Message(string context, string message)
        {
            System.Console.WriteLine($"{context}-{message}");
        }

    }
}