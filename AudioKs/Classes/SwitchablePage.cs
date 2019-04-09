using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AudioKs
{
    public class SwitchablePage : Page
    {
        public new string Name { get; set; }

        public SwitchablePage(string name)
        {
             /*DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchablePage),
                 new FrameworkPropertyMetadata(typeof(SwitchablePage)));*/

            Name = name;
        }

        public virtual void OnSwitch()
        {

        }
    }
}
