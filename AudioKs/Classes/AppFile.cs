using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioKs
{
    public class AppFile
    {
        public string NAME { get; set; }
        public string PATH { get; set; }
        public string VERSION { get; set; }
        public bool COMPRESSED { get; set; }
        public Uri LINK { get; set; }

        public AppFile(string name, string path, string version, string compressed, string link)
        {
            NAME = name;
            PATH = path;
            VERSION = version;
            COMPRESSED = compressed == "1" ? true : false;
            LINK = new Uri(link);
        }
    }



    public enum VersionType
    {
        prototype,
        alpha,
        beta,
        stable
    }
}
