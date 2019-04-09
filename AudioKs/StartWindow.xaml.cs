using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AudioKs
{
    /// <summary>
    /// Logique d'interaction pour StartWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        public static StartWindow instance = null;
        private const string SERVER_NAME = @"http://api.kristenjestin.com/update.php?uid=";
        //private const string SERVER_NAME = @"http://127.0.0.1:8080/edsa-API_CzK2/update.php?uid=";

        private VersionType versionType { get; set; }
        private string versionTypeUid { get; set; }

        private Appli onlineApp;
        private List<AppFile> fileToDownload;


        public StartWindow()
        {
            InitializeComponent();
            instance = this;

            // File
            fileToDownload = new List<AppFile>();
        }

        // MAKE AAPP MANAGE THIS

        #region ----- Function -----

        // Element
        private void ChangeMainText(string text)
        {
            MainText.Text = text;
        }


        // App
        private async void CheckingUpdate()
        {
            if (await NeedUpdate() && File.Exists(Path.Combine(App.instance.APP_PATH, "Updater.exe")))
            {
                // Create jsonfile to download files
                object jsonFile = new {
                    NAME = onlineApp.NAME,
                    VERSION = onlineApp.VERSION,
                    FULL = onlineApp.FULL,
                    FILES = fileToDownload
                };

                string fileJSON = JsonConvert.SerializeObject(jsonFile, Formatting.Indented);
                string fileName = Path.Combine(Path.GetTempPath(), App.instance.APP_UID);
                File.WriteAllText(fileName, fileJSON);

                // Launch Updater
                LaunchUpdater();
            }
            else
            {
                ChangeMainText(FindResource("startup-starting") as string);
                App.instance.StartMainWin();
            }
        }

        private VersionType getNeededVersion()
        {
            foreach (VersionType type in Enum.GetValues(typeof(VersionType)))
            {
                string path = Path.Combine(App.instance.APP_PATH, type.ToString());
                if (File.Exists(path))
                {
                    versionType = type;
                    versionTypeUid = File.ReadAllText(path);
                    return versionType;
                }
            }

            versionTypeUid = "";
            return versionType = VersionType.stable;
        }

        private async Task<bool> NeedUpdate()
        {
            // Check if user have internet and if he want auto update
            if (HaveInternet() && App.instance.settings.GetBool("Auto_Update", true))
            {
                string url = getInfoURL();
                // Check if URL Exist
                if (await URLExist(url))
                {
                    string json = await GetJSON(url);
                    // Check if App exist in API
                    if (json != null && !json.Contains("ERROR"))
                    {
                        // Compare local and online version
                        onlineApp = JsonConvert.DeserializeObject<Appli>(json);
                        if (onlineApp.FULL)
                            CheckFileFull(onlineApp);
                        else
                            CheckFile(onlineApp);

                        // Check if need to launch Updater
                        if (fileToDownload.Count > 0)
                            return true;
                    }
                }
            }

            return false;
        }

        private void CheckFile(Appli onlineApp)
        {
            foreach (AppFile onlineFile in onlineApp.FILES)
            {
                if (onlineFile.PATH != null) onlineFile.PATH = onlineFile.PATH + "\\";
                string path = App.instance.APP_PATH + onlineFile.PATH + onlineFile.NAME;
                bool needUpdate = false;
                if (File.Exists(path))
                {
                    switch (Path.GetExtension(path))
                    {
                        case ".exe":
                            App.instance.APP_NAME = Path.GetFileNameWithoutExtension(onlineFile.NAME);
                            if (!File.Exists(App.instance.APP_PATH + "version") || GetVersion() != onlineApp.VERSION)
                                needUpdate = true;
                            break;

                        default:
                            if (GetVersionStr(onlineFile.PATH + onlineFile.NAME) != onlineFile.VERSION)
                                needUpdate = true;
                            break;
                    }
                }
                else
                    needUpdate = true;


                if (needUpdate)
                    fileToDownload.Add(onlineFile);
            }
        }
        private void CheckFileFull(Appli onlineApp)
        {
            if (!File.Exists(App.instance.APP_PATH + "version") || GetVersion() != onlineApp.VERSION)
                fileToDownload = onlineApp.FILES;
        }



        // Version
        private float GetVersion()
        {
            float version = 0.0f;
            if (float.TryParse(File.ReadAllText(App.instance.APP_PATH + "version"), out version))
                return version;

            return 0.0f;
        }
        private float CreateVersion(string version)
        {
            String[] number = version.Split('.');

            return float.Parse(number[0] + "," + number[1]);
        }

        private string GetVersionStr(string fileName)
        {
            FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(App.instance.APP_PATH + "\\" + fileName);

            if (String.IsNullOrWhiteSpace(fileInfo.FileVersion) && String.IsNullOrWhiteSpace(fileInfo.ProductVersion))
                return "0.0.0";
            else if (String.IsNullOrWhiteSpace(fileInfo.ProductVersion))
                return CreateVersionStr(fileInfo.FileVersion);
            else
                return CreateVersionStr(fileInfo.ProductVersion);
        }
        private string CreateVersionStr(string version)
        {
            String[] number = version.Split('.');

            return number[0] + "." + number[1] + "." + number[2];
        }


        // Internet
        private bool HaveInternet()
        {
            return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
        }

        private async Task<bool> URLExist(string url)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 2500;
                WebResponse response = await Task.Run(() => request.GetResponse());
                return true;
            }
            catch { }
            return false;
        }

        private string getInfoURL()
        {
            string URL = SERVER_NAME + App.instance.APP_UID;
            if (versionType != VersionType.stable)
                URL += "&type=" + versionTypeUid;
            return URL;
        }

        private async Task<string> GetJSON(string url)
        {
            // Get Json
            WebRequest request = WebRequest.Create(url);
            request.Timeout = 2500; // miliseconds
            WebResponse response = null;

            try
            {
                response = await Task.Run(() => request.GetResponse());
                StreamReader readerOnline = new StreamReader(response.GetResponseStream());
                return readerOnline.ReadToEnd();
            }
            catch
            {
                return null;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }


        // Other
        private void LaunchUpdater()
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(App.instance.APP_PATH + "Updater.exe");
                processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                processStartInfo.UseShellExecute = true;
                processStartInfo.Verb = "runas";

                Process.Start(processStartInfo);
            }
            catch {}
            finally
            {
                Application.Current.Shutdown();
            }
        }

        #endregion


        #region ----- Event -----


        // Window
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            // Checking type needed : alpha, beta, ... And start online checking
            getNeededVersion();
            CheckingUpdate();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

        }

        #endregion


    }
}
