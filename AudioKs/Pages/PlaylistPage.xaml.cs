using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioKs
{
    /// <summary>
    /// Logique d'interaction pour MusicPage.xaml
    /// </summary>
    public partial class PlaylistPage : SwitchablePage
    {
        public Playlist CurrentPlaylist { get; set; }

        public PlaylistPage(string name) : base(name)
        {
            InitializeComponent();
            DataContext = this;
        }

        public override void OnSwitch()
        {
            MainWindow.instance.getPlaylistPage().DataGrid.MusicContainer.DataContext = CurrentPlaylist;

            PlName.Text = CurrentPlaylist.Name;
            setMediaState();

            if (CurrentPlaylist.GetBitmapImage() != null)
                PlCover.Source = CurrentPlaylist.GetBitmapImage();
            else
                PlCover.Source = FindResource("DefaultCoverImage") as BitmapImage;
        }



        #region ----- Function -----

        public void setMediaState()
        {
            SoundBar bar = MainWindow.instance.SoundBar;
            if (bar.Playing == true && bar.CurrentPlaylist == CurrentPlaylist)
            {
                PlayImage.Source = FindResource("PauseImage") as BitmapImage;
            }
            else
            {
                PlayImage.Source = FindResource("PlayImage") as BitmapImage;
            }
        }

        #endregion


        #region ----- Event -----

        // Click
        public void Play_Click(object sender, RoutedEventArgs e)
        {
            bool good = true;
            if (CurrentPlaylist != MainWindow.instance.SoundBar.CurrentPlaylist)
            {
                good = MainWindow.instance.SoundBar.LoadPlaylist(CurrentPlaylist, false);
            }

            if (good)
            {
                MainWindow.instance.SoundBar.Play_Click(sender, e);
                setMediaState();
            }
        }

        #endregion
    }
}
