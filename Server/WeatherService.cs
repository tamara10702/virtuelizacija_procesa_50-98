using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Server
{
    public class WeatherService : IWeather
    {
        private static bool sessionActive = false;

        public void EndSession()
        {
            if (!sessionActive)
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Sesija nije pokrenuta. Ne možete završiti sesiju."));
            }

            sessionActive = false;
            Console.WriteLine($"Sesija zavrsena.");
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

            // ima jos da se implementira...
        }

        public void StartSession(string meta)
        {
            if (string.IsNullOrWhiteSpace(meta))
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Meta podaci ne smeju biti prazni."));
            }

            if (sessionActive)
            {
                throw new FaultException<DataFormatFault>(new DataFormatFault("Sesija je vec aktivna."));
            }

            sessionActive = true;
            Console.WriteLine($"Sesija zapoceta: {meta}");
        }
    }
}
