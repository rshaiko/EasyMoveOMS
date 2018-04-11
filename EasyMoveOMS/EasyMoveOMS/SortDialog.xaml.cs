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
    /// Interaction logic for SortDialog.xaml
    /// </summary>
    public partial class SortDialog : Window
    {
        ListView lvOrdersSort;
        List<ListOrderItem> orderListSort;
        public SortDialog(List<ListOrderItem> orderList, ListView lvOrders)
        {
            InitializeComponent();
            orderListSort = orderList;
            lvOrdersSort = lvOrders;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (rbName.IsChecked.Value)
            {
                var listByName = from p in orderListSort orderby p.name select p;
                List<ListOrderItem> orderListByName = listByName.ToList();
                lvOrdersSort.ItemsSource = orderListByName;
            }
            if (rbDate.IsChecked.Value)
            {
                var listByDate = from p in orderListSort orderby p.dateTime select p;
                List<ListOrderItem> orderListByDate = listByDate.ToList();
                lvOrdersSort.ItemsSource = orderListByDate;
            }
            if (rbAddress.IsChecked.Value)
            {
                var listByAddr = from p in orderListSort orderby p.addrLine select p;
                List<ListOrderItem> orderListByAddr = listByAddr.ToList();
                lvOrdersSort.ItemsSource = orderListByAddr;
            }
            if (rbPhone.IsChecked.Value)
            {
                var listByPhone = from p in orderListSort orderby p.phones select p;
                List<ListOrderItem> orderListByPhone = listByPhone.ToList();
                lvOrdersSort.ItemsSource = orderListByPhone;
            }
            if (rbStatus.IsChecked.Value)
            {
                var listByStatus = from p in orderListSort orderby p.orderStatus select p;
                List<ListOrderItem> orderListByStatus = listByStatus.ToList();
                lvOrdersSort.ItemsSource = orderListByStatus;
            }
                DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
