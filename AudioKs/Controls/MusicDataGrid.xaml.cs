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
    /// Logique d'interaction pour MusicDataGrid.xaml
    /// </summary>
    public partial class MusicDataGrid : UserControl
    {
        public MusicDataGrid()
        {
            InitializeComponent();
        }


        #region ----- Event -----

        // Music Container
        private void MusicContainer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            /*
            Grid grid = (Grid)sender;

            if (grid.Uid != "-1")
            {
                int id = Convert.ToInt32(grid.Uid);

                // Change visibility
                ShowMusicPlaylist(true);

                Playlist playlist = FindPlaylist(id);
                if (playlist != null)
                    LoadPlaylist(playlist, false);
            }
            else
            {
                CreatePlaylistBackgroundPanel.Visibility = Visibility.Visible;
            }*/
        }

        private void MusicContainerRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Audio audio = (sender as DataGridRow).Item as Audio;
            if (audio != null)
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    /*if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                    {
                        RepeatCheckbox.IsChecked = true;
                    }*/

                    Playlist CurrentPlaylist = MusicContainer.DataContext as Playlist;
                    if (CurrentPlaylist != MainWindow.instance.SoundBar.CurrentPlaylist)
                    {
                        MainWindow.instance.SoundBar.LoadPlaylist(CurrentPlaylist, false);
                    }
                    MainWindow.instance.SoundBar.PlayAudio(audio);
                    MainWindow.instance.getPlaylistPage().setMediaState();

                }
                else if (e.ChangedButton == MouseButton.Right)
                {
                    string argument = "/select, \"" + audio.Path + "\"";
                    Process.Start("explorer.exe", argument);
                }
            }
        }

        private void MusicContainer_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string path in files)
                {
                    FileAttributes attr = File.GetAttributes(path);
                    bool isFolder = (attr & FileAttributes.Directory) == FileAttributes.Directory;

                    if (isFolder)
                        foreach (string filePath in MainWindow.instance.GetFolderFilesPath(path))
                        {
                            MainWindow.instance.getPlaylistPage().CurrentPlaylist.AddAudio(filePath);
                        }
                    else
                    {
                        MainWindow.instance.getPlaylistPage().CurrentPlaylist.AddAudio(path);
                    }
                }
            }
        }

        private void MusicContainer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                /*
                Audio audio = MusicContainer.SelectedItem as Audio;
                if (audio != null)
                {
                    Playlist pl = FindPlaylist(currentPlaylist);
                    pl.Remove(audio);
                    // Send to clients
                    server.SendMessage("REM_AUDIO=" + server.GetJson(audio));

                    if (audio == currentAudio)
                        SwitchAudio();
                }
                */
            }
        }

        #endregion
    }
}
