using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub.Helpers
{
    public class Number
    {
        public static double HalfRound(double value)
        {
            return Math.Round(value * 2) / 2;
        }
    }
}
