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
    /// Logique d'interaction pour PageHeader.xaml
    /// </summary>
    public partial class PageHeader : UserControl
    {
        public PageHeader()
        {
            InitializeComponent();
            DataContext = this;
        }

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty
            = DependencyProperty.Register("Header", typeof(string), typeof(PageHeader));


        /*
        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public static readonly DependencyProperty IsCheckedProperty
            = DependencyProperty.Register(
                "IsChecked",
                typeof(bool?),
                typeof(ToggleSwitchWPF),
                new FrameworkPropertyMetadata(
                    false,
                    OnIsCheckedChanged
                    )
                );

        private static void OnIsCheckedChanged(DependencyObject src, DependencyPropertyChangedEventArgs e)
        {
            ver switch = src as ToggleSwitchWPF;
            switch.tbnMainToggleButton.IsChecked = e.NewValue as bool?;
        }
        */
    }
}
