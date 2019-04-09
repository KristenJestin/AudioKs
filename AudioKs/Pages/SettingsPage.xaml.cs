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
    /// Logique d'interaction pour SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : SwitchablePage
    {
        public SettingsPage(string name) : base(name)
        {
            InitializeComponent();
        }

        public override void OnSwitch()
        {

        }


        public void Load()
        {
            // Lang
            string languageCode = App.instance.settings.GetString("Lang", "en");
            if (!App.instance.availableLang.Where(o => string.Equals(languageCode, o, StringComparison.OrdinalIgnoreCase)).Any())
                languageCode = "en";

            int index = App.instance.availableLang.IndexOf(languageCode);
            LangBox.SelectedIndex = index;

            //Auto Update
            AutoUpdate.IsChecked = App.instance.settings.GetBool("Auto_Update", true);
        }



        #region ----- Event -----

        private void LangBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string lang = ((FrameworkElement)LangBox.SelectedItem).Uid;

            App.instance.settings.Set("Lang", lang);
            App.instance.SetLanguage(MainWindow.instance, lang);
        }

        private void AutoUpdate_Checked(object sender, RoutedEventArgs e)
        {
            bool autoUpdate = AutoUpdate.IsChecked ?? false;
            App.instance.settings.Set("Auto_Update", autoUpdate);
        }

        #endregion
    }
}
