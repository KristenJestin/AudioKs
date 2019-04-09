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
    /// Logique d'interaction pour MenuItem.xaml
    /// </summary>
    public partial class SubMenuItem : UserControl
    {
        public Playlist Playlist { get; set; }

        public SubMenuItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        public bool IsCurrentPage
        {
            get { return (bool)GetValue(IsCurrentPageProperty); }
            set { SetValue(IsCurrentPageProperty, value); }
        }
        public static readonly DependencyProperty IsCurrentPageProperty
            = DependencyProperty.Register("IsCurrentPage", typeof(bool), typeof(SubMenuItem));

        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }
        public static readonly DependencyProperty ItemNameProperty
            = DependencyProperty.Register("SubItemName", typeof(string), typeof(SubMenuItem));




        private void Text_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.instance.getPlaylistPage().CurrentPlaylist = Playlist;
            MainWindow.instance.pageManager.Navigate(MainWindow.instance.getPlaylistPage());
        }
    }
}
