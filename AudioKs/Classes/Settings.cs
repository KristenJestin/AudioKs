using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AudioKs
{
    public class Settings
    {
        public string Name { get; private set; }
        public string Text { get; set; }
        public JObject Object { get; set; }

        public Settings(string name)
        {
            Name = name;

            // Check if file already exist
            if (!File.Exists(Name))
            {
                File.WriteAllText(Name, "{}");
            }
            Text = File.ReadAllText(Name);

            // Check if the file is correct
            try
            {
                Object = JObject.Parse(Text);
            }
            catch
            {
                File.WriteAllText(Name, "{}");
                Text = "{}";
                Object = JObject.Parse("{}");
            }
        }


        #region ----- Get -----

        public bool IsSet(string name)
        {
            if (!Object.ContainsKey(name))
                return false;
            return true;
        }

        public string GetString(string name, string defaultValue)
        {
            if (!Object.ContainsKey(name))
                return defaultValue;

            return (string)Object[name];
        }

        public int GetInt(string name, int defaultValue)
        {
            if (!Object.ContainsKey(name))
                return defaultValue;

            try
            {
                return (int)Object[name];
            }
            catch
            {
                return defaultValue;
            }
        }

        public double GetDouble(string name, double defaultValue)
        {
            if (!Object.ContainsKey(name))
                return defaultValue;

            try
            {
                return (double)Object[name];
            }
            catch
            {
                return defaultValue;
            }
        }

        public bool GetBool(string name, bool defaultValue)
        {
            if (!Object.ContainsKey(name))
                return defaultValue;

            try
            {
                return (bool)Object[name];
            }
            catch
            {
                return defaultValue;
            }
        }

        #endregion


        #region ----- Set -----

        public void Set(string key, JToken value)
        {
            if (!Object.ContainsKey(key))
                Object.Add(key, value);
            else
                Object[key] = value;
        }

        #endregion


        public void Save()
        {
            Text = Object.ToString(Formatting.Indented);
            File.WriteAllText(Name, Text);
        }





        public static Settings settings
        {
            get
            {
                return new Settings(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AudioKs", "settings.json"));
            }
        }
    }
}
