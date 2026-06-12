using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.IO;
using System.Configuration;
using System.Globalization;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WeatherService : IWeather
    {
        private static WeatherFileWriter dataWriter = null;
        private static WeatherFileWriter rejectsWriter = null;

        private static readonly string ARCHIVE_PATH = @"C:\Users\User\OneDrive\Desktop\psi\6. semestar\virtuelizacija\projekatFajlovi\archive";

        private static bool sessionActive = false;
        private static readonly object lockObject = new object();

        private static List<WeatherSample> samples = new List<WeatherSample>();

        public delegate void WeatherEventHandler(object sender, WeatherEventArgs e);

        public event WeatherEventHandler OnTransferStarted;
        public event WeatherEventHandler OnSampleReceived;
        public event WeatherEventHandler OnTransferCompleted;
        public event WeatherEventHandler OnWarningRaised;
        public event WeatherEventHandler OnPressureSpike;
        public event WeatherEventHandler OnOutOfBandWarning;
        public event WeatherEventHandler OnVPActSpike;
        public event WeatherEventHandler OnVPDefSpike;

        private readonly double P_THRESHOLD;
        private readonly double VPact_THRESHOLD;
        private readonly double VPdef_THRESHOLD;
        private readonly double MEAN_DEVIATION;

        public WeatherService()
        {
            P_THRESHOLD = double.Parse(ConfigurationManager.AppSettings["P_THRESHOLD"] ?? "0.2", CultureInfo.InvariantCulture);
            VPact_THRESHOLD = double.Parse(ConfigurationManager.AppSettings["VPact_THRESHOLD"] ?? "0.15", CultureInfo.InvariantCulture);
            VPdef_THRESHOLD = double.Parse(ConfigurationManager.AppSettings["VPdef_THRESHOLD"] ?? "0.3", CultureInfo.InvariantCulture);
            MEAN_DEVIATION = double.Parse(ConfigurationManager.AppSettings["MEAN_DEVIATION"] ?? "0.25", CultureInfo.InvariantCulture);
        }

        public void EndSession()
        {
            lock (lockObject)
            {
                if (!sessionActive)
                {
                    throw new FaultException<DataFormatFault>(new DataFormatFault("Sesija nije pokrenuta. Ne možete završiti sesiju."));
                }
                dataWriter?.Dispose();
                rejectsWriter?.Dispose();
                dataWriter = null;
                rejectsWriter = null;

                sessionActive = false;
            }

            Console.WriteLine("Zavrsen prenos.Sesija zavrsena.");
            OnTransferCompleted?.Invoke(this, new WeatherEventArgs(0, "Sesija zavrsena"));
        }

        public void PushSample(WeatherSample sample)
        {
            if (!sessionActive)
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Sesija nije pokrenuta. Pozovite prvo StartSession."));
            }

            if (sample == null)
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Sample je null."));
            }

            if (sample.Date == default)
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Datum nije postavljen."));
            }

            try
            {
                if (sample.Pressure <= 0)
                    throw new FaultException<ValidationFault>(new ValidationFault("Pritisak mora biti veci od 0."));

                if (sample.T < -100 || sample.T > 60)
                    throw new FaultException<ValidationFault>(new ValidationFault("Temperatura je van realisticnog opsega."));

                if (sample.Tpot < 173 || sample.Tpot > 333)
                    throw new FaultException<ValidationFault>(new ValidationFault("Tpot je van realisticnog opsega."));

                if (sample.Tdew < -100 || sample.Tdew > 60)
                    throw new FaultException<ValidationFault>(new ValidationFault("Tdew je van realisticnog opsega."));

                if (sample.VPmax < 0 || sample.VPmax > 100)
                    throw new FaultException<ValidationFault>(new ValidationFault("VPmax je van realisticnog opsega."));

                if (sample.VPdef < 0 || sample.VPdef > 100)
                    throw new FaultException<ValidationFault>(new ValidationFault("VPdef je van realisticnog opsega."));

                if (sample.VPact < 0 || sample.VPact > 100)
                    throw new FaultException<ValidationFault>(new ValidationFault("VPact je van realisticnog opsega."));
            }
            catch (FaultException<ValidationFault> ex)
            {
                lock (lockObject)
                {
                    string rejectLine = $"{sample.Date},{sample.T},{sample.Pressure},{sample.Tpot},{sample.Tdew},{sample.VPmax},{sample.VPdef},{sample.VPact},{ex.Detail.Message}";
                    rejectsWriter?.WriteLine(rejectLine);
                }

                Console.WriteLine($"Odbacen uzorak: {ex.Detail.Message}");
                throw;
            }

            lock (lockObject)
            {
                string csvLine = $"{sample.Date},{sample.T},{sample.Pressure},{sample.Tpot},{sample.Tdew},{sample.VPmax},{sample.VPdef},{sample.VPact}";
                dataWriter?.WriteLine(csvLine);
                samples.Add(sample);
            }

            OnSampleReceived?.Invoke(this, new WeatherEventArgs(sample.Pressure, "Sample primljen"));
            Console.WriteLine($"Prenos u toku... Sample primljen: T={sample.T}, Pressure={sample.Pressure}, Date={sample.Date}");

            AnalyzeSample(sample);
        }

        private void AnalyzeSample(WeatherSample current)
        {
            if (samples.Count < 2)
                return;

            var previous = samples[samples.Count - 2];

            // Analitika 1

            double deltaP = current.Pressure - previous.Pressure;
            if (Math.Abs(deltaP) > P_THRESHOLD)
            {
                string direction = deltaP > 0 ? "iznad ocekivanog" : "ispod ocekivanog";
                string msg = $"Nagla promena pritiska ({direction}), deltaP={deltaP:F2}";
                OnPressureSpike?.Invoke(this, new WeatherEventArgs(current.Pressure, msg));
                OnWarningRaised?.Invoke(this, new WeatherEventArgs(current.Pressure, msg));
            }

            double pMean = samples.Average(s => s.Pressure);
            if (current.Pressure < (1 - MEAN_DEVIATION) * pMean || current.Pressure > (1 + MEAN_DEVIATION) * pMean)
            {
                string direction = current.Pressure < pMean ? "ispod ocekivane vrednosti" : "iznad ocekivane vrednosti";
                string msg = $"Pritisak van pojasa proseka ({direction}), P={current.Pressure:F2}, Pmean={pMean:F2}";
                OnOutOfBandWarning?.Invoke(this, new WeatherEventArgs(current.Pressure, msg));
                OnWarningRaised?.Invoke(this, new WeatherEventArgs(current.Pressure, msg));
            }

            // Analitika 2

            double deltaVPact = current.VPact - previous.VPact;
            if (Math.Abs(deltaVPact) > VPact_THRESHOLD)
            {
                string direction = deltaVPact > 0 ? "iznad ocekivanog" : "ispod ocekivanog";
                string msg = $"Nagla promena VPact ({direction}), deltaVPact={deltaVPact:F2}";
                OnVPActSpike?.Invoke(this, new WeatherEventArgs(current.VPact, msg));
                OnWarningRaised?.Invoke(this, new WeatherEventArgs(current.VPact, msg));
            }

            double deltaVPdef = current.VPdef - previous.VPdef;
            if (Math.Abs(deltaVPdef) > VPdef_THRESHOLD)
            {
                string direction = deltaVPdef > 0 ? "iznad ocekivanog" : "ispod ocekivanog";
                string msg = $"Nagla promena VPdef ({direction}), deltaVPdef={deltaVPdef:F2}";
                OnVPDefSpike?.Invoke(this, new WeatherEventArgs(current.VPdef, msg));
                OnWarningRaised?.Invoke(this, new WeatherEventArgs(current.VPdef, msg));
            }
        }

        public void StartSession(string meta)
        {
            if (string.IsNullOrWhiteSpace(meta))
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Meta podaci ne smeju biti prazni."));
            }

            lock (lockObject)
            {
                if (sessionActive)
                {
                    throw new FaultException<DataFormatFault>(new DataFormatFault("Sesija je vec aktivna."));
                }
                try
                {
                    if (!Directory.Exists(ARCHIVE_PATH))
                        Directory.CreateDirectory(ARCHIVE_PATH);

                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string dataPath = Path.Combine(ARCHIVE_PATH, $"measurements_{timestamp}.csv");
                    string rejectsPath = Path.Combine(ARCHIVE_PATH, $"rejects_{timestamp}.csv");

                    dataWriter = new WeatherFileWriter(dataPath);
                    dataWriter.WriteLine("Date,T,Pressure,Tpot,Tdew,VPmax,VPdef,VPact");

                    rejectsWriter = new WeatherFileWriter(rejectsPath);
                    rejectsWriter.WriteLine("Date,T,Pressure,Tpot,Tdew,VPmax,VPdef,VPact,Reason");
                }
                catch (Exception ex)
                {
                    throw new FaultException<DataFormatFault>(new DataFormatFault($"Greska pri otvaranju arhivskih fajlova: {ex.Message}"));
                }
                samples.Clear();
                sessionActive = true;
            }

            //Console.WriteLine($"Sesija zapoceta: {meta}");
            OnTransferStarted?.Invoke(this, new WeatherEventArgs(0, $"Sesija zapoceta: {meta}"));

        }
    }
}
