using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioKs
{
    public class Appli
    {
        public string NAME { get; set; }
        public float VERSION { get; set; }
        public bool FULL { get; set; }
        public List<AppFile> FILES { get; set; }

        public Appli(string name, string version, string full, List<AppFile> files)
        {
            NAME = name;
            VERSION = float.Parse(version.Replace(".", ","));
            FULL = full == "1" ? true : false;
            FILES = files;
        }
    }
}
