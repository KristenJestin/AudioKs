using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AudioKs
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static App instance = null;
        public string APP_UID = "VaP4r9QNbCsbfMv";

        public string APP_PATH = AppDomain.CurrentDomain.BaseDirectory;
        public string APP_NAME = "AudioKs";

        public List<string> availableLang = new List<string> { "en", "fr" };
        public Settings settings { get; set; }
        public string DataDir { get; set; }

        private StartWindow StartWin;
        private MainWindow MainWin;
        private Thread thread;
        private DispatcherTimer timer;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            instance = this;

            // Create folder in AppData
            DataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_NAME);
            Directory.CreateDirectory(DataDir);
            settings = new Settings(Path.Combine(DataDir, "settings.json"));
            // Lang
            if (!settings.IsSet("Lang"))
            {
                string languageCode = Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName;
                if (!availableLang.Where(o => string.Equals(languageCode, o, StringComparison.OrdinalIgnoreCase)).Any())
                    settings.Set("Lang", "en");
                else
                    settings.Set("Lang", languageCode);
            }

            // Timer            
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(800);
            timer.Tick += WaitForMain;


            // Define Window
            StartWin = new StartWindow();
            MainWin = new MainWindow();

            // Lang
            SetLanguage(StartWin, settings.GetString("Lang", "en"));
            SetLanguage(MainWin, settings.GetString("Lang", "en"));

            // Display Start Window
            StartWin.Show();
            //MainWin.Show();

        }


        #region ----- Functions -----

        public void CloseApp()
        {
            Dispatcher.Invoke(() => Current.Shutdown());
        }

        public void StartMainWin()
        {
            // Starting new thread for loading window
            thread = new Thread(() =>
            {
                MainWin = new MainWindow();
                MainWin.Show();
                Dispatcher.Run();
            })
            { IsBackground = true };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            // Wait until is loaded
            timer.Start();
        }

        // Lang
        public void SetLanguage(Window win, string lang)
        {
            string path = "Dictionaries/Lang/"; // Default files path            

            List<ResourceDictionary> dicToRemove = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Resources.MergedDictionaries)
                if (dictionary.Source.ToString().Contains(@"/Strings.") == true)
                    dicToRemove.Add(dictionary);

            foreach (ResourceDictionary dictionary in dicToRemove)
                Resources.MergedDictionaries.Remove(dictionary);

            var languageDictionary = new ResourceDictionary()
            {
                Source = new Uri($"{path}Strings.{lang}.xaml", UriKind.Relative)
            };

            // Set dictionnaries
            Resources.MergedDictionaries.Add(languageDictionary);
            RedefineResourcesDict(win, Resources.MergedDictionaries);
        }
        public void RedefineResourcesDict(Window win, Collection<ResourceDictionary> mergedDict = null)
        {
            if (mergedDict == null)
                mergedDict = win.Resources.MergedDictionaries;

            win.Resources.MergedDictionaries.Clear();
            foreach (ResourceDictionary dict in mergedDict)
                win.Resources.MergedDictionaries.Add(dict);
        }

        #endregion

        #region ----- Event -----

        // Waiter
        private void WaitForMain(object sender, EventArgs e)
        {
            if (MainWin == null) return;

            if (MainWin.loaded)
            {
                Current.MainWindow = MainWin;
                StartWin.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(StartWin.Close));
            }
        }

        #endregion
    }
}
