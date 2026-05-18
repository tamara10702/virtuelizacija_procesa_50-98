using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            WeatherService service = new WeatherService();
            ServiceHost host = new ServiceHost(service);

            host.Open();
            Console.WriteLine("Server is running. Press any key to stop.");
            Console.ReadKey();
            host.Close();
            Console.WriteLine("Service is closed.");
        }
    }
}
