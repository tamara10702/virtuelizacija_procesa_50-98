using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using System.IO;
using System.Globalization;
using System.Security.Authentication.ExtendedProtection;
using System.ServiceModel;

namespace Client
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string csvPath = @"C:\Users\User\OneDrive\Desktop\psi\6. semestar\virtuelizacija\projekatFajlovi\cleaned_weather.csv";
            string invalidLogPath = @"C:\Users\User\OneDrive\Desktop\psi\6. semestar\virtuelizacija\projekatFajlovi\invalid_samples_log.txt";

            var samples = LoadCsv(csvPath, invalidLogPath);
            Console.WriteLine($"Ucitano validnih redova: {samples.Count}");

            ChannelFactory<IWeather> factory = new ChannelFactory<IWeather>("WeatherService");
            IWeather proxy = factory.CreateChannel();

            try
            {
                proxy.StartSession("Test session metadata");

                foreach (var sample in samples)
                {
                    try
                    {
                        proxy.PushSample(sample);
                        Console.WriteLine($"Sample poslat: {sample.Date} T={sample.T} Pressure={sample.Pressure}");
                    }
                    catch (FaultException<DataFormatFault> ex)
                    {
                        Console.WriteLine($"DataFormatFault: {ex.Detail.Message}");
                    }
                    catch (FaultException<ValidationFault> ex)
                    {
                        Console.WriteLine($"ValidationFault: {ex.Detail.Message}");
                    }
                }

                proxy.EndSession();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Greska pri sesiji: {ex.Message}");
            }

            Console.WriteLine("Test je zavrsen.");
            Console.ReadKey();

        }

        static List<WeatherSample> LoadCsv(string path, string invalidLogPath)
        {
            var samples = new List<WeatherSample>();
            var invalidRows = new List<string>();
            int samplesRead = 0;
            using (var reader = new WeatherFileReader(path))
            {
                reader.ReadLine(); // Preskoci header
                string line;

                while (samplesRead<107 && (line = reader.ReadLine()) != null)
                {
                    try
                    {
                        string[] cols = line.Split(',');
                        var sample = new WeatherSample
                        {
                            Date = DateTime.Parse(cols[0], CultureInfo.InvariantCulture),
                            Pressure = double.Parse(cols[1], CultureInfo.InvariantCulture),
                            T = double.Parse(cols[2], CultureInfo.InvariantCulture),
                            Tpot = double.Parse(cols[3], CultureInfo.InvariantCulture),
                            Tdew = double.Parse(cols[4], CultureInfo.InvariantCulture),
                            VPmax = double.Parse(cols[5], CultureInfo.InvariantCulture),
                            VPdef = double.Parse(cols[6], CultureInfo.InvariantCulture),
                            VPact = double.Parse(cols[7], CultureInfo.InvariantCulture)
                        };
                        samples.Add(sample);
                        samplesRead++;

                    }
                    catch
                    {
                        invalidRows.Add(line);
                    }
                }
                while ((line = reader.ReadLine()) != null)
                {
                    invalidRows.Add(line);
                }
            }
            try
            {
                File.WriteAllLines(invalidLogPath, invalidRows);
            }
            catch(Exception ex)
            { 
                Console.WriteLine($"Greska pri pisanju log fajla: {ex.Message}");
            }

            return samples;
        }
    }
}
