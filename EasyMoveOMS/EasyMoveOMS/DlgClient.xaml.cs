using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DlgClient : Window
    {
        List<Client> cl = new List<Client>();
        public Client currEditedClient = new Client();
        long id;
        String clientName, clientPhoneH, clientPhoneW, clientEmail;

        public DlgClient(Client c, int i)
        {
            InitializeComponent();
            if (i == 0) btOrder.IsEnabled = false;
            if (c != null)
            {
                //Put client data to the

                loadClientList();
              
                lblId.Content = c.id;
                tbName.Text = c.name;
                tbPhoneH.Text = c.phoneH;
                tbPhoneW.Text = c.phoneW;
                tbEmail.Text = c.email;
                currEditedClient = c;
            }
            else
            {
                loadClientList();
                btDelete.IsEnabled = false;
            }

        }

        private void loadClientList()
        {
            try
            {
                Globals.db.loadClientData(ref cl);
                lvClients.ItemsSource = cl;
            }
            catch (MySqlException exx)
            {
                MessageBox.Show("Error loading clients data:\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            catch (Exception exx)
            {
                MessageBox.Show(exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            //DialogResult = true;
        }

        private void btDelete_Click(object sender, RoutedEventArgs e)
        {
            if(int.TryParse(lblId.Content+"", out int deletedId))
            {
                try
                {
                    Globals.db.deleteClient(deletedId);

                    currEditedClient = new Client();
                    lblId.Content = "...";
                    tbName.Text = "";
                    tbPhoneH.Text = "";
                    tbPhoneW.Text = "";
                    tbEmail.Text = "";
                    lvClients.SelectedIndex = -1;
                    btUpdate.Content = "Save";
                    btDelete.IsEnabled = false;
                    loadClientList();
                }
                catch (MySqlException exx)
                {
                MessageBox.Show("Error deleting clients\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
                }
            }
        }

        private void btOrder_Click(object sender, RoutedEventArgs e)
        {
            if (currEditedClient.id == 0)
            {
                MessageBox.Show("Client must be saved into database first", "Save new client", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
            else
            {
                try  //to validate
                {
                    if (!isValide()) return;
                }
                
                catch (InvalidDataException ex)
                {
                    MessageBox.Show(ex.Message, "Data error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
                currEditedClient.name = clientName;
                currEditedClient.phoneH = clientPhoneH;
                currEditedClient.phoneW = clientPhoneW;
                currEditedClient.email = clientEmail;
                DialogResult = true;
            }
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = false;
        }

        private void lvClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvClients.SelectedIndex == -1) return;
            currEditedClient = (Client) lvClients.SelectedItem;
            lblId.Content = currEditedClient.id + "";
            tbName.Text = currEditedClient.name;
            tbPhoneH.Text = currEditedClient.phoneH;
            tbPhoneW.Text = currEditedClient.phoneW;
            tbEmail.Text = currEditedClient.email;
            btDelete.IsEnabled = true;
            btUpdate.Content = "Update";
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbSearch.Text == "")
            {
                loadClientList();
                return;
            }
            String word = tbSearch.Text;
            List<Client> cList = new List<Client>();
            if (word != "")
            {
                var result = from t in cl where (t.name).ToLower().Contains(word.ToLower()) || t.phones.Contains(word) select t;
                cList = result.ToList();
            }
            lvClients.ItemsSource = cList;
        }

        private void lvClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvClients.SelectedIndex == -1) return;
            currEditedClient = (Client)lvClients.SelectedItem;
            lblId.Content = currEditedClient.id + "";
            tbName.Text = currEditedClient.name;
            tbPhoneH.Text = currEditedClient.phoneH;
            tbPhoneW.Text = currEditedClient.phoneW;
            tbEmail.Text = currEditedClient.email;
            btDelete.IsEnabled = true;
            btUpdate.Content = "Update";
        }

        private void btNEw_Click(object sender, RoutedEventArgs e)
        {
            currEditedClient = new Client();
            lblId.Content = "...";
            tbName.Text = "";
            tbPhoneH.Text = "";
            tbPhoneW.Text = "";
            tbEmail.Text = "";
            lvClients.SelectedIndex = -1;
            btUpdate.Content = "Save";
            btDelete.IsEnabled = false;
        }

        private void btUpdate_Click(object sender, RoutedEventArgs e)
        {
            try  //to validate
            {
                if (isValide())
                {
                    currEditedClient.name = clientName;
                    currEditedClient.phoneH = clientPhoneH;
                    currEditedClient.phoneW = clientPhoneW;
                    currEditedClient.email = clientEmail;
                }
            }
            catch (InvalidDataException ex)
            {
                MessageBox.Show(ex.Message, "Data error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }

            if (currEditedClient.id == 0) // SAVE
            {
                try
                {
                    long newId = Globals.db.saveNewClient(currEditedClient);
                    MessageBox.Show("New client was saved into database" , "New client - EasyMove", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                    lblId.Content = newId + "";
                    currEditedClient.id = newId;
                    loadClientList();
                }
                catch (MySqlException exx)
                {
                    MessageBox.Show("Error saving new client:\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
            }
            else        //UPDATE
            {
                try
                {
                    Globals.db.updateClient(currEditedClient);
                    btUpdate.Content = "Save";
                    loadClientList();
                }
                catch (MySqlException exx)
                {
                    MessageBox.Show("Clientdata was not updated:\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                    return;
                }
            }
            btDelete.IsEnabled = true;
        }

        private bool isValide()
        {
            bool isOk = false;
            clientName = tbName.Text;
            clientPhoneH = tbPhoneH.Text;
            clientPhoneW = tbPhoneW.Text;
            clientEmail = tbEmail.Text;

            if (clientName.Length < 2 || clientName.Length > 150)
            {
                throw new InvalidDataException("Client name must be 2 to 150 characters");
            }
            if (clientPhoneH.Length > 25 || clientPhoneW.Length > 25)
            {
                throw new InvalidDataException("Phone number can not exceed 25 characters");
            }

            if (clientEmail.Length > 0 && !SmallClasses.emailIsValid(clientEmail))
            {
                tbEmail.Background = Brushes.Red;
                throw new InvalidDataException("Entered email is not valid");
            }
            isOk = true;
            return isOk;
        }
    }
}
