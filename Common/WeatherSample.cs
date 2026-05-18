using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ServiceModel;

namespace Common
{
    [DataContract]
    public class WeatherSample
    {
        double t;           // Temperatura
        double pressure;    // Pritisak
        double tpot;        // Potencijalna temperatura
        double tdew;        // Temperatura tacke rose
        double vpmax;       // Maks pritisak vodene pare
        double vpdef;       // Deficit pritiska vodene pare
        double vpact;       // Stvarni pritisak vodene pare
        DateTime date;

        public WeatherSample() { }

        public WeatherSample(double t, double pressure, double tpot, double tdew, double vpmax, double vpdef, double vpact, DateTime date)
        {
            this.t = t;
            this.pressure = pressure;
            this.tpot = tpot;
            this.tdew = tdew;
            this.vpmax = vpmax;
            this.vpdef = vpdef;
            this.vpact = vpact;
            this.date = date;
        }

        [DataMember]
        public double T { get => t; set => t = value; }

        [DataMember]
        public double Pressure { get => pressure; set => pressure = value; }

        [DataMember]
        public double Tpot { get => tpot; set => tpot = value; }

        [DataMember]
        public double Tdew { get => tdew; set => tdew = value; }

        [DataMember]
        public double VPmax { get => vpmax; set => vpmax = value; }

        [DataMember]
        public double VPdef { get => vpdef; set => vpdef = value; }

        [DataMember]
        public double VPact { get => vpact; set => vpact = value; }

        [DataMember]
        public DateTime Date { get => date; set => date = value; }
    }
}
