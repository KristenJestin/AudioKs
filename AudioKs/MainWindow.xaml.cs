using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace AudioKs
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow instance = null;
        public PageManager pageManager;
        public bool loaded = false;

        private string[] allowedExtensions = { ".wav", ".mid", ".midi", ".wma", ".mp3", ".ogg", ".rma", };

        // Save
        public SQLiteConnection SQLite { get; set; }

        // Playlist
        public Dictionary<int, Playlist> Playlists { get; set; }

        // Menu
        private bool menuDisplayed;
        private Storyboard LeftMenuShow, LeftMenuHide, MenuImageIn, MenuImageOut;
        private BitmapImage LeftMenuOpen, LeftMenuClose;

        // Server
        public ServerManager Server { get; set; }


        public MainWindow()
        {
            InitializeComponent();
            instance = this;

            // Define min Size and set Size for AllowTransparency
            WinResize.minSize = new WinResize.POINT { x = 800, y = 500 };
            SourceInitialized += (s, e) =>
            {
                IntPtr handle = (new WindowInteropHelper(this)).Handle;
                HwndSource.FromHwnd(handle).AddHook(new HwndSourceHook(WinResize.WindowProc));
            };

            Initilisation();

            loaded = true;
        }

        private void Initilisation()
        {
            // Initialise SQLite
            SQLite = new SQLiteConnection(Path.Combine(App.instance.DataDir, "data.db3"));
            SQLite.CreateTable<PlaylistAsso>();
            SQLite.CreateTable<Audio>();
            SQLite.CreateTable<Playlist>();

            // Page Manager
            pageManager = new PageManager(PageHolder);
            pageManager.Add(AvailablePage.Music, new MusicPage("MUSIC"));
            pageManager.Add(AvailablePage.Playlists, new PlaylistsPage("PLAYLISTS"));
            pageManager.Add(AvailablePage.Playlist, new PlaylistPage("PLAYLIST"));
            pageManager.Add(AvailablePage.Settings, new SettingsPage("SETTINGS"));
            pageManager.Navigate(0);
                                          
            // Check Music folder
            List<Audio> audios = SQLite.Table<Audio>().ToList();
            foreach (string filePath in GetFolderFilesPath(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)))
                if (IsAudioFile(filePath) && !audios.Any(a => a.Path == filePath))
                    new Audio { Path = filePath }.Insert();

            // Playlists
            Playlists = new Dictionary<int, Playlist>();
            // Add default playlist
            Playlists.Add(0, DefaultPlaylist(SQLite.Table<Audio>().ToList()));
            // And all other Playlist
            List<Playlist> otherPlaylist = SQLite.GetAllWithChildren<Playlist>();
            // Add in playlist page
            foreach (Playlist pl in otherPlaylist)
                AddPlaylist(pl);

            // Sound Bar
            SoundBar.Main = this;
            SoundBar.OnMainLoaded();

            getSettingsPage().Load();

            //Add item in menu
            getMusicPage().LoadContainer();
            LeftMenu.LoadMenu();


            // Menu
            LeftMenuShow = FindResource("SbShowMenu") as Storyboard;
            LeftMenuHide = FindResource("SbHideMenu") as Storyboard;
            MenuImageIn = FindResource("SbRotateIn") as Storyboard;
            MenuImageOut = FindResource("SbRotateOut") as Storyboard;
            LeftMenuOpen = FindResource("MenuOpenImage") as BitmapImage;
            LeftMenuClose = FindResource("MenuCloseImage") as BitmapImage;
            StartedMenuPosition(App.instance.settings.GetBool("Menu_Open", true));

            // TEST
            Playlist plt = new Playlist
            {
                Audios = null,
                Name = "Harry Potter",
                ImagePath = @"C:\Users\KriS\Pictures\Fond d'écran\home.jpg",
                Random = true,
                Repeat = false
            };
            //SQLite.InsertWithChildren(plt);

            // Initialise Server
            Server = new ServerManager("", 9876);
            if (Server.IsEnabled)
            {
                Server.DataReceived += Server_DataReceived;
                Server.ClientConnected += Server_ClientConnected;
                Server.ClientDisconnected += Server_ClientDisconnected;
                Connected.Content = Server.ConnectedClientsCount + " CONNECTÉ(S) - " + Server.GetLocalIPAddress().ToString();
            }

            // Set default playlist to music page
            SoundBar.LoadPlaylist(Playlists.ElementAt(0).Value, false);
        }




        #region ----- Function -----

        // Playlist
        private Playlist DefaultPlaylist(List<Audio> audios)
        {
            Playlist playlist = new Playlist
            {
                ID = -1,
                Name = "Default-PL",
                Audios = new ObservableCollection<Audio>(),
                Random = false,
                Repeat = false
            };

            if (audios.Count > 0)
                foreach (Audio audio in audios)
                    playlist.AddAudio(audio.Path);

            return playlist;
        }

        public void AddPlaylist(Playlist playlist)
        {
            playlist.Load();

            PlaylistCover cover = new PlaylistCover
            {
                Title = playlist.Name,
                Image = playlist.GetBitmapImage(),
                ID = playlist.ID,
                Playlist = playlist
            };
            getPlaylistsPage().PlContainer.Container.Children.Add(cover);

            Playlists.Add(playlist.ID, playlist);
        }

        // Other
        public MusicPage getMusicPage()
        {
            return pageManager.pages[AvailablePage.Music] as MusicPage;
        }
        public PlaylistsPage getPlaylistsPage()
        {
            return pageManager.pages[AvailablePage.Playlists] as PlaylistsPage;
        }
        public PlaylistPage getPlaylistPage()
        {
            return pageManager.pages[AvailablePage.Playlist] as PlaylistPage;
        }
        public SettingsPage getSettingsPage()
        {
            return pageManager.pages[AvailablePage.Settings] as SettingsPage;
        }

        public bool IsAudioFile(string path)
        {
            if (!File.Exists(path)) return false;
            return -1 != Array.IndexOf(allowedExtensions, System.IO.Path.GetExtension(path).ToLowerInvariant());
        }

        public void UpdateDataContext()
        {
            getMusicPage().DataGrid.MusicContainer.Items.Refresh();
            getPlaylistPage().DataGrid.MusicContainer.Items.Refresh();
        }

        private void StartedMenuPosition(bool actived)
        {
            menuDisplayed = actived;
            double leftMenuWidth = (double)(FindResource("LeftMenu-Width") as double?);

            if (actived)            
                LeftMenu.Margin = new Thickness(0);            
            else            
                LeftMenu.Margin = new Thickness(-leftMenuWidth, 0, 0, 0);            

            MenuImageTransform.Angle = actived ? 0 : 180;
        }

        private void ActiveMenu()
        {
            Storyboard sb2 = menuDisplayed ? MenuImageIn : MenuImageOut;
            sb2.Begin(MenuImage, true);

            Storyboard sb = menuDisplayed ? LeftMenuHide : LeftMenuShow;
            sb.Begin(LeftMenu, true);

            menuDisplayed = !menuDisplayed;
            //MenuImage.Source = menuDisplayed ? LeftMenuClose : LeftMenuOpen;
        }

        public int Modulo(int x, int m)
        {
            int r = x % m;
            return r < 0 ? r + m : r;
        }

        public List<string> GetFolderFilesPath(string path)
        {
            List<string> paths = new List<string>();
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            FileInfo[] info = dirInfo.GetFiles("*.*");
            foreach (FileInfo f in info)
            {
                paths.Add(f.FullName);
            }
            DirectoryInfo[] subDirectories = dirInfo.GetDirectories();
            foreach (DirectoryInfo directory in subDirectories)
            {
                paths.AddRange(GetFolderFilesPath(directory.FullName));
            }

            return paths;
        }

        // New Playlist Panel
        public void CloseNewPlaylistPanel()
        {
            NewPlaylistContainer.Visibility = Visibility.Collapsed;
        }

        #endregion




        #region ----- Event -----

        // Click
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            ActiveMenu();
        }

        // New Playlist Panel
        private void NewPlaylistContainer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!((FrameworkElement)e.OriginalSource).Name.Equals("NewPlaylistContainer")) return;
            //CloseNewPlaylistPanel();
        }

        #endregion




        #region ----- Window -----

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            App.instance.CloseApp();
        }

        private void WinSize_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;

            else if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;

        }

        private void PlayThumbButton_Click(object sender, EventArgs e)
        {
            SoundBar.Play_Click(sender, new RoutedEventArgs());
        }

        private void Minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WinSizeImage.Source = FindResource("SmallImage") as BitmapImage;

            else if (WindowState == WindowState.Normal)
                WinSizeImage.Source = FindResource("FullImage") as BitmapImage;
        }

        public void Window_Closing(object sender, CancelEventArgs e)
        {
            App.instance.settings.Set("Menu_Open", menuDisplayed);
            App.instance.settings.Save();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }

        #endregion



        #region ----- Server -----

        private void Server_ClientConnected(object sender, TcpClient client)
        {
            Dispatcher.Invoke(() => Connected.Content = Server.ConnectedClientsCount + " CONNECTÉ(S) - " + Server.GetLocalIPAddress().ToString());
            Server.Clients.Add(client);
        }

        private void Server_ClientDisconnected(object sender, TcpClient client)
        {
            Dispatcher.Invoke(() => Connected.Content = Server.ConnectedClientsCount + " CONNECTÉ(S) - " + Server.GetLocalIPAddress().ToString());
            Server.Clients.Remove(client);
        }

        private void Server_DataReceived(object sender, SimpleTCP.Message e)
        {
            string received = e.MessageString;
            string respond = "";
            TcpClient client = e.TcpClient;
            Dispatcher.Invoke(() => respond = ReceivedMessage(client, received));

            if (!String.IsNullOrWhiteSpace(respond))
            {
                e.ReplyLine(respond + "\n");
            }
            else
            {
                e.ReplyLine("RECEIVED" + "\n");
            }

            // Validation
            /*if (server.Clients[client].isValidate() && !server.Clients[client].Sended_Playlist)
            {
                server.Clients[client].Sended_Playlist = true;
                if (currentPlaylist != -1)
                {
                    Playlist pl = FindPlaylist(currentPlaylist);
                    e.ReplyLine("PLAYLIST=" + server.GetJson(pl, pl.getIndexOfKey(currentAudio)) + "\n");
                }
            }*/
        }

        private string ReceivedMessage(TcpClient tcpClient, string text)
        {
            // Create valide Android Client
            /*if (text.StartsWith("PASSWORD"))
            {
                string password = DecryptMessage(text);
                if (server.Password == password)
                {
                    client.Validade_Password = true;
                    return "PASSWORD VALIDATE";
                }
                else
                {
                    return "KICK=WRONG PASSWORD";
                }
            }
            else if (text.StartsWith("VERSION"))
            {
                float version = 0.0f;
                bool hasVersion = float.TryParse(DecryptMessage(text).Replace(".", ","), out version);
                if (version != 0.0f && server.allowedVersion.IsValid(version))
                {
                    client.Validade_Version = true;
                    return "VERSION VALIDATE";
                }
                else
                {
                    return "KICK=WRONG VERSION";
                }
            }


            // Check if Android Client has been validate
            if (!client.isValidate())
            {
                return "KICK=NOT VALIDATE";
            }*/
            if(text == "HELLO")
            {
                // Send to Server
                Server.SendMessage("PLAYLIST=" + Server.GetJson(SoundBar.CurrentPlaylist, SoundBar.CurrentPlaylist.getIndexOfKey(SoundBar.CurrentAudio)));

                List<PlaylistView> tempPL = new List<PlaylistView>();
                foreach(Playlist pl in Playlists.Values)
                {
                    tempPL.Add(Server.GetPlView(pl));
                }

                Server.SendMessage("NPLAYLISTS=" + JsonConvert.SerializeObject(tempPL));
            }
            else if (text.StartsWith("PLAYLIST")){
                string plName = DecryptMessage(text);
                int position = -1;
                foreach(Playlist pl in Playlists.Values)
                {
                    if (pl.Name == plName) {
                        position = pl.ID;
                        break;
                    }
                }

                if (position != -1)
                {
                    getPlaylistPage().CurrentPlaylist = Playlists[position];
                    pageManager.Navigate(getPlaylistPage());
                    SoundBar.LoadPlaylist(Playlists[position], false);                    
                }
                else
                {
                    pageManager.Navigate(getMusicPage());
                    SoundBar.LoadPlaylist(Playlists[0], false);
                }
            }

            // Apply Android Client action
            else if (SoundBar.mediaPlayer.Source != null)
            {
                if (text.StartsWith("PLAY"))
                {
                    int position = Convert.ToInt32(DecryptMessage(text));
                    SoundBar.LoadAudio(position, true);
                }
                else if (text.Equals("INVERT"))
                {
                    SoundBar.ChangeMediaState();
                }
                else if (text.Equals("PREVIOUS"))
                {
                    SoundBar.ChangeAudio(false);
                }
                else if (text.Equals("NEXT"))
                {
                    SoundBar.ChangeAudio(true);
                }
                else if (text.StartsWith("TIME"))
                {
                    double progess = Convert.ToInt32(DecryptMessage(text));
                    SoundBar.ChangeAudioPosition(progess);
                }
                else if (text.Equals("VOLUME_UP"))
                {
                    SoundBar.VolumeSlider.Value += 0.1;
                }
                else if (text.Equals("VOLUME_DOWN"))
                {
                    SoundBar.VolumeSlider.Value -= 0.1;
                }
            }

            // If nothing specific to respond
            return null;
        }

        private string DecryptMessage(string text, char separator = '=')
        {
            string[] split = text.Split(separator);

            if (split.Count() > 1)
            {
                string value = "";
                for (int i = 0; i < split.Count(); i++)
                {
                    if (i != 0)
                        value = split[i];
                }

                return value;
            }

            return "ERROR";
        }

        #endregion
    }


    public partial class PageManager
    {
        private Frame holder;
        public SwitchablePage CurrentPage { get; set; }
        public Dictionary<AvailablePage, SwitchablePage> pages;


        public PageManager(Frame holder)
        {
            this.holder = holder;
            pages = new Dictionary<AvailablePage, SwitchablePage>();
        }

        public void Add(AvailablePage type, SwitchablePage page)
        {
            pages.Add(type, page);
        }

        public void Navigate(SwitchablePage page)
        {
            NavigateAction(page);
        }
        public void Navigate(AvailablePage type)
        {
            SwitchablePage page = pages[type];
            NavigateAction(page);
        }

        private void NavigateAction(SwitchablePage page)
        {
            if (page == null) return;

            holder.Navigate(page);
            page.OnSwitch();
            CurrentPage = page;
        }
    }

    public enum AvailablePage
    {
        Music,
        Playlists,
        Playlist,
        Settings
    }
}
