using EasyMoveOMS.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        List<Service> services = new List<Service>();
        double sum = 0;
        public static double TPS=0.05;
        public static double TVQ = 0.09975;
        public static double totalTax = 1.14975;


        public InvoiceWindow()
        {
            InitializeComponent();
           
           
            
            //populating the fields
            //lblName.Content = currentOrder.Name;   .......


            //if(not fixed price)
            lblTotalBeforeTax.Content = (Convert.ToDouble("85")) * (Convert.ToDouble("4")) + "";//add travel time
            lblTPS.Content = Convert.ToDouble(lblTotalBeforeTax.Content) * TPS;
            lblTVQ.Content = Convert.ToDouble(lblTotalBeforeTax.Content) * TVQ;
            tbTotal.Text = Convert.ToDouble(lblTotalBeforeTax.Content) * totalTax + "";



            services.Add(new Service() { Description = "Moving", Price = 85, Quantity = 4, Amount = 85 * 4 });
            services.Add(new Service() { Description = "Travel", Price = 85, Quantity = 1, Amount = 85 * 1 });

            // services.Add(new Service() { Description = "", Amount = 0 });
            //dgInvoice.Items.Add(new Service() { Description = "", Amount = 0 });

            dgInvoice.ItemsSource = services;

            for (int i = 0; i < services.Count; i++)
            {
                sum += services[i].Amount;
            }
            sum= Math.Round(sum, 2);

            lblTotalBeforeTax.Content = sum;
            lblTPS.Content = Math.Round(sum * TPS, 2);
            lblTVQ.Content = Math.Round(sum * TVQ, 2);
            tbTotal.Text = Math.Round(sum * totalTax, 2) + "";

            sum = 0;
            btnReset.IsEnabled = false;

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
        
            //int rowIndex;
        private void dgInvoice_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                var column = e.Column as DataGridBoundColumn;
                if (column != null)
                {
                    var bindingPath = (column.Binding as Binding).Path.Path;

                    // rowIndex has the row index
                    // bindingPath has the column's binding
                    // el.Text has the new, user-entered value

                    if (bindingPath == "Price")
                    {
                      int rowIndex = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;




                        string value = el.Text;
                        double price;
                        if (!double.TryParse(value, out price))
                        {
                            MessageBox.Show("Amount must be a number.");
                            return;
                        }
                        services[rowIndex].Amount = Math.Round(price * services[rowIndex].Quantity,2);


                        //recalculating the new total and other values


                        //  double sum = 0;
                        for (int i = 0; i < services.Count; i++)
                        {
                            sum += services[i].Amount;
                        }
                        sum = Math.Round(sum, 2);


                        lblTotalBeforeTax.Content = sum;
                        lblTPS.Content = Math.Round(sum * TPS, 2);
                        lblTVQ.Content = Math.Round(sum * TVQ, 2);
                        tbTotal.Text = Math.Round(sum * totalTax, 2) + "";

                        sum = 0;

                    }
                    if (bindingPath == "Quantity")
                    {
                        int rowIndex = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;




                        string value = el.Text;
                        double quant;
                        if (!double.TryParse(value, out quant))
                        {
                            MessageBox.Show("Quantity must be a number.");
                            return;
                        }
                        services[rowIndex].Amount = quant * services[rowIndex].Price;


                        //recalculating the new total and other values
                        //double sum=0;
                        for (int i = 0; i < services.Count; i++)
                        {
                            sum += services[i].Amount;
                        }
                        sum = Math.Round(sum, 2);


                        lblTotalBeforeTax.Content = sum;
                        lblTPS.Content = Math.Round(sum * TPS, 2);
                        lblTVQ.Content = Math.Round(sum * TVQ, 2);
                        tbTotal.Text = Math.Round(sum * totalTax, 2) + "";

                        sum = 0;
                    }
                }
            }
        }


        
        double local;
        private void tbDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            

            if (tbDiscount.Text != "") { tbTotal.IsEnabled = false; }
            if (tbDiscount.Text == "") { tbTotal.IsEnabled = true; }


            string value = tbDiscount.Text;
            double discount;

            if (!double.TryParse(value, out discount))
            {
                if (value == "")
                {
                    lblTotalBeforeTax.Content = local;
                    
                    lblTPS.Content = Math.Round(local * TPS, 2);
                    lblTVQ.Content = Math.Round(local * TVQ, 2);
                    tbTotal.Text = Math.Round(local * totalTax, 2) + "";
                    return;
                }
                MessageBox.Show("Discount must be a number.");
                return;
            }
            double temp = local - local * discount / 100;//Convert.ToDouble(lblTotalBeforeTax.Content) - Convert.ToDouble(lblTotalBeforeTax.Content) * discount / 100;//sum - sum * discount / 100;
            lblTotalBeforeTax.Content = Math.Round(temp, 2);
            lblTPS.Content = Math.Round(temp * TPS, 2);
            lblTVQ.Content = Math.Round(temp * TVQ, 2);
            tbTotal.Text = Math.Round(temp * totalTax, 2) + "";


        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            //dgInvoice.CancelEdit();
            //if (dgInvoice.CancelEdit(DataGridEditingUnit.Cell)) { return; }

            //if (rowIndex != (services.Count - 1)) { return; }
            MessageBoxResult result = MessageBox.Show("Would you like to delete the selected row?", "Alert", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    //try
                    //{
                        int index = dgInvoice.SelectedIndex;

                        if (index < 0)
                        {

                            return;
                        }
                        //if (index == services.Count - 1)
                        //{
                        //    MessageBox.Show("Please complete editing the current row.");
                        //    return;
                        //}

                        services.RemoveAt(index);

                        dgInvoice.ItemsSource = services;
                        dgInvoice.Items.Refresh();

                        for (int i = 0; i < services.Count; i++)
                        {
                            sum += services[i].Amount;
                        }

                        sum = Math.Round(sum, 2);


                        lblTotalBeforeTax.Content = sum;
                        lblTPS.Content = Math.Round(sum * TPS, 2);
                        lblTVQ.Content = Math.Round(sum * TVQ, 2);
                        tbTotal.Text = Math.Round(sum * totalTax, 2) + "";

                        sum = 0;
                    //}
                    //catch (InvalidOperationException)
                    //{
                    //    MessageBox.Show("Please complete editing the current row.");
                    //        return;

                    //}

                    break;
                case MessageBoxResult.No:

                    break;


            }
        }

        private void tbDiscount_GotFocus(object sender, RoutedEventArgs e)
        {

             local = Convert.ToDouble(lblTotalBeforeTax.Content);
        }

        private void tbTotal_GotFocus(object sender, RoutedEventArgs e)
        {
            if(tbTotal.Text!="")
            localTotal = Convert.ToDouble(tbTotal.Text);
        }

        double localTotal;
        private void tbTotal_TextChanged_1(object sender, TextChangedEventArgs e)//if the total amount is agreed
        {
            if(localTotal!=0)
            lblLocalTotal.Content =localTotal;
            btnReset.IsEnabled = true;
            //string value = tbTotal.Text;
            //double total;

            //if (!double.TryParse(value, out total))
            //{

            //    MessageBox.Show("Discount must be a number.");
            //    return;
            //}
            //if ((localTotal - total) > 0)
            //{
            //    double temp = (localTotal - total) * 100 / localTotal;


            //    lblTotalBeforeTax.Content = (1 - temp / 100) * total;
            //    lblTPS.Content = (1 - temp / 100) * total * TPS;
            //    lblTVQ.Content = (1 - temp / 100) * total * TVQ;

            //}
            //if ((localTotal - total) < 0)
            //{
            //    lblDiscount.Content = "";
            //    double temp = (total - localTotal) * 100 / localTotal;


            //    lblTotalBeforeTax.Content = (1 + temp / 100) * total;
            //    lblTPS.Content = (1 + temp / 100) * total * TPS;
            //    lblTVQ.Content = (1 + temp / 100) * total * TVQ;
            //}

        }
        private void btnSetNew_Click(object sender, RoutedEventArgs e)
        {
            string value = tbTotal.Text;
            double total;

            if (!double.TryParse(value, out total))
            {
                if (value == "")
                {
                    MessageBox.Show("Fill out the field, please.");
                    return;
                }
                MessageBox.Show("Total must be a number.");
                return;
            }

            if ((total - localTotal) > 0)
            {
                if (services[services.Count - 1].Description == "Other" || services[services.Count - 1].Description == "Discount")
                {
                    services.RemoveAt(services.Count - 1);

                    dgInvoice.ItemsSource = services;
                    dgInvoice.Items.Refresh();
                }
                    services.Add(new Service() { Description = "Other", Price = total - localTotal, Quantity = 1, Amount = (total - localTotal) * 1 });
                    

                    dgInvoice.ItemsSource = services;
                    dgInvoice.Items.Refresh();

                    double temp = total / totalTax;
                    lblTotalBeforeTax.Content = Math.Round(temp, 2);
                    lblTPS.Content = Math.Round(temp * TPS, 2);
                    lblTVQ.Content = Math.Round(temp * TVQ, 2);
                
            }

            if ((localTotal - total) > 0)
            {
                if (services[services.Count - 1].Description == "Other" || services[services.Count - 1].Description == "Discount")
                {
                    services.RemoveAt(services.Count - 1);

                    dgInvoice.ItemsSource = services;
                    dgInvoice.Items.Refresh();
                }
                services.Add(new Service() { Description = "Discount", Price = localTotal - total, Quantity = 1, Amount = (localTotal - total) * 1 });
                dgInvoice.ItemsSource = services;
                dgInvoice.Items.Refresh();

                double temp = total / totalTax;
                lblTotalBeforeTax.Content = Math.Round(temp, 2);
                lblTPS.Content = Math.Round(temp * TPS, 2);
                lblTVQ.Content = Math.Round(temp * TVQ, 2);

            }
        }

        private void chbCalculateTax_Checked(object sender, RoutedEventArgs e)
        {
     
            lblTPS.Content = "";
            lblTVQ.Content = "";
            tbTotal.Text = lblTotalBeforeTax.Content+"";
        }

        private void chbCalculateTax_Unchecked(object sender, RoutedEventArgs e)
        {
            lblTPS.Content = Math.Round(Convert.ToDouble(lblTotalBeforeTax.Content)*TPS, 2)+ "";
            lblTVQ.Content = Math.Round(Convert.ToDouble(lblTotalBeforeTax.Content) * TVQ, 2) + "";
            tbTotal.Text = Math.Round(Convert.ToDouble(lblTotalBeforeTax.Content) * totalTax,2) + "";
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            if (lblLocalTotal.Content!="")
            {
                tbTotal.Text = Math.Round(Convert.ToDouble(lblLocalTotal.Content), 2) + "";

                double temp = Convert.ToDouble(lblLocalTotal.Content) / totalTax;
                lblTotalBeforeTax.Content = Math.Round(temp, 2);
                lblTPS.Content = Math.Round(temp * TPS, 2);
                lblTVQ.Content = Math.Round(temp * TVQ, 2);


            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
        //    public long id { get; set; }
        //public long orderId { get; set; }
        //public DateTime invoiceDate { get; set; }
        //public long clientAddrId { get; set; }
        //public bool noTax { get; set; }


            long id= 0;
            long orId = 1;//order
            DateTime invDate = new DateTime();//order
            long clientAddrId = 1;
            bool noT= (bool)chbCalculateTax.IsChecked;
            Invoice inv = new Invoice(id, orId, invDate, clientAddrId, noT);
           

            long  invoiceId= Globals.db.AddInvoice(inv);

            

            long id1 = 0;

            string[] name = new string[services.Count];
            double[] price = new double[services.Count];
            for (int i = 0; id < services.Count - 1; i++)
            { 
                name[i] = services[i].Description;
                price[i] = services[i].Amount;

                InvoiceItem ii = new InvoiceItem(id1, invoiceId, name[i], price[i]);
               // Globals.db.AddInvoiceItems(ii);
            }
            


        }
    }

    public class Service : INotifyPropertyChanged//,  IEditableObject
    {
        public Service() { Quantity = 1; }
        public string Description { get; set; }
        public double Price { get; set; }
        public double Quantity { get; set; }
        //public double Amount { get; set; }
        private double _amount;
        public double Amount
        {
            get { return _amount; }
            set { _amount = value; NotifyPropertyChanged("Amount"); }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        

        public event PropertyChangedEventHandler PropertyChanged;
    }

}

