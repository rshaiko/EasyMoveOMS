using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public MainWindow()
        {
            try
            {
             
                
                InitializeComponent();
                chbShowAll.IsChecked = (bool)Settings.Default["showAll"];

                Globals.truckList = Globals.db.GetWorkingTrucks();
                if (chbShowAll.IsChecked.Value)
                {
                    Globals.db.reloadOrderList(ref orderList);
                    lvOrders.ItemsSource = orderList;
                }
                else
                {
                    Globals.db.reloadOrderListScheduled(ref orderList);
                    lvOrders.ItemsSource = orderList;
                }

                //reloadClientsList();
                

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

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<ListOrderItem> ListOrderItem = orderList;        
            String word = tbSearch.Text;
            if (word != "")
            {
                var result = (from o in ListOrderItem where o.name.Contains(word) || o.dateTime.Contains(word)
                             || o.addrLine.Contains(word) || o.phones.Contains(word) || (o.orderStatus.ToString().Contains(word))
                              select o );
                ListOrderItem = result.ToList();

                
            }
            lvOrders.ItemsSource = ListOrderItem;
        }

        private void btSort_Click(object sender, RoutedEventArgs e)
        {
            SortDialog dlg = new SortDialog( orderList, lvOrders);
            if (dlg.ShowDialog() == true)
            {

            }
        }

        private void chbShowAll_Unchecked(object sender, RoutedEventArgs e)
        {
            Globals.db.reloadOrderListScheduled(ref orderList);
            lvOrders.ItemsSource = orderList;
        }

        private void chbShowAll_Checked(object sender, RoutedEventArgs e)
        {
            Globals.db.reloadOrderList(ref orderList);
            lvOrders.ItemsSource = orderList;
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                lvOrders.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            lvOrders.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
        public class SortAdorner : Adorner
        {
            private static Geometry ascGeometry =
                    Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

            private static Geometry descGeometry =
                    Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

            public ListSortDirection Direction { get; private set; }

            public SortAdorner(UIElement element, ListSortDirection dir)
                    : base(element)
            {
                this.Direction = dir;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                if (AdornedElement.RenderSize.Width < 20)
                    return;

                TranslateTransform transform = new TranslateTransform
                        (
                                AdornedElement.RenderSize.Width - 15,
                                (AdornedElement.RenderSize.Height - 5) / 2
                        );
                drawingContext.PushTransform(transform);

                Geometry geometry = ascGeometry;
                if (this.Direction == ListSortDirection.Descending)
                    geometry = descGeometry;
                drawingContext.DrawGeometry(Brushes.Black, null, geometry);

                drawingContext.Pop();
            }
        }
    }
}
