using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public class WeatherFileReader : IDisposable
    {
        private StreamReader reader;
        private bool disposed = false;

        public WeatherFileReader(string path) 
        {
            reader = new StreamReader(path);
        }

        public string ReadLine()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(WeatherFileReader));
            return reader.ReadLine();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    reader?.Dispose();
                }
                disposed = true;
            }
        }
        ~WeatherFileReader()
        {
            Dispose(false);
        }
    }
}
