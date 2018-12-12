using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureHub
{
    public class AppSettings
    {
        public string DbPath { get; set; }

        public string DbFullPath
        {
            get
            {
                var loc = System.AppContext.BaseDirectory;
                var folder = Path.Combine(loc, DbPath);

                return folder;
            }
        }
    }
}
