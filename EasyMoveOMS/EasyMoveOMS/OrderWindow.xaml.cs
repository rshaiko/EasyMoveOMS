using Microsoft.Windows.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
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
        Order currentOrder = null;
        List <Address> orderAddresses = new List<Address>();
        Order.OrderStatus os;
        Client orderClient = null;
        Truck orderTruck;
        

        bool isNewOrder=false;
        bool isNewClient = true;
        bool tbZipEventsOn = true; //off events if not manual changes
        bool cbDoneEventsOn = true; //off events if not manual changes
        bool movingDateIsChanged = false;
        bool contactOnDateIsChanged = false;

        String zipAct, zipDest, zipInt;
        bool[] zipsOk = new bool[3] { false, false, false }; //true if zip code is well formatted

        //REGULAR EXPRESSIONS
        //Regex for Canadian Postal Code
        static String rexZip = @"^[ABCEGHJKLMNPRSTVXYabceghjklmnprstvxy][0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz] ?[0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz][0-9]$";
        static Regex rZip = new Regex(rexZip);
        
        //Regex for Email
        //static String rexEmail = @"^[ABCEGHJKLMNPRSTVXYabceghjklmnprstvxy][0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz] ?[0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz][0-9]$";
        //static Regex rEmail = new Regex(rexEmail);


        //MOVING INFORMATION
        long orderId, truckId;
        DateTime moveDate;
        TimeSpan moveTime, maxTime, minTime, travelTime, arriveTimeFrom, arriveTimeTo, doneStartTime, doneEndTime, doneBreaksTime, doneTotalTime;
        int startHour, startMinute, travelTimeH, travelTimeM, maxHours, maxMinutes, hours, minutes;
        String phoneHome, phoneWork, email, googleTime, googleDistance;
        int workers;
        bool payPerHour;
        decimal pricePerHour, deposit;

        //CLIENT INFORMATION
        long clientId;
        String clientName, clientPhoneH, clientPhoneW, clientEmail;
        //DESCRIPTION OF ITEMS TO MOVE
        int boxes, beds, sofas, frigos, wds, desks, tables, chairs, other;
        string details;
        bool oversized, overweight, fragile, expensive;

        //PAYMENT INFORMATION
        bool isCompletePayment;

        //ORDER STATUS
        enum Status {scheduled, suspended, done};
        DateTime contactOnDate;
        int timeStartH, timeStartM, timeBreaksH, timeBreaksM, timeEndH, timeEndM, timeTotalH, timeTotalM;


        //ADDRESS INFORMATION 
        String addrLine, addrCity, addrProvince, addrZip, addrNotes;
        int addrFloor;
        bool addrStairs, addrElevator, addrIsBilling;
        Address.AddrType addrType;

        public OrderWindow (Order order)
        {
            InitializeComponent();
            currentOrder = order;
            if (currentOrder != null)  
            {
                orderClient = null; // TODO change to existing client
            }
            else
            {
                isNewOrder = true;
            }

            cbTruck.ItemsSource = Globals.truckList;
        }

        // !!!!!!! --- GET Schedule --- !!!!!!!!!!!!
        private void dpMoveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            movingDateIsChanged = true;
        }
        private void dpContactOn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            contactOnDateIsChanged = true;
        }

        //SAVE
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            validateOrder();
            String confirmation;

            //Save new Client if it is NEW
            if (orderClient.Id == 0)
            {
                try
                {
                    clientId = MainWindow.db.saveNewClient(orderClient);
                    orderClient.Id = clientId;
                    confirmation = "1) Database: New client " + orderClient.Name + " was saved \n";
                    System.Windows.MessageBox.Show("Database: New client " + orderClient.Name + " was saved");
                    tbClient.IsEnabled = false;
                    tbEmail.IsEnabled = false;
                    tbPhoneH.IsEnabled = false;
                    tbPhoneW.IsEnabled = false;
                }
                catch (SqlException er)
                {
                    System.Windows.MessageBox.Show("Database error. New client was not added! \n" + er.Message);
                    return;
                }
            }

            if (currentOrder == null)
            {
                currentOrder = new Order();
                
            }
            currentOrder.orderAddresses = orderAddresses;
            currentOrder.id = 0; // IF new
            currentOrder.moveDate = moveDate;
            currentOrder.moveTime = moveTime;
            currentOrder.workers = workers;
            currentOrder.pricePerHour = pricePerHour;
            currentOrder.maxTime = maxTime;
            currentOrder.minTime = minTime;
            currentOrder.deposit = deposit;
            currentOrder.travelTime = travelTime;
            currentOrder.arriveTimeFrom = arriveTimeFrom;
            currentOrder.arriveTimeTo = arriveTimeTo;
            currentOrder.boxes = boxes;
            currentOrder.beds = beds;
            currentOrder.sofas = sofas;
            currentOrder.frigos = frigos;
            currentOrder.wds = wds;
            currentOrder.desks = desks;
            currentOrder.tables = tables;
            currentOrder.chairs = chairs;
            currentOrder.other = other;
            currentOrder.oversized = oversized;
            currentOrder.overweight = overweight;
            currentOrder.fragile = fragile;
            currentOrder.expensive = expensive;
            currentOrder.details = details;
            currentOrder.isPaid = isCompletePayment;
            currentOrder.orderStatus = os;
            currentOrder.contactOnDate = contactOnDate;
            currentOrder.doneStartTime = doneStartTime;
            currentOrder.doneEndTime = doneEndTime;
            currentOrder.doneBreaksTime = doneBreaksTime;
            currentOrder.doneTotalTime = doneTotalTime;
            currentOrder.orderClient = orderClient;


            //Save new or
            try
            {
                orderId = MainWindow.db.saveNewOrder(currentOrder);
                currentOrder.id = orderId;
            }
            catch(SqlException ex)
            {

            }

        }

        private void validateOrder()
        {
            try
            {
                // >> Validations
                // -- date and time
                if (isNewOrder && !movingDateIsChanged)
                {
                    throw new InvalidDataException("Moving date is not selected");
                }
                moveDate = dpMoveDate.DisplayDate;

                //GetVisualChild(cbStartTimeH)

                if (cbStartTimeH.Text == "")
                {
                    cbStartTimeH.Background = Brushes.Red;
                    throw new InvalidDataException("Start time is not selected");
                }

                if (!int.TryParse(cbStartTimeH.Text, out startHour))
                {
                    throw new InvalidDataException("Inavalid start time selected");
                }

                if ((startHour < 0 || startHour > 23))
                {
                    throw new InvalidDataException("Inavalid start time selected");
                }
                if (!int.TryParse(cbStartTimeM.Text, out startMinute)) startMinute = 0;

                moveTime = new TimeSpan(startHour, startMinute, 0);

                // ==> CUSTOMER
                if (orderClient == null)
                {
                    clientName = tbClient.Text;
                    clientPhoneH = tbPhoneH.Text;
                    clientPhoneW = tbPhoneW.Text;
                    clientEmail = tbEmail.Text;
                    if (clientName.Length < 2 || clientName.Length > 150)
                    {
                        tbClient.Background = Brushes.Red;
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
                    if (isNewClient)
                    {
                        orderClient = new Client(0, clientName, clientEmail, clientPhoneH, clientPhoneW);
                    }
                }

                // ==> TRUCK AND WORKERS
                if (cbTruck.SelectedIndex == -1)
                {
                    //TODO Order without track
                    throw new InvalidDataException("Truck is not selected");
                }
                orderTruck = (Truck)cbTruck.SelectedItem;

                if (!int.TryParse(tbWorkers.Text, out workers))
                {
                    tbWorkers.Background = Brushes.Red;
                    throw new InvalidDataException("Invalid number of workers");
                }
                if (workers < 0)
                {
                    tbWorkers.Background = Brushes.Red;
                    throw new InvalidDataException("Workers number can not be negative");
                }

                
                //Max HOURS
                if(!int.TryParse(tbMaxHours.Text, out maxHours))
                {
                    if (tbMaxHours.Text == "") { maxHours = 0; }
                    else throw new InvalidDataException("Invalid limit in hours");
                }
                else
                {
                    if (maxHours < 0 || maxHours > 100) throw new InvalidDataException("Max limit must be 0 to 100 hours");
                }

                if (!int.TryParse(cbMaxMinutes.Text, out maxMinutes)) maxMinutes = 0;
                maxTime = new TimeSpan(maxHours, maxMinutes, 0);

                 if(!int.TryParse(tbMinHours.Text, out hours))
                {
                    if (tbMinHours.Text == "") { hours = 0; }
                    else throw new InvalidDataException("Invalid limit in hours");
                }
                else
                {
                    if (hours < 0 || hours > 100) throw new InvalidDataException("Min limit must be 0 to 100 hours");
                }

                if (!int.TryParse(cbMinMinutes.Text, out minutes)) minutes = 0;
                minTime = new TimeSpan(hours, minutes, 0);



                //DEPOSIT
                if(!decimal.TryParse(tbDeposit.Text, out deposit))
                {
                    if (tbDeposit.Text == "") deposit = 0;
                    else throw new InvalidDataException("Invalid deposit amount entered");
                }

                //TRAVEL TIME
                if (!int.TryParse(tbTravelTimeH.Text, out travelTimeH))
                {
                    tbTravelTimeH.Background = Brushes.Red;
                    return;
                }
                if (travelTimeH < 0)
                {
                    tbTravelTimeH.Background = Brushes.Red;
                    return;
                }
                if (!int.TryParse(cbTravelMinutes.Text, out travelTimeM )) travelTimeM = 0 ;
                travelTime = new TimeSpan(travelTimeH, travelTimeM, 0);

                //ARRIVE TIME
                //--> from
                if (!int.TryParse(cbArriveFromH.Text, out hours))
                {
                    if (cbArriveFromH.Text == "") { hours = 0; }
                    else throw new InvalidDataException("Invalid Arrive time hours (from)");
                }
                else
                {
                    if (maxHours < 0 || maxHours > 24) throw new InvalidDataException("Invalid Arrive time hours (from)");
                }

                if (!int.TryParse(cbArriveFromM.Text, out minutes)) minutes = 0;
                arriveTimeFrom = new TimeSpan(hours, minutes, 0);
                // --> to
                if (!int.TryParse(tbArriveToH.Text, out hours))
                {
                    if (tbArriveToH.Text == "") { hours = 0; }
                    else throw new InvalidDataException("Invalid Arrive time hours (to)");
                }
                else
                {
                    if (maxHours < 0 || maxHours > 24) throw new InvalidDataException("Invalid Arrive time hours (to)");
                }

                if (!int.TryParse(cbArriveToM.Text, out minutes)) minutes = 0;
                arriveTimeTo = new TimeSpan(hours, minutes, 0);

                //DESCRIPTION validators
                validateItemsToMove(tbBoxes, ref boxes);
                validateItemsToMove(tbBeds, ref beds);
                validateItemsToMove(tbSofas, ref sofas);
                validateItemsToMove(tbFrigos, ref frigos);
                validateItemsToMove(tbWds, ref wds);
                validateItemsToMove(tbDesks, ref desks);
                validateItemsToMove(tbTables, ref tables);
                validateItemsToMove(tbChairs, ref chairs);
                validateItemsToMove(tbOther, ref other);

                oversized = (cbOversized.IsChecked == true) ? true : false;
                overweight = (cbOverweight.IsChecked == true) ? true : false;
                fragile = (cbFragile.IsChecked == true) ? true : false;
                expensive = (cbExpensive.IsChecked == true) ? true : false;

                details = cbDetails.Text;

                //PAYMENT INFO
                isCompletePayment = (cbComplete.IsChecked == true);

                //ORDER STATUS
                contactOnDate = new DateTime(2000, 1, 1);
                if (rbScheduled.IsChecked == true) os = Order.OrderStatus.Scheduled;
                else if (rbSuspended.IsChecked == true)
                {
                    os = Order.OrderStatus.Suspended;
                    contactOnDate = (contactOnDateIsChanged) ? dpContactOn.DisplayDate : new DateTime(2000, 1, 1);
                }
                else if (rbDone.IsChecked == true)
                {
                    os = Order.OrderStatus.Done;
                    if (cbDoneTotalH.IsEnabled==true)
                    {
                        if (cbDoneStartH.IsEnabled == false)
                        {
                            doneStartTime = new TimeSpan(0, 0, 0);
                            doneEndTime = new TimeSpan(0, 0, 0);
                            doneBreaksTime = new TimeSpan(0, 0, 0);

                            hours = getHours(cbDoneTotalH);
                            minutes = getMinutes(cbDoneTotalM);
                            doneTotalTime = new TimeSpan(hours, minutes, 0);
                        }
                        else
                        {
                            doneStartTime = new TimeSpan(0, 0, 0);
                            doneEndTime = new TimeSpan(0, 0, 0);
                            doneBreaksTime = new TimeSpan(0, 0, 0);
                            doneTotalTime = new TimeSpan(0, 0, 0);
                        }
                    }
                    else
                    {
                        if(cbDoneStartH.Text=="" && cbDoneEndH.Text=="" && cbDoneBreaksH.Text == "")
                        {
                            doneStartTime = new TimeSpan(0, 0, 0);
                            doneEndTime = new TimeSpan(0, 0, 0);
                            doneBreaksTime = new TimeSpan(0, 0, 0);
                            doneTotalTime = new TimeSpan(0, 0, 0);
                        }
                        else
                        {
                            doneStartTime = new TimeSpan(getHours(cbDoneStartH), getMinutes(cbDoneStartM), 0);
                            doneEndTime = new TimeSpan(getHours(cbDoneEndH), getMinutes(cbDoneEndM), 0);
                            doneBreaksTime = new TimeSpan(getHours(cbDoneBreaksH), getMinutes(cbDoneBreaksM), 0);
                            doneTotalTime = new TimeSpan(getHours(cbDoneTotalH), getMinutes(cbDoneTotalM), 0);
                        }
                    }
                }
                else throw new InvalidDataException("Order status is not selected");

                // ADDRESSES TO the LIST
                orderAddresses.Clear();
                // ==> Actual address
                addrIsBilling = (cbIsBillingAct.IsChecked == true) ?  true: false;
                if (tbAddrLineAct.Text == "")
                {
                    throw new InvalidDataException("Customer actual address is missing");
                }
                if (!validateTextBoxTextLength(tbAddrLineAct, 5, 150))
                {
                    throw new InvalidDataException("Address line must be 5 to 150 characters");
                }
                addrLine = tbAddrLineAct.Text;
                if (!int.TryParse(tbFloorAct.Text, out addrFloor))
                {
                    throw new InvalidDataException("Invalid floor value");
                }
                addrStairs = (cbStairsAct.IsChecked == true) ? true : false;
                addrElevator = (cbElevatorAct.IsChecked == true) ? true : false;
                //city
                if (tbCityAct.Text == "")
                {
                    throw new InvalidDataException("City line in actual address is missing");
                }
                if (!validateTextBoxTextLength(tbCityAct, 2, 50))
                {
                    throw new InvalidDataException("City name must be 2 to 50 characters");
                }
                addrCity = tbCityAct.Text;

                addrProvince = cbProvinceAct.Text;

                addrZip = tbZipAct.Text;
                if (addrZip == "")
                {
                    throw new InvalidDataException("ZIP line in actual address is missing");
                }
                else
                {
                    Match mZip = rZip.Match(addrZip);
                    if (!mZip.Success) throw new InvalidDataException("invalid ZIP in actual address");
                }
                addrNotes = tbNotesAct.Text;
                addrType = Address.AddrType.Actual;
                //add act address to the list
                orderAddresses.Add(new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes));


                // ==> Destination address
                addrIsBilling = (cbIsBillingDest.IsChecked == true) ? true : false;
                if (tbAddrLineDest.Text == "")
                {
                    throw new InvalidDataException("Customer destination address is missing");
                }
                if (!validateTextBoxTextLength(tbAddrLineDest, 5, 150))
                {
                    throw new InvalidDataException("Address line must be 5 to 150 characters");
                }
                addrLine = tbAddrLineDest.Text;
                if (!int.TryParse(tbFloorDest.Text, out addrFloor))
                {
                    throw new InvalidDataException("Invalid floor value");
                }
                addrStairs = (cbStairsDest.IsChecked == true) ? true : false;
                addrElevator = (cbElevatorDest.IsChecked == true) ? true : false;
                //city
                if (tbCityDest.Text == "")
                {
                    throw new InvalidDataException("City line in the destination address is missing");
                }
                if (!validateTextBoxTextLength(tbCityDest, 2, 50))
                {
                    throw new InvalidDataException("City name must be 2 to 50 characters");
                }
                addrCity = tbCityDest.Text;

                addrProvince = cbProvinceDest.Text;

                addrZip = tbZipDest.Text;
                if (addrZip == "")
                {
                    throw new InvalidDataException("ZIP line in the destination address is missing");
                }
                else
                {
                    Match mZip = rZip.Match(addrZip);
                    if (!mZip.Success) throw new InvalidDataException("invalid ZIP in the destination address");
                }
                addrNotes = tbNotesDest.Text;
                addrType = Address.AddrType.Destination;
                //add dest address to the list
                orderAddresses.Add(new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes));


                // ==> Intermediate address
                addrIsBilling = (cbIsBillingInt.IsChecked == true) ? true : false;
                if (cbUseIntermediateAddress.IsChecked == true)
                {
                    if (tbAddrLineInt.Text == "")
                    {
                        throw new InvalidDataException("Customer intermediate address is missing");
                    }
                    if (!validateTextBoxTextLength(tbAddrLineInt, 5, 150))
                    {
                        throw new InvalidDataException("Address line must be 5 to 150 characters");
                    }
                    addrLine = tbAddrLineInt.Text;
                    if (!int.TryParse(tbFloorInt.Text, out addrFloor))
                    {
                        throw new InvalidDataException("Invalid floor value");
                    }
                    addrStairs = (cbStairsInt.IsChecked == true) ? true : false;
                    addrElevator = (cbElevatorInt.IsChecked == true) ? true : false;
                    //city
                    if (tbCityInt.Text == "")
                    {
                        throw new InvalidDataException("City line in intermediate address is missing");
                    }
                    if (!validateTextBoxTextLength(tbCityInt, 2, 50))
                    {
                        throw new InvalidDataException("City name must be 2 to 50 characters");
                    }
                    addrCity = tbCityInt.Text;

                    addrProvince = cbProvinceInt.Text;

                    addrZip = tbZipInt.Text;
                    if (addrZip == "")
                    {
                        throw new InvalidDataException("ZIP line in the intermediate address is missing");
                    }
                    else
                    {
                        Match mZip = rZip.Match(addrZip);
                        if (!mZip.Success) throw new InvalidDataException("invalid ZIP in the intermediate address");
                    }
                    addrNotes = "";
                    addrType = Address.AddrType.Intermediate;
                    //add int address to the list
                    orderAddresses.Add(new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes));
                }


            }
            catch (InvalidDataException ex)
            {
                //TODO ERROR WINDOW
                System.Windows.MessageBox.Show(ex.Message);
                return; // MAYBE 
            }



        }

        private int getHours(ComboBox cbH)
        {
            int hours;
            if (!int.TryParse(cbH.Text, out hours))
            {
                if (tbArriveToH.Text == "") { hours = 0; }
                else throw new InvalidDataException("Invalid hours value in the Order Done section");
            }
            else
            {
                if (hours < 0 || hours > 23) throw new InvalidDataException("Invalid hours value in the Order Done section");
            }
            return hours;
        }

        private int getMinutes(ComboBox cbM)
        {
            int minutes;
            if (!int.TryParse(cbM.Text, out minutes)) minutes = 0;
            return minutes;
        }

        private bool validateTextBoxTextLength(TextBox tb, int min, int max)
        {
            int textLength = tb.Text.Length;
            return (textLength < min || textLength > max) ? false : true;
        }

        private void rbDone_Checked(object sender, RoutedEventArgs e)
        {
            cbDoneStartH.IsEnabled = true;
            cbDoneStartM.IsEnabled = true;
            cbDoneEndH.IsEnabled = true;
            cbDoneEndM.IsEnabled = true;
            cbDoneBreaksH.IsEnabled = true;
            cbDoneBreaksM.IsEnabled = true;
            cbDoneTotalH.IsEnabled = true;
            cbDoneTotalM.IsEnabled = true;

            dpContactOn.IsEnabled = false;
        }

        private void cbUseIntermediateAddress_Checked(object sender, RoutedEventArgs e)
        {
            tbAddrLineInt.IsEnabled = true;
            tbFloorInt.IsEnabled = true;
            cbStairsInt.IsEnabled = true;
            cbElevatorInt.IsEnabled = true;
            tbCityInt.IsEnabled = true;
            cbProvinceInt.IsEnabled = true;
            tbZipInt.IsEnabled = true;
            cbIsBillingInt.IsEnabled = true;
            spinFloorAddrInt.IsEnabled = true;
        }

        private void cbUseIntermediateAddress_Unchecked(object sender, RoutedEventArgs e)
        {
            tbAddrLineInt.IsEnabled = false;
            tbFloorInt.IsEnabled = false;
            cbStairsInt.IsEnabled = false;
            cbElevatorInt.IsEnabled = false;
            tbCityInt.IsEnabled = false;
            cbProvinceInt.IsEnabled = false;
            tbZipInt.IsEnabled = false;
            cbIsBillingInt.IsEnabled = false;
            spinFloorAddrInt.IsEnabled = false;
        }

        private void cbIsBillingDest_Checked(object sender, RoutedEventArgs e)
        {
            if (cbIsBillingDest.IsChecked == true)
            {
                cbIsBillingAct.IsChecked = false;
                cbIsBillingInt.IsChecked = false;
            }
        }

        private void cbIsBillingAct_Checked(object sender, RoutedEventArgs e)
        {
            if (cbIsBillingAct.IsChecked == true)
            {
                cbIsBillingDest.IsChecked = false;
                cbIsBillingInt.IsChecked = false;
            }
        }

        private void cbIsBillingInt_Checked(object sender, RoutedEventArgs e)
        {
            if (cbIsBillingInt.IsChecked == true)
            {
                cbIsBillingDest.IsChecked = false;
                cbIsBillingAct.IsChecked = false;
            }
        }


        private void tbTables_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(int.TryParse(tbTables.Text, out int number))
            {
                tbChairs.Text = (number * 4) + "";
                tbChairs.Background = Brushes.Yellow;
            }
            else
            {
                tbChairs.Text = "";
                tbChairs.Background = Brushes.White;
            }
        }

        
        private void tbInvoice_Click(object sender, RoutedEventArgs e)
        {
            InvoiceWindow dlg1 = new InvoiceWindow();
            if (dlg1.ShowDialog() == true)
            {

            }

        }

        private void rbScheduled_Checked(object sender, RoutedEventArgs e)
        {
            cbDoneEventsOn = false;
            cbDoneStartH.IsEnabled = false;
            cbDoneStartM.IsEnabled = false;
            cbDoneEndH.IsEnabled = false;
            cbDoneEndM.IsEnabled = false;
            cbDoneBreaksH.IsEnabled = false;
            cbDoneBreaksM.IsEnabled = false;
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;

            cbDoneStartH.Text = "";
            cbDoneStartM.SelectedIndex = -1;
            cbDoneEndH.Text = "";
            cbDoneEndM.SelectedIndex = -1;
            cbDoneBreaksH.Text = "";
            cbDoneBreaksM.SelectedIndex = -1;
            cbDoneTotalH.Text = "";
            cbDoneTotalM.SelectedIndex = -1;

            dpContactOn.IsEnabled = false;
            cbDoneEventsOn = true;
        }

        private void rbSuspended_Checked(object sender, RoutedEventArgs e)
        {
            cbDoneEventsOn = false;

            cbDoneStartH.IsEnabled = false;
            cbDoneStartM.IsEnabled = false;
            cbDoneEndH.IsEnabled = false;
            cbDoneEndM.IsEnabled = false;
            cbDoneBreaksH.IsEnabled = false;
            cbDoneBreaksM.IsEnabled = false;
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;

            cbDoneStartH.Text = "";
            cbDoneStartM.SelectedIndex = -1;
            cbDoneEndH.Text = "";
            cbDoneEndM.SelectedIndex = -1;
            cbDoneBreaksH.Text = "";
            cbDoneBreaksM.SelectedIndex = -1;
            cbDoneTotalH.Text = "";
            cbDoneTotalM.SelectedIndex = -1;

            dpContactOn.IsEnabled = true;
            contactOnDateIsChanged = false;

            cbDoneEventsOn = true;
        }

        //Calculation of total working time for the DONE order
        private void tryCalculateTheTotalTime()
        {
            int startH, startM, endH, endM, breakH=0, breakM;

            if (
                (int.TryParse(cbDoneStartH.Text, out startH)) &&
                (int.TryParse(cbDoneEndH.Text, out endH)) &&
                ((cbDoneBreaksH.Text=="")||(cbDoneBreaksH.Text!="" && int.TryParse(cbDoneBreaksH.Text, out breakH)))
                )
            {
                if (cbDoneBreaksH.Text == "") breakH = 0; 
                
                //minutes
                if (cbDoneBreaksM.SelectedIndex == -1) breakM = 0;
                else breakM = cbDoneBreaksM.SelectedIndex * 15;
                if (cbDoneEndM.SelectedIndex == -1) endM = 0;
                else endM = cbDoneEndM.SelectedIndex * 15;
                if (cbDoneStartM.SelectedIndex == -1) startM = 0;
                else startM = cbDoneStartM.SelectedIndex * 15;

                if (endH < startH) endH = endH + 24;
                int totalMinutes = endH * 60 - startH * 60 - breakH * 60 + endM - startM - breakM;
                if (totalMinutes < 0)
                {
                    cbDoneEventsOn = false;
                    cbDoneTotalH.Text = "";
                    cbDoneTotalM.SelectedIndex = -1;
                    cbDoneEventsOn = true;
                }
                int justMinutes = totalMinutes % 60;
                int justHours = (totalMinutes - justMinutes) / 60;
                cbDoneEventsOn = false;
                cbDoneTotalH.Text = justHours+"";
                cbDoneTotalM.SelectedIndex = justMinutes / 15;
                cbDoneEventsOn = true;
            }
            else
            {
                cbDoneEventsOn = false;
                cbDoneTotalH.Text = "";
                cbDoneTotalM.SelectedIndex = -1;
                cbDoneEventsOn = true;
            }
        }

        private void cbDoneStartH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            if (cbDoneBreaksH.Text == "") cbDoneBreaksH.Text = "0";
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;
            tryCalculateTheTotalTime();
        }

        private void cbDoneEndH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            if (cbDoneBreaksH.Text == "") cbDoneBreaksH.Text = "0";
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;
            tryCalculateTheTotalTime();
        }

        private void cbDoneBreaksH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;
            tryCalculateTheTotalTime();
        }

        private void cbDoneTotalH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            cbDoneStartH.IsEnabled = false;
            cbDoneStartM.IsEnabled = false;
            cbDoneEndH.IsEnabled = false;
            cbDoneEndM.IsEnabled = false;
            cbDoneBreaksH.IsEnabled = false;
            cbDoneBreaksM.IsEnabled = false;
        }

        private void cbDoneStartM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;
            tryCalculateTheTotalTime();
        }

        private void cbDoneEndM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;
            tryCalculateTheTotalTime();
        }

        private void cbDoneBreaksM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            cbDoneTotalH.IsEnabled = false;
            cbDoneTotalM.IsEnabled = false;
            tryCalculateTheTotalTime();
        }

        private void cbDoneTotalM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!cbDoneEventsOn) return;
            cbDoneStartH.IsEnabled = false;
            cbDoneStartM.IsEnabled = false;
            cbDoneEndH.IsEnabled = false;
            cbDoneEndM.IsEnabled = false;
            cbDoneBreaksH.IsEnabled = false;
            cbDoneBreaksM.IsEnabled = false;
        }

        //Some Validation Methods
        private void validateItemsToMove(TextBox tb, ref int itemTypeNumber)
        {
            if (tb.Text == "")
            {
                itemTypeNumber = 0;
            }
            else
            {
                if(!int.TryParse(tb.Text, out itemTypeNumber)){
                tb.Background = Brushes.Red;
                throw new InvalidDataException("Invalid number of items entered");
                }
                if (itemTypeNumber < 0)
                {
                tb.Background = Brushes.Red;
                throw new InvalidDataException("Negative number of items entered");
                }
            }
        }

        //calculating arrive time // TODO

        private void cbStartTimeH_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(cbStartTimeH.Text, out timeStartH))
            {

                cbArriveFromH.Text = timeStartH + "";
                tbArriveToH.Text = (timeStartH + 1) + "";
                cbArriveFromH.Background = Brushes.Yellow;
                tbArriveToH.Background = Brushes.Yellow;
            }
            else
            {
                cbArriveFromH.Background = Brushes.Yellow;
                tbArriveToH.Background = Brushes.Yellow;
            }
        }

        // --> SPIN BOXES-ITEMS 
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

        private void spinTravelTimeH_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbTravelTimeH, e);
        }

        private void spinArriveToH_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbArriveToH, e);
        }

        private void spinWorkers_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            getNewSpinValue(tbWorkers, e);
        }

        private void spinMaxHours_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbMaxHours, e);
        }

        private void spinMinHours_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbMinHours, e);
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

        
        // --> ZIP validations + Distance API
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
                    tb.Text = tb.Text.Substring(0, 3) + " " + tb.Text.Substring(3, 3);
                }
                tb.Text = (tb.Text).ToUpper();
                tbZipEventsOn = true;
                if (zipsOk[0] && zipsOk[1] && i != 2) // i!=2 - don't make api request in case entering intermediate address
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
                    tbGoogleDistance.Text = "";
                    tbGoogleTime.Text = "";
                }
                tb.Background = Brushes.LightYellow;
            }
        }

        //SOME EVENTS
        //-- uncheck selected truck
        private void btClearTruckSelection_Click(object sender, RoutedEventArgs e)
        {
            cbTruck.SelectedIndex = -1;
        }

        //Make white on hover

        private void cbArriveFromH_MouseEnter(object sender, MouseEventArgs e)
        {
            cbArriveFromH.Background = Brushes.White;
        }

        private void cbArriveFromM_MouseEnter(object sender, MouseEventArgs e)
        {
            cbArriveFromM.Background = Brushes.White;
        }

        private void tbTravelTimeH_MouseEnter(object sender, MouseEventArgs e)
        {
            tbTravelTimeH.Background = Brushes.White;
        }

        private void tbArriveToH_MouseEnter(object sender, MouseEventArgs e)
        {
            tbArriveToH.Background = Brushes.White;
        }

        private void tbBoxes_MouseEnter(object sender, MouseEventArgs e)
        {
            tbBoxes.Background = Brushes.White;
        }

        private void tbBeds_MouseEnter(object sender, MouseEventArgs e)
        {
            tbBeds.Background = Brushes.White;
        }

        private void tbSofas_MouseEnter(object sender, MouseEventArgs e)
        {
            tbSofas.Background = Brushes.White;
        }

        private void tbFrigos_MouseEnter(object sender, MouseEventArgs e)
        {
            tbFrigos.Background = Brushes.White;
        }

        private void tbWds_MouseEnter(object sender, MouseEventArgs e)
        {
            tbWds.Background = Brushes.White;
        }

        private void tbDesks_MouseEnter(object sender, MouseEventArgs e)
        {
            tbDesks.Background = Brushes.White;
        }

        private void tbTables_MouseEnter(object sender, MouseEventArgs e)
        {
            tbTables.Background = Brushes.White;
        }

        private void tbChairs_MouseEnter(object sender, MouseEventArgs e)
        {
            tbChairs.Background = Brushes.White;
        }

        private void tbOther_MouseEnter(object sender, MouseEventArgs e)
        {
            tbOther.Background = Brushes.White;
        }

        private void tbEmail_MouseEnter(object sender, MouseEventArgs e)
        {
            tbEmail.Background = Brushes.White;
        }

        private void cbStartTimeH_MouseEnter(object sender, MouseEventArgs e)
        {
            cbStartTimeH.Background = Brushes.White;
        }

        private void tbClient_MouseEnter(object sender, MouseEventArgs e)
        {
            tbClient.Background = Brushes.White;
        }

    }
}
