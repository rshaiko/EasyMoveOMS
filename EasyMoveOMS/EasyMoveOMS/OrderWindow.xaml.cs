using Microsoft.Windows.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    //TODOs
    //1) Actual and destination codes are equal

    public partial class OrderWindow : Window
    {
        //DECLARATIONS
        bool tbZipEventsOn = true; //off events if not manual changes
        String zipAct, zipDest, zipInt;
        bool[] zipsOk = new bool[3] { false, false, false }; //true if actual or destination zip codes well formatted

        //REGULAR EXPRESSIONS
        //Regex for Canadian Postal Code
        static String rexZip = @"^[ABCEGHJKLMNPRSTVXYabceghjklmnprstvxy][0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz] ?[0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz][0-9]$";
        static Regex rZip = new Regex(rexZip);
        
        //Regex for Email
        //static String rexEmail = @"^[ABCEGHJKLMNPRSTVXYabceghjklmnprstvxy][0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz] ?[0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz][0-9]$";
        //static Regex rEmail = new Regex(rexEmail);


        //MOVING INFORMATION
        float orderId, truckId, clientId;
        DateTime moveDate;
        String startHour, startMinute;
        String phoneHome, phoneWork, email;
        int workers, hours, travelTime;
        String arriveTimeFrom, arriveTimeTo;

        //DESCRIPTION OF ITEMS TO MOVE
        int boxes, beds, sofas, frigos, wds, desks, tables, chairs, other;
        string details;
        bool oversized, overweight, fragile, expensive;



        //PAYMENT INFORMATION
        bool perHour, isCompletePayment;
        int maxHours;
        decimal estimatedTotal, paymentsTotal, deposit;

        //ORDER STATUS
        enum Status {scheduled, suspended, done};
        DateTime contactOnDate;
        int timeStartH, timeStartM, timeBreaksH, timeBreaksM, timeEndH, timeEndM, timeTotalH, timeTotalM;

        //ADDRESS INFORMATION 
        ////String addrActLine, addrDestLine, addrIntLine;
        ////String addrActCity, addrDestCity, addrIntCity;
        ////String addrActZip, addrDestZip, addrIntZip;
        ////String addrActProvince, addrDestProvince, addrIntProvince;
        String addrLine, addrCity, addrProvince, addrZip, addrNotes;
        int addrFloor;
        bool addrStairs, addrElevator, addrIsBilling;

        public OrderWindow()
        {
            InitializeComponent();
        }

        //ZIP validations
        private void tbZipAct_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbZipEventsOn) checkZip(tbZipAct.Text, 0, sender);
        }

        private void tbZipDest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbZipEventsOn) checkZip(tbZipDest.Text, 1, sender);
        }

        private void tbZipInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (tbZipEventsOn) checkZip(tbZipDest.Text, 2, sender);
        }

        private void checkZip(string tmp, int i, object sender)
        {
            TextBox tb = (TextBox)sender;
            Match mZip = rZip.Match(tmp);
            if (mZip.Success)
            {
                zipsOk[i] = true;
                tb.Background = Brushes.LightGreen;
                tbZipEventsOn = false;
                if (tb.Text.Length == 6)
                {
                    tb.Text = tb.Text.Substring(0, 3) + " " + tb.Text.Substring(3,3);
                }
                tb.Text = (tb.Text).ToUpper();
                tbZipEventsOn = true;
                if (zipsOk[0] && zipsOk[1] && i!=2) // i!=2 - don't make api request in case entering intermediate address
                {
                    string url = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + tbZipAct.Text + "&destinations=" + tbZipDest.Text + "&key=" + Globals.APIkey; 
                    try
                    {
                        string json = Api.getJson(url);
                        GoogleMatrixData goo = JsonConvert.DeserializeObject<GoogleMatrixData>(json);
                        tbGoogleDistance.Text = goo.rows[0].elements[0].distance.text.ToString();
                        tbGoogleTime.Text = goo.rows[0].elements[0].duration.text.ToString();
                    }
                    catch (Exception ex)
                    {
                        tbGoogleDistance.Text = "";
                        tbGoogleTime.Text = "";
                    }
                }
            }
            else
            {
                zipsOk[i] = false;
                if (i == 0 || i == 1)
                {
                    tbGoogleDistance.Text ="";
                    tbGoogleTime.Text = "";
                }
                tb.Background = Brushes.LightYellow;
            }
        }

        // !!!!!!! --- GET Schedule --- !!!!!!!!!!!!
        private void dpMoveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //SAVE
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            validateOrder();

        }

        private void validateOrder()
        {
            moveDate=dpMoveDate.DisplayDate;
            
        }

        //Some Validation Methods



        //SPIN BOXES
        private void spinBoxes_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbBoxes, e);
        }

        private void spinBeds_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbBeds, e);
        }

        private void spinSofas_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbSofas, e);
        }

        private void spinFrigos_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbFrigos, e);
        }

        private void spinWds_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbWds, e);
        }

        private void spinDesks_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbDesks, e);
        }

        private void spinTables_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbTables, e);
        }

        private void spinChairs_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbChairs, e);
        }

        private void spinOther_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbOther, e);
        }

        private void spinFloorAddrAct_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbFloorAct, e);
        }

        private void spinFloorAddrDest_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbFloorDest, e);
        }

        private void spinFloorAddrInt_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbFloorInt, e);
        }

        private void spinTravelTime_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbTravelTime, e);
        }

        private void spinArriveTo_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbArriveTo, e);
        }

        private void spinWorkers_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            getNewSpinValue(tbWorkers, e);
        }

        private void spinHours_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            getNewSpinValue(tbHours, e);
        }

        private void getNewSpinValue(TextBox tb, SpinEventArgs e)
        {
            string currentSpinValue = tb.Text;
            int currentValue;
            try
            {
                currentValue = String.IsNullOrEmpty(currentSpinValue) ? 0 : Convert.ToInt32(currentSpinValue);
            }
            catch (FormatException) {
                currentValue = 0;
            }
            if (e.Direction == SpinDirection.Increase)
                currentValue++;
            else
                currentValue--;
            if (currentValue < 0)
                currentValue = 0;
            tb.Text = currentValue.ToString();
        }
    }
}
