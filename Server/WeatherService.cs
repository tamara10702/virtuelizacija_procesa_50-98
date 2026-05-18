using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
                     ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WeatherService : IWeather
    {
        private static bool sessionActive = false;
        private static readonly object lockObject = new object();

        public void EndSession()
        {
            lock (lockObject)
            {
                if (!sessionActive)
                {
                    throw new FaultException<DataFormatFault>(new DataFormatFault("Sesija nije pokrenuta. Ne možete završiti sesiju."));
                }

                sessionActive = false;
            }

            Console.WriteLine("Sesija zavrsena.");
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

            if (sample.Pressure <= 0)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Pritisak mora biti veci od 0."));
            }

            if (sample.T < -100 || sample.T > 60)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Temperatura je van realisticnog opsega."));
            }

            if (sample.Tpot < -100 || sample.Tpot > 60)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Tpot je van realisticnog opsega."));
            }

            if (sample.Tdew < -100 || sample.Tdew > 60)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("Tdew je van realisticnog opsega."));
            }

            if (sample.VPmax < 0 || sample.VPmax > 100)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("VPmax je van realisticnog opsega."));
            }

            if (sample.VPdef < 0 || sample.VPdef > 100)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("VPdef je van realisticnog opsega."));
            }

            if (sample.VPact < 0 || sample.VPact > 100)
            {
                throw new FaultException<ValidationFault>(new ValidationFault("VPact je van realisticnog opsega."));
            }

            Console.WriteLine($"Sample primljen: T={sample.T}, Pressure={sample.Pressure}, Date={sample.Date}");
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
                sessionActive = true;
            }
            
            Console.WriteLine($"Sesija zapoceta: {meta}");
        }
    }
}
