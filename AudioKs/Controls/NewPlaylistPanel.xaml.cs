using Microsoft.Win32;
using SQLiteNetExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Logique d'interaction pour NewPlaylistPanel.xaml
    /// </summary>
    public partial class NewPlaylistPanel : UserControl
    {
        private string ImagePath { get; set; }

        public NewPlaylistPanel()
        {
            InitializeComponent();
        }

        #region ----- Function -----

        private void ResetImage()
        {
            Vignette.Source = FindResource("DefaultCoverImage") as BitmapImage;
        }

        #endregion


        #region ----- Event -----

        private void Vignette_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                ImagePath = openFileDialog.FileName;
                Vignette.Source = new BitmapImage(new Uri(ImagePath));
            }
        }

        private void Validate_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(Name.Text))
            {
                MessageBox.Show(FindResource("new-playlist-name-incorrect").ToString(), "Alert", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            MainWindow.instance.CloseNewPlaylistPanel();

            // Create new playlist
            Playlist playlist = new Playlist
            {
                Name = Name.Text,
                ImagePath = ImagePath,
                Audios = new ObservableCollection<Audio>(),
                Random = false,
                Repeat = false
            };
            try
            {
                MainWindow.instance.SQLite.InsertWithChildren(playlist);
            }
            catch
            {
                MessageBox.Show("Error", "Alert", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Add new playlist in playlists page
            MainWindow.instance.AddPlaylist(playlist);
            // Reset Panel
            ResetImage();
            Name.Text = "";
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.instance.CloseNewPlaylistPanel();
        }

        #endregion
    }
}
