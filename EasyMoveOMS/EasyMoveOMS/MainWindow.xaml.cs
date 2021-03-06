﻿using System;
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
                //DataContext = this;

                //Call Database every 3 minutes to keep connection alive
                var startTimeSpan = TimeSpan.Zero;
                var periodTimeSpan = TimeSpan.FromMinutes(3);

                var timer = new System.Threading.Timer((e) =>
                {
                    try
                    {
                        Globals.db = new Database();
                    }
                    catch (SqlException exx)
                    {
                        MessageBox.Show("Error opening database connection: " + exx.Message);
                        Environment.Exit(1);
                    }
                }, null, startTimeSpan, periodTimeSpan);


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
                lblStatus.Text = "Total number of orders: "+ orderList.Count;
            }
            catch (SqlException e)
            {
                MessageBox.Show("Error opening database connection: " + e.Message);
                Environment.Exit(1);
            }
        }

        private void refreshOrderWindow()
        {
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
            lblStatus.Text = "Total number of orders: " + orderList.Count;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Settings.Default["showAll"] = chbShowAll.IsChecked;
            Settings.Default.Save();
        }

        //NEW ORDER
        private void btNew_Click(object sender, RoutedEventArgs e)
        {
            createNewOrder();
        }

        private void createNewOrder()
        {
            OrderWindow dlg = new OrderWindow(null);
            if (dlg.ShowDialog() == true)
            {
                refreshOrderWindow();
            }
            else
            {
                refreshOrderWindow();
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
            editCurrentOrder();
        }

        private void editCurrentOrder()
        {
            if (lvOrders.SelectedIndex == -1) return;
            ListOrderItem loi = (ListOrderItem)lvOrders.SelectedItem;
            Order o = new Order { id = loi.id };
            OrderWindow dlg = new OrderWindow(o);
            if (dlg.ShowDialog() == true)
            {
                refreshOrderWindow();
            }
            else
            {
                refreshOrderWindow();
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<ListOrderItem> ListOrderItem = orderList;        
            String word = tbSearch.Text;
            if (word != "")
            {
                var result = (from o in ListOrderItem where (o.name).ToLower().Contains(word.ToLower()) || o.dateTime.Contains(word)
                             || o.addrLine.Contains(word) || o.phones.Contains(word) || (o.orderStatus.ToString().Contains(word))
                              select o );
                ListOrderItem = result.ToList();
            }
            lvOrders.ItemsSource = ListOrderItem;
            lblStatus.Text = "Total number of orders: " + ListOrderItem.Count;
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
            lblStatus.Text = "Total number of orders: " + orderList.Count;
        }

        private void chbShowAll_Checked(object sender, RoutedEventArgs e)
        {
            Globals.db.reloadOrderList(ref orderList);
            lvOrders.ItemsSource = orderList;
            lblStatus.Text = "Total number of orders: " + orderList.Count;
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

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            InvoiceWindow dlg1 = new InvoiceWindow(null);
            if (dlg1.ShowDialog() == true)
            {

            }
        }

        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            editCurrentOrder();
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to delete?", "Alert", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    int index = lvOrders.SelectedIndex;
                    if (index < 0)
                    {
                        return;
                    }
                    ListOrderItem i = (ListOrderItem)lvOrders.Items[index];
                    try
                    {
                        Globals.db.DeleteListOrderItemById((int)i.id);
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
                        lblStatus.Text = "Total number of orders: " + orderList.Count;


                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        MessageBox.Show("Database query error " + ex.Message);
                    }

                    break;
                case MessageBoxResult.No:

                    break;


            }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure to delete?", "Alert", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    int index = lvOrders.SelectedIndex;
                    if (index < 0)
                    {
                        return;
                    }
                    ListOrderItem i = (ListOrderItem)lvOrders.Items[index];
                    try
                    {
                        Globals.db.DeleteListOrderItemById((int)i.id);
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
                        lblStatus.Text = "Total number of orders: " + orderList.Count;


                    }
                    catch (SqlException ex)
                    {
                        Console.WriteLine(ex.StackTrace);
                        MessageBox.Show("Database query error " + ex.Message);
                    }

                    break;
                case MessageBoxResult.No:

                    break;


            }
        }

        private void miClients_Click(object sender, RoutedEventArgs e)
        {
            DlgClient dc = new DlgClient(null, 0);
            dc.ShowDialog();
        }
    }
}
