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
        public InvoiceWindow()
        {
            InitializeComponent();

            //populating the fields
            //lblName.Content = currentOrder.Name;   .......


            //if(not fixed price)
            lblTotalBeforeTax.Content  = (Convert.ToDouble("85")) * (Convert.ToDouble("4")) + "";//add travel time
            lblTPS.Content = Convert.ToDouble(lblTotalBeforeTax.Content) * 0.05;
            lblTVQ.Content = Convert.ToDouble(lblTotalBeforeTax.Content) * 0.09975;
            tbTotal.Text = Convert.ToDouble(lblTotalBeforeTax.Content) * 1.14975 + "";

           
            
             services.Add(new Service() { Description = "Moving", Price=85, Quantity=4, Amount = 85*4 });
            services.Add(new Service() { Description = "Travel", Price = 85, Quantity = 1, Amount = 85 * 1 });

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
                        services[rowIndex].Amount = price* services[rowIndex].Quantity;


                        //recalculating the new total and other values


                      //  double sum = 0;
                        for (int i = 0; i < services.Count; i++)
                        {
                            sum += services[i].Amount;
                        }

                        lblTotalBeforeTax.Content = sum;
                        lblTPS.Content = sum * 0.05;
                        lblTVQ.Content = sum * 0.09975;
                        tbTotal.Text = sum * 1.14975 + "";
                        sum = 0;

                    }
                    if (bindingPath == "Quantity")
                    {
                        int rowIndex = e.Row.GetIndex();
                        var el = e.EditingElement as TextBox;




                        string value = el.Text;
                        int quant;
                        if (!int.TryParse(value, out quant))
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

                        lblTotalBeforeTax.Content = sum;
                        lblTPS.Content = sum * 0.05;
                        lblTVQ.Content = sum * 0.09975;
                        tbTotal.Text = sum * 1.14975 + "";

                        sum = 0;
                    }
                }
            }
        }

       /* private void addOne_Click(object sender, RoutedEventArgs e)
        {
         
        }*/

        private void tbTotal_TextChanged(object sender, TextChangedEventArgs e)//if the total amount is agreed
        {
            
        }

        private void tbDiscount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbDiscount.Text != "") { tbTotal.IsEnabled = false; }
        }

        private void miDelete_Click(object sender, RoutedEventArgs e)
        {
            //dgInvoice.CommitEdit();

            MessageBoxResult result = MessageBox.Show("Would you like to delete the selected row?", "Alert", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    int index = dgInvoice.SelectedIndex;
                    
                    if (index < 0 || index==services.Count-1)
            {
                        MessageBox.Show("Please complete editing the current row.");
                        return;
            }
                    
                        services.RemoveAt(index);

                        dgInvoice.ItemsSource = services;
                        dgInvoice.Items.Refresh();

                        for (int i = 0; i < services.Count; i++)
                        {
                            sum += services[i].Amount;
                        }

                        lblTotalBeforeTax.Content = sum;
                        lblTPS.Content = sum * 0.05;
                        lblTVQ.Content = sum * 0.09975;
                        tbTotal.Text = sum * 1.14975 + "";

                        sum = 0;
                    
                    break;
                case MessageBoxResult.No:

                    break;


            }
        }
    }

    public class Service : INotifyPropertyChanged
    {
        public Service() { Quantity = 1; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
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

