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

            service.OnTransferStarted += (sender, e) =>
                Console.WriteLine($"[EVENT] {e.Message}");

            service.OnSampleReceived += (sender, e) =>
                Console.WriteLine($"[EVENT] {e.Message}, Pressure={e.Value}");

            service.OnTransferCompleted += (sender, e) =>
                Console.WriteLine($"[EVENT] {e.Message}");

            service.OnWarningRaised += (sender, e) =>
                Console.WriteLine($"[WARNING] {e.Message}");

            service.OnPressureSpike += (sender, e) =>
                Console.WriteLine($"[PRESSURE SPIKE] {e.Message}");

            service.OnOutOfBandWarning += (sender, e) =>
                Console.WriteLine($"[OUT OF BAND] {e.Message}");

            service.OnVPActSpike += (sender, e) =>
                Console.WriteLine($"[VPAct SPIKE] {e.Message}");

            service.OnVPDefSpike += (sender, e) =>
                Console.WriteLine($"[VPDef SPIKE] {e.Message}");

            host.Open();
            Console.WriteLine("Server is running. Press any key to stop.");
            Console.ReadKey();
            host.Close();
            Console.WriteLine("Service is closed.");
        }
    }
}
