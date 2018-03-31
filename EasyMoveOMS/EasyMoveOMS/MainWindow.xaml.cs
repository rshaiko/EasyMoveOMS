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
        Database db;

        public MainWindow()
        {
            try
            {
             
                db = new Database();
                InitializeComponent();
                //reloadClientsList();
                chbShowAll.IsChecked = (bool)Settings.Default["showAll"];
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
            lvOrders.ItemsSource = db.GetAllClients();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default["showAll"] = chbShowAll.IsChecked;
            Settings.Default.Save();
        }

        //NEW ORDER
        private void btNew_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow dlg = new OrderWindow();
            if (dlg.ShowDialog() == true)
            {

            }
        }
    }
}
