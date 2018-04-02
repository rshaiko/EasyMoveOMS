using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for InvoiceWindow.xaml
    /// </summary>
    public partial class InvoiceWindow : Window
    {
        double addAmount = 0;
        double addAmount2 = 0;
        double addAmount3 = 0;
        public InvoiceWindow()
        {
            InitializeComponent();
            /* double tbHourly;
             string str = tbHourly.Text;

             if (!double.TryParse(str, out tbHourly))
             {
                 
             }*/
             //populating the fields
            tbTotalBefore.Text = (Convert.ToDouble(tbHourly.Text)) * (Convert.ToDouble(tbHours.Text)) + addAmount + addAmount2 + addAmount3 + "";
            lblTPS.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.05;
            lblTVQ.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.0975;
            tbTotal.Text = Convert.ToDouble(tbTotalBefore.Text) * 1.1475 + "";

            List<Service> services = new List<Service>();
            // services.Add(new Service() { Description = "", Amount = 0 });
            // services.Add(new Service() { Description = "", Amount = 0 });
            //dgInvoice.Items.Add(new Service() { Description = "", Amount = 0 });

            dgInvoice.ItemsSource = services;

            

        }
        

        /* private void dgInvoice_SelectionChanged(object sender, SelectionChangedEventArgs e)
         {
             DataGrid dg = (DataGrid)sender;
             DataRowView rowSelected = dg.SelectedItem as DataRowView;
             if (rowSelected != null) {
                 lblDescription.Content = rowSelected.Row[0].ToString();
                 lblAmount.Content = rowSelected.Row[1].ToString();
             }
         }*/

        private void dgInvoice_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;
                    if (bindingPath == "Description")
                    {
                        int rowIndex = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;
                        if (rowIndex == 0)
                            lblDescription.Content = el.Text;
                        else if (rowIndex == 1)
                            lblDescription2.Content = el.Text;
                        else
                            lblDescription3.Content = el.Text;
                        // rowIndex has the row index
                        // bindingPath has the column's binding
                        // el.Text has the new, user-entered value
                    }
                    if (bindingPath == "Amount")
                    {
                        int rowIndex = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;
                        if (rowIndex == 0)
                        {
                            lblAmount.Content = el.Text;
                            addAmount = Convert.ToDouble(el.Text);
                            //recalculating the new total and other values
                            tbTotalBefore.Text = (Convert.ToDouble(tbHourly.Text)) * (Convert.ToDouble(tbHours.Text)) + addAmount + addAmount2 + addAmount3 + "";
                            lblTPS.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.05;
                            lblTVQ.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.0975;
                            tbTotal.Text = Convert.ToDouble(tbTotalBefore.Text) * 1.1475 + "";
                        }
                        else if (rowIndex == 1)
                        {
                            lblAmount2.Content = el.Text;
                            addAmount2 = Convert.ToDouble(el.Text);
                            //recalculating the new total and other values
                            tbTotalBefore.Text = (Convert.ToDouble(tbHourly.Text)) * (Convert.ToDouble(tbHours.Text)) + addAmount + addAmount2 + addAmount3 + "";
                            lblTPS.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.05;
                            lblTVQ.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.0975;
                            tbTotal.Text = Convert.ToDouble(tbTotalBefore.Text) * 1.1475 + "";
                        }
                        else
                        {
                            lblAmount3.Content = el.Text;
                            addAmount3 = Convert.ToDouble(el.Text);
                            //recalculating the new total and other values
                            tbTotalBefore.Text = (Convert.ToDouble(tbHourly.Text)) * (Convert.ToDouble(tbHours.Text)) + addAmount + addAmount2 + addAmount3 + "";
                            lblTPS.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.05;
                            lblTVQ.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.0975;
                            tbTotal.Text = Convert.ToDouble(tbTotalBefore.Text) * 1.1475 + "";
                        }
                        // rowIndex has the row index
                        // bindingPath has the column's binding
                        // el.Text has the new, user-entered value
                    }
                }
            }
        }

        private void addOne_Click(object sender, RoutedEventArgs e)
        {

        }

        private void tbTotal_TextChanged(object sender, TextChangedEventArgs e)//if the total amount is agreed
        {
            if (tbTotal.Text != "")
            {
                tbTotalBefore.Text = (Convert.ToDouble(tbTotal.Text) / 1.1475) + "";
                lblTVQ.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.0975;
                lblTPS.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.05;
                tbHourly.Text = (Convert.ToDouble(tbTotalBefore.Text) - addAmount - addAmount2 - addAmount3) / Convert.ToDouble(tbHours.Text) + "";
            }
        }

        private void tbDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbDiscount.Text != "")
            {
                tbTotalBefore.Text = ((100 - Convert.ToDouble(tbDiscount.Text))/100.0)* Convert.ToDouble(tbTotalBefore.Text) + "";
                lblTVQ.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.0975;
                lblTPS.Content = Convert.ToDouble(tbTotalBefore.Text) * 0.05;
                tbHourly.Text = (Convert.ToDouble(tbTotalBefore.Text) - addAmount - addAmount2 - addAmount3) / Convert.ToDouble(tbHours.Text) + "";
                tbTotal.Text= Convert.ToDouble(tbTotalBefore.Text) * 1.1475 +"";
            }
        }
    }

    public class Service
    {
        public string Description { get; set; }

        public double Amount { get; set; }

    }
}
