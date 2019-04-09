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
    public partial class PlaylistNew : UserControl
    {
        public PlaylistNew()
        {
            InitializeComponent();
            DataContext = this;
        }

        #region ----- Event -----

        private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Open new Menu
            MainWindow.instance.NewPlaylistContainer.Visibility = Visibility.Visible;
        }

        #endregion
    }
}
