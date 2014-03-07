using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SlkRepair.Properties;

namespace SlkRepair
{
    class Program
    {
        static void Main(string[] args)
        {
            IEnumerable<string> pars = from arg in args where arg.StartsWith("/") select arg.Substring(1);
            IEnumerable<string> files = from arg in args where !arg.StartsWith("/") && File.Exists(arg) select arg;
            foreach (string par in pars)
            {
                string[] kv = par.Split(':');
                switch (kv[0].ToLower())
                {
                    case "convert":
                    case "c":
                        switch (kv[1].ToLower())
                        {
                            case "1":
                            case "yes":
                            case "y":
                                Settings.Default.Convert = true;
                                break;
                            case "0":
                            case "no":
                            case "n":
                                Settings.Default.Convert = false;
                                break;
                        } break;
                }
            }
            foreach (string file in files)
                SlkLib.Liposuct(file);
        }
    }
}
