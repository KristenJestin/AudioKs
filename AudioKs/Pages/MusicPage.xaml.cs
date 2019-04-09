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
    public partial class MusicPage : SwitchablePage
    {
        public MusicPage(string name) : base(name)
        {
            InitializeComponent();
        }

        public override void OnSwitch()
        {

        }


        public void LoadContainer()
        {
            DataGrid.MusicContainer.DataContext = MainWindow.instance.Playlists.ElementAt(0).Value;
        }


    }
}
