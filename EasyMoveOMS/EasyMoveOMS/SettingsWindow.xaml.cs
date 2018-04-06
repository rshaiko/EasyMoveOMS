using EasyMoveOMS.Properties;
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
using System.Windows.Shapes;

namespace EasyMoveOMS
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            //newTB.Text=(string) Settings.Default["companyName"];
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            

            if(tbCompany.Text !="")
            Settings.Default["companyName"] = tbCompany.Text;
             if(tbParking.Text != "")
            Settings.Default["truckParking"] = tbParking.Text;
            if (tbPhone.Text != "")
                Settings.Default["phoneNumber"] = tbPhone.Text;
            if (tbProvince.Text != "")
                Settings.Default["province"] = tbProvince.Text;
           if (tbCity.Text != "")
                Settings.Default["city"] = tbCity.Text;


            Settings.Default.Save();
        }
    }
}
