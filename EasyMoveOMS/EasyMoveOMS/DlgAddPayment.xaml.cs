using MySql.Data.MySqlClient;
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
    /// Interaction logic for DlgAddPayment.xaml
    /// </summary>
    public partial class DlgAddPayment : Window
    {
        long id;
        Payment editedPayment;
        decimal amount;
        DateTime paymentDate;
        Payment.PayMethod method;
        String notes;

        public DlgAddPayment(Payment p)
        {
            InitializeComponent();
            editedPayment = p;
            dpDate.SelectedDate = DateTime.Today;
            if (editedPayment.id != 0)
            {
                dpDate.SelectedDate = editedPayment.paymentDate;
                tbAmount.Text = editedPayment.amount+"";
                cbbMethod.SelectedIndex = (int)editedPayment.method;
                tbNotes.Text = editedPayment.notes;
                Title = "Edit Payment";
            }
        }

        private void btSaveAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!decimal.TryParse(tbAmount.Text, out amount)) throw new Exception("Invalid amount entered");
                paymentDate = (DateTime)dpDate.SelectedDate;
                if (cbbMethod.SelectedIndex == -1) throw new Exception("Payment method is not selected");
                method = (Payment.PayMethod)(cbbMethod.SelectedIndex);
                notes = tbNotes.Text;
                if(notes.Length>150) throw new Exception("Notes can not exceed 150 characters");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Data error", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return;
            }
            editedPayment.amount = amount;
            editedPayment.paymentDate = paymentDate;
            editedPayment.method = method;
            editedPayment.notes = notes;
            try
            {
                if (editedPayment.id == 0)
                {
                    id = Globals.db.addPayment(editedPayment);
                    editedPayment.id = id;
                }
                else Globals.db.updatePayment(editedPayment);
            }
            catch (MySqlException exx)
            {
                MessageBox.Show("Payment was not saved:\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            DialogResult = true;

        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if (editedPayment.id == 0)
            {
                dpDate.SelectedDate = DateTime.Today;
                tbAmount.Text = "";
                cbbMethod.SelectedIndex = -1;
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this payment?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.No) return;
                try
                {
                    Globals.db.deletePayment(editedPayment.id);
                }
                catch (MySqlException exx)
                {
                    MessageBox.Show("Payment was not deleted:\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
            }
            DialogResult = true;
        }

       
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
