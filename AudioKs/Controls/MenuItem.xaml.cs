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
    /// Logique d'interaction pour MenuItem.xaml
    /// </summary>
    public partial class MenuItem : UserControl
    {
        public SwitchablePage Page { get; set; }
        public Playlist Playlist { get; set; }
        public ObservableCollection<SubMenuItem> SubMenuItems { get; set; }

        public MenuItem()
        {
            InitializeComponent();
            SubMenuItems = new ObservableCollection<SubMenuItem>();

            DataContext = this;
        }


        #region ----- Dependency -----

        public bool IsCurrentPage
        {
            get { return (bool)GetValue(IsCurrentPageProperty); }
            set { SetValue(IsCurrentPageProperty, value); }
        }
        public static readonly DependencyProperty IsCurrentPageProperty
            = DependencyProperty.Register("IsCurrentPage", typeof(bool), typeof(MenuItem));

        public string ItemName
        {
            get { return (string)GetValue(ItemNameProperty); }
            set { SetValue(ItemNameProperty, value); }
        }
        public static readonly DependencyProperty ItemNameProperty
            = DependencyProperty.Register("ItemName", typeof(string), typeof(MenuItem));

        #endregion


        #region ----- Event -----

        private void Text_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            /*if (Playlist != null)
                MainWindow.instance.SoundBar.LoadPlaylist(Playlist, false);*/
            MainWindow.instance.pageManager.Navigate(Page);
        }

        #endregion
    }
}
