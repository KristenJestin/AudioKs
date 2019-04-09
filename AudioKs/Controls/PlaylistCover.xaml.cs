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
    /// Logique d'interaction pour PlaylistCover.xaml
    /// </summary>
    public partial class PlaylistCover : UserControl
    {
        public int ID { get; set; }
        public Playlist Playlist { get; set; }

        public PlaylistCover()
        {
            InitializeComponent();
            DataContext = this;
        }


        public BitmapImage Image
        {
            get { return (BitmapImage)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }
        public static readonly DependencyProperty ImageProperty
            = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(PlaylistCover));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register("Title", typeof(string), typeof(PlaylistCover));




        #region ----- Event -----

        private void StackPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MainWindow.instance.getPlaylistPage().CurrentPlaylist = Playlist;
            MainWindow.instance.pageManager.Navigate(MainWindow.instance.getPlaylistPage());
        }

        #endregion
    }
}
