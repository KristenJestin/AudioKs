using System;
using System.Collections.Generic;
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
    /// Logique d'interaction pour Menu.xaml
    /// </summary>
    public partial class Menu : UserControl
    {
        private int maxSubItem = 5;

        public Menu()
        {
            InitializeComponent();
        }

        public void LoadMenu()
        {
            // Music Item
            MusicsItem.Page = MainWindow.instance.getMusicPage();
            MusicsItem.Playlist = MainWindow.instance.Playlists.ElementAt(0).Value;

            // Playlist Item
            PlaylistsItem.Page = MainWindow.instance.getPlaylistsPage();
            PlaylistsItem.Playlist = MainWindow.instance.Playlists.ElementAt(0).Value;
            // Sub
            for (int i = 1; i < MainWindow.instance.Playlists.Count; i++)
            {
                Playlist pl = MainWindow.instance.Playlists.ElementAt(i).Value;
                PlaylistsItem.SubMenuItems.Add(new SubMenuItem { ItemName = pl.Name, Playlist = pl });
                if (i == maxSubItem)
                    break;
            }
            
            // Settings Item
            SettingsItem.Page = MainWindow.instance.getSettingsPage();

        }
    }
}
