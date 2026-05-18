using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Common
{
    public class WeatherFileWriter : IDisposable
    {
        private StreamWriter writer;
        private bool disposed = false;
        public WeatherFileWriter(string path)
        {
            writer = new StreamWriter(path, append: true);
        }

        public void WriteLine(string line)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(WeatherFileWriter));
            writer.WriteLine(line);
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
                    writer?.Dispose();
                }
                disposed = true;
            }
        }

        ~WeatherFileWriter()
        {
            Dispose(false);
        }
    }
}
