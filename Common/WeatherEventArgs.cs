using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class WeatherEventArgs : EventArgs
    {
        public double Value { get; }
        public string Message { get; }

        public WeatherEventArgs(double value, string message)
        {
            Value = value;
            Message = message;
        }
    }
}
