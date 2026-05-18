using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Common
{
    [ServiceContract]
    public interface IWeather
    {
        [OperationContract]
        void StartSession(string meta);
        [OperationContract]
        void PushSample(WeatherSample sample);
        [OperationContract]
        void EndSession();
    }
}
