using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
using EasyMoveOMS.Properties;



namespace EasyMoveOMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //public static Database db;
        List<ListOrderItem> orderList = new List<ListOrderItem>();

        public MainWindow()
        {
            try
            {
             
                
                InitializeComponent();
                Globals.truckList = Globals.db.GetWorkingTrucks();

                Globals.db.reloadOrderList(ref orderList);
                lvOrders.ItemsSource = orderList;

                //reloadClientsList();
                chbShowAll.IsChecked = (bool)Settings.Default["showAll"];

                //Rom@
                

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.StackTrace);
                MessageBox.Show("Error opening database connection: " + e.Message);
                Environment.Exit(1);
            }


            
             
                /*
             InvoiceWindow dlg1 = new InvoiceWindow();
             if (dlg1.ShowDialog() == true)
             {

             }*/
        }

        private void reloadClientsList()
        {
            lvOrders.ItemsSource = Globals.db.GetAllClients();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default["showAll"] = chbShowAll.IsChecked;
            Settings.Default.Save();
        }

        //NEW ORDER
        private void btNew_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow dlg = new OrderWindow(null);
            if (dlg.ShowDialog() == true)
            {

            }
        }

        

        private void mSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow dlg = new SettingsWindow();
            if (dlg.ShowDialog() == true)
            {

            }

        }

        private void lvOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvOrders.SelectedIndex == -1) return;
            ListOrderItem loi = (ListOrderItem)lvOrders.SelectedItem;
            Order o = new Order { id = loi.id };
            OrderWindow dlg = new OrderWindow(o);
            if (dlg.ShowDialog() == true)
            {

            }
        }
    }
}
