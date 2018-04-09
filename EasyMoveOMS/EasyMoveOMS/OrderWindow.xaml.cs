using Microsoft.Windows.Controls;
using MySql.Data.MySqlClient;
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
        Order currentOrder;
        List <Address> orderAddresses = new List<Address>() { new Address(), new Address(), new Address() };
        Order.OrderStatus os;
        Client orderClient;
        Truck orderTruck;


        bool isValid = false;
        bool isNewOrder=false;
        bool isNewClient = true;
        //bool isFirstAddrInsert = false;
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
        
        //Regex for Canadian Province
        static String rexProv = @"^(?:AB|BC|MB|N[BLTSU]|ON|PE|QC|SK|YT)$";
        static Regex rProv = new Regex(rexProv);
        
        //Regex for Email
        //static String rexEmail = @"^[ABCEGHJKLMNPRSTVXYabceghjklmnprstvxy][0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz] ?[0-9][ABCEGHJKLMNPRSTVWXYZabceghjklmnprstvwxyz][0-9]$";
        //static Regex rEmail = new Regex(rexEmail);


        //MOVING INFORMATION
        long orderId, truckId;
        DateTime moveDate, currentScheduleDay;
        TimeSpan moveTime, maxTime, minTime, workTime, travelTime, arriveTimeFrom, arriveTimeTo, 
            doneStartTime, doneEndTime, doneBreaksTime, doneTotalTime,
            timeTruckFrom, timeTruckTo;
        int startHour, startMinute, workTimeH, workTimeM, travelTimeH, travelTimeM, maxHours, maxMinutes, hours, minutes;
        int[] minGoo = new int[3] { 0, 0, 0 }; //to calculate estimated Google time - arr of SECONDS !!!
        int[] meterGoo = new int[3] { 0, 0, 0 }; //to calculate estimated Google distance - arr of meters
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
        bool useIntAddress;
        bool addrStairs, addrElevator, addrIsBilling;
        Address.AddrType addrType;

        public OrderWindow (Order order)
        {
            InitializeComponent();
            currentOrder = order;
            if (currentOrder == null)  
            {
                currentOrder = new Order();
                lvPayments.ItemsSource = currentOrder.orderPayments;
                isNewOrder = true;

                orderClient = new Client();
                orderClient.id = 0;
                orderTruck = new Truck();
                orderTruck.id = 0;
                //orderAddresses.Add(new Address());
                //orderAddresses.Add(new Address());
                //orderAddresses.Add(new Address());
            }
            else
            {
                isNewOrder = false;
                currentOrder = order;
                refreshPaymentsList();
                //lvPayments.ItemsSource = currentOrder.orderPayments;
                //orderClient = order.orderClient; // TODO LOAD INFO + change to existing client
                //orderTruck = order.orderTruck; 
            }

            cbTruck.ItemsSource = Globals.truckList;
        }

        // !!!!!!! --- GET Day Schedule --- !!!!!!!!!!!!
        private void dpMoveDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            movingDateIsChanged = true;
            DateTime dt = (DateTime)dpMoveDate.SelectedDate;
            if (isNewOrder) lblTitle.Content = "New Order - on " + dt.ToLongDateString();
            updateSchedule(dt);
        }

        private void updateSchedule(DateTime dt)
        {
            List<DayScheduleItem> dayScheduleItems = new List<DayScheduleItem>();
            try
            {
                dayScheduleItems = Globals.db.getDayScheduleData(dt);
            }
            catch (MySqlException)
            {
                return;
            }
            currentScheduleDay = dt;
            checkOverlap(ref dayScheduleItems);
            lvSchedule.ItemsSource = dayScheduleItems;
            lblSchedule.Content = "On " + dt.ToLongDateString()+ " schedule";// .ToString("yyyy-MM-dd") ;
        }

        private void checkOverlap(ref List<DayScheduleItem> dayScheduleItems)
        {
            foreach (DayScheduleItem d0 in dayScheduleItems) d0.overlap = false;
            foreach (DayScheduleItem d1 in dayScheduleItems)
            {
                foreach(DayScheduleItem d2 in dayScheduleItems)
                {
                    if (d1.orderId != d2.orderId)
                    {
                        //Check for overlaps
                        if (!((d1.timeTruckFrom.Ticks < d2.timeTruckFrom.Ticks && d1.timeTruckTo.Ticks < d2.timeTruckFrom.Ticks)
                            || (d1.timeTruckFrom.Ticks > d2.timeTruckTo.Ticks && d1.timeTruckTo.Ticks > d2.timeTruckFrom.Ticks)))
                        {
                            d1.overlap = true;
                            d2.overlap = true;
                        }
                    }
                }
            }
        }

        private void btOneDayBack_Click(object sender, RoutedEventArgs e)
        {
            currentScheduleDay=currentScheduleDay.AddDays(-1);
            updateSchedule(currentScheduleDay);
        }

        private void btOneDayForward_Click(object sender, RoutedEventArgs e)
        {
            currentScheduleDay=currentScheduleDay.AddDays(1);
            updateSchedule(currentScheduleDay);
        }
        //END SCHEDULE


        private void dpContactOn_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            contactOnDateIsChanged = true;
        }

        //SAVE
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            saveOrder();
        }

        private void saveOrder()
        {
            String confirmation = "";
            isValid = false;
            validateOrder();
            if (!isValid) return;

            // ==> Save new Client if it is NEW
            if (orderClient.id == 0)
            {
                orderClient = new Client(0, clientName, clientEmail, clientPhoneH, clientPhoneW);
                try
                {
                    clientId = Globals.db.saveNewClient(orderClient);
                    orderClient.id = clientId;
                    currentOrder.orderClient = orderClient;
                    confirmation = "==> Database: New client <" + orderClient.name + "> was saved\n";
                    //System.Windows.MessageBox.Show("Database: New client " + orderClient.name + " was saved");
                    tbClient.IsEnabled = false;
                    tbEmail.IsEnabled = false;
                    tbPhoneH.IsEnabled = false;
                    tbPhoneW.IsEnabled = false;
                    btSelectClient.Content = "Edit/Change";
                }
                catch (MySqlException er)
                {
                    System.Windows.MessageBox.Show("Database: Error saving order data \n" + er.Message);
                    return;
                }
            }

            // ==> SAVE ORDER data (or create new) in the database
            if (isNewOrder) currentOrder = new Order();
            fillCurrentOrderFromForm();
            if (isNewOrder)
            {
                try
                {
                    orderId = Globals.db.saveNewOrder(currentOrder);
                    currentOrder.id = orderId;
                    isNewOrder = false;
                    lblTitle.Content = "Order #" + currentOrder.id;
                    confirmation += "==> Database: New Order #" + currentOrder.id + " was created.\n";
                }
                catch (MySqlException ex)
                {
                    System.Windows.MessageBox.Show(confirmation + "Database error. Error saving new order.\n" + ex.Message);
                }
            }
            else
            {
                try
                {
                    Globals.db.updateOrder(currentOrder);
                    confirmation += "==> Order #" + currentOrder.id + " - Saved Ok\n";
                    //System.Windows.MessageBox.Show(confirmation);
                }
                catch (MySqlException ex)
                {
                    System.Windows.MessageBox.Show(confirmation + "Order - Error!\nAddresses - Error!" + ex.Message);
                }

            }

            // ==> SAVE ADDRESS data 
            if (orderAddresses[0].id == 0)
            {
                // initial (new addresses) insert into database
                try
                {
                    foreach (Address addr in orderAddresses)
                    {
                        addr.orderId = currentOrder.id;
                    }
                    long lastId = Globals.db.insertAddresses(orderAddresses);

                    //changing id-s from 0 to real
                    orderAddresses[0].id = lastId;
                    orderAddresses[1].id = lastId + 1;
                    orderAddresses[2].id = lastId + 2;
                    
                    confirmation += "Addresses - Ok";
                    System.Windows.MessageBox.Show(confirmation);
                }
                catch (MySqlException ex)
                {
                    System.Windows.MessageBox.Show(confirmation + "Addresses - Error! - " + ex.Message);
                }
            }
            else // order is already saved
            {
                try
                {
                    Globals.db.updateAddresses(orderAddresses);
                    confirmation += "Addresses - Ok";
                    System.Windows.MessageBox.Show(confirmation);
                }
                catch (MySqlException ex)
                {
                    System.Windows.MessageBox.Show(confirmation + "Addresses - Error! - " + ex.Message);
                }
                catch (InvalidDataException ex)
                {
                    System.Windows.MessageBox.Show(confirmation + "Addresses - Error! - " + ex.Message);
                }
            }
            currentOrder.orderAddresses = orderAddresses;
        }

        private void fillCurrentOrderFromForm()
        {
            currentOrder.orderAddresses = orderAddresses;
            currentOrder.moveDate = moveDate;
            currentOrder.moveTime = moveTime;

            currentOrder.orderClient = orderClient;
            currentOrder.orderTruck = orderTruck;

            currentOrder.workers = workers;
            currentOrder.pricePerHour = pricePerHour;
            currentOrder.maxTime = maxTime;
            currentOrder.minTime = minTime;
            currentOrder.deposit = deposit;
            currentOrder.workTime = workTime;
            currentOrder.travelTime = travelTime;
            currentOrder.timeTruckFrom = timeTruckFrom;
            currentOrder.timeTruckTo = timeTruckTo;
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
            currentOrder.useIntAddress = useIntAddress;
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
                moveDate = (DateTime)dpMoveDate.SelectedDate;

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
                if (orderClient.id == 0)
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

                //WORK TIME
                if (!int.TryParse(tbWorkTimeH.Text, out workTimeH))
                {
                    tbWorkTimeH.Background = Brushes.Red;
                    return;
                }
                if (workTimeH < 0)
                {
                    tbWorkTimeH.Background = Brushes.Red;
                    return;
                }
                if (!int.TryParse(cbTravelMinutes.Text, out workTimeM )) workTimeM = 0 ;
                workTime = new TimeSpan(workTimeH, workTimeM, 0);

                //TIME TRUCK Schedule - FROM >> TO

                TimeSpan halfOfTravelTime = new TimeSpan(travelTime.Ticks / 2); // in seconds

                //timeTruckFrom
                if (minGoo[0] > 0) //take Google time if exist
                {
                    TimeSpan travelToClient = new TimeSpan(0, 0, minGoo[0]);
                    
                    if (travelToClient.Ticks > moveTime.Ticks) timeTruckFrom = new TimeSpan(0, 0, 0);
                    else timeTruckFrom = moveTime - travelToClient;
                }
                else
                {
                    if (halfOfTravelTime.Ticks > moveTime.Ticks) timeTruckFrom = new TimeSpan(0, 0, 0);
                    else timeTruckFrom = moveTime - halfOfTravelTime;
                }

                //timeTruckTo
                if (minGoo[1] > 0) //take Google time if exist
                {
                    TimeSpan travelFromClient = new TimeSpan(0, 0, minGoo[1]);
                    if (new TimeSpan(travelFromClient.Ticks + workTime.Ticks+ moveTime.Ticks).Ticks > new TimeSpan(23,59,59).Ticks) timeTruckTo = new TimeSpan(23,59, 59);
                    else timeTruckTo = moveTime + workTime + travelFromClient;
                }
                else
                {
                    if ((halfOfTravelTime.Ticks + workTime.Ticks + moveTime.Ticks) > new TimeSpan(23, 59, 59).Ticks) timeTruckTo = new TimeSpan(23, 59, 59);
                    else timeTruckTo = moveTime + workTime + halfOfTravelTime;
                }


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

                if (checkProvince(cbProvinceAct)) addrProvince = cbProvinceAct.Text.ToUpper();

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
                //new actual address 
                Address newActual = new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes);

                newActual.id = orderAddresses[0].id;
                newActual.orderId = orderAddresses[0].orderId;

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

                if (checkProvince(cbProvinceDest)) addrProvince = cbProvinceDest.Text.ToUpper();
                
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
                Address newDestination = new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes);
                newDestination.id = orderAddresses[1].id;
                newDestination.orderId = orderAddresses[1].orderId;
                
                // ==> Intermediate address
                useIntAddress = false;
                if (cbUseIntermediateAddress.IsChecked == true)
                {
                    useIntAddress = true;
                    addrIsBilling = (cbIsBillingInt.IsChecked == true) ? true : false;
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

                    if (checkProvince(cbProvinceInt)) addrProvince = cbProvinceInt.Text.ToUpper();

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
                    
                    orderAddresses.Add(new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes));
                }
                else
                {
                    useIntAddress = false;
                    addrLine = ""; addrCity = ""; addrProvince = "QC"; addrNotes = "";
                    addrZip = ""; addrFloor = 1; addrStairs = false;
                    addrElevator = false; addrIsBilling = false; addrType = Address.AddrType.Intermediate;
                }
                //add int address to the list
                Address newIntermediate = new Address(addrLine, addrCity, addrZip, addrProvince, addrFloor, addrElevator, addrStairs, addrIsBilling, addrType, addrNotes);
                newIntermediate.id = orderAddresses[2].id;
                newIntermediate.orderId = orderAddresses[2].orderId;
                orderAddresses.Clear();
                orderAddresses.Add(newActual);
                orderAddresses.Add(newDestination);
                orderAddresses.Add(newIntermediate);

                // HERE ALL order information is valid
                isValid = true; 
            }
            catch (InvalidDataException ex)
            {
                //TODO ERROR WINDOW
                System.Windows.MessageBox.Show(ex.Message);
                return; // MAYBE 
            }
        }

        private bool checkProvince(ComboBox cb)
        {
            string pr = cb.Text.ToUpper();
            Match mProv = rProv.Match(pr);
            if (!mProv.Success)
            {
                cb.Background = Brushes.Red;
                throw new InvalidDataException("Invalid province code entered");
            }
            cb.Background = Brushes.LightGreen;
            return true;
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
            saveOrder();
            InvoiceWindow dlg1 = new InvoiceWindow(currentOrder);
            if (dlg1.ShowDialog() == true)
            {

            }

        }

        private void cbProvince_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((ComboBox)sender).Background = Brushes.White;
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

        

        private void tbWorkTimeH_TextChanged(object sender, TextChangedEventArgs e)
        {
            tbWorkTimeH.Background = Brushes.White;
        }

        private void cbComplete_Checked(object sender, RoutedEventArgs e)
        {
            tbPaymentsTotal.Background = Brushes.LightGreen;
        }

        private void cbComplete_Unchecked(object sender, RoutedEventArgs e)
        {
            tbPaymentsTotal.Background = Brushes.White;
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
        private void spinWorkTimeH_Spin(object sender, SpinEventArgs e)
        {
            getNewSpinValue(tbWorkTimeH, e);
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
            if (tbZipEventsOn) checkZip(tbZipInt.Text, 2, sender);
        }

        private void checkZip(string tmp, int i, object sender)
        {
            TextBox tb = (TextBox)sender;
            Match mZip = rZip.Match(tmp);
            if (mZip.Success)
            {
                zipsOk[i] = true;
                //calculateGoogle
                if (tb.Background != Brushes.LightGreen)
                {
                    //calculate Google Data
                    googleData(tmp, i);
                }

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
                        tbGoogleDistance.Content = goo.rows[0].elements[0].distance.text.ToString();
                        tbGoogleTime.Content = goo.rows[0].elements[0].duration.text.ToString();
                    }
                    catch (Exception)
                    {
                        tbGoogleDistance.Content = "...";
                        tbGoogleTime.Content = "...";
                    }
                }
            }
            else
            {
                zipsOk[i] = false;
                if (i == 0 || i == 1)
                {
                    tbGoogleDistance.Content = "...";
                    tbGoogleTime.Content = "...";
                }
                tb.Background = Brushes.LightYellow;
            }
        }

        private void googleData(string tmp, int i)
        {
            if (i == 0)
            {
                string url = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + Globals.truckParkingZip + "&destinations=" + tmp + "&key=" + Globals.APIkey;
                try
                {
                    string json = Api.getJson(url);
                    GoogleMatrixData goo = JsonConvert.DeserializeObject<GoogleMatrixData>(json);
                    if(goo.rows[0].elements[0].status!="OK") throw new Exception();
                    tbGoogleDistanceTo.Content = goo.rows[0].elements[0].distance.text.ToString();
                    tbGoogleTimeTo.Content = goo.rows[0].elements[0].duration.text.ToString();
                    int.TryParse(goo.rows[0].elements[0].distance.value+"", out meterGoo[0]);
                    int.TryParse(goo.rows[0].elements[0].duration.value + "", out minGoo[0]);
                    tryCalculateGooEstimation();
                    String s;
                    s = goo.destination_addresses[0];
                    String[] sa = s.Split(',');
                    cbProvinceAct.Text = sa[1].Substring(1, 2);
                    tbCityAct.Text = sa[0];
                    tbCityAct.Background = Brushes.LightGreen;

                }
                catch (Exception)
                {
                    meterGoo[0] = 0;
                    minGoo[0] = 0;
                    tryCalculateGooEstimation();
                    tbGoogleDistanceTo.Content = "...";
                    tbGoogleTimeTo.Content = "...";
                    tbCityAct.Background = Brushes.OrangeRed;
                }
            }
            else if (i == 1){
                string url = "https://maps.googleapis.com/maps/api/distancematrix/json?origins=" + tmp + "&destinations=" + Globals.truckParkingZip + "&key=" + Globals.APIkey;
                try
                {
                    string json = Api.getJson(url);
                    GoogleMatrixData goo = JsonConvert.DeserializeObject<GoogleMatrixData>(json);
                    if (goo.rows[0].elements[0].status != "OK") throw new Exception();
                    tbGoogleDistanceFrom.Content = goo.rows[0].elements[0].distance.text.ToString();
                    tbGoogleTimeFrom.Content = goo.rows[0].elements[0].duration.text.ToString();
                    int.TryParse(goo.rows[0].elements[0].distance.value + "", out meterGoo[1]);
                    int.TryParse(goo.rows[0].elements[0].duration.value + "", out minGoo[1]);
                    tryCalculateGooEstimation();
                    String s;
                    s = goo.origin_addresses[0];
                    String[] sa = s.Split(',');
                    cbProvinceDest.Text = sa[1].Substring(1, 2);
                    tbCityDest.Text = sa[0];
                    tbCityDest.Background = Brushes.LightGreen;

                }
                catch (Exception)
                {
                    meterGoo[1] = 0;
                    minGoo[1] = 0;
                    tryCalculateGooEstimation();
                    tbGoogleDistanceFrom.Content = "...";
                    tbGoogleTimeFrom.Content = "...";
                    tbCityDest.Background = Brushes.OrangeRed;
                }
            }
        }

        private void tryCalculateGooEstimation()
        {
            if (meterGoo[0] > 0 && meterGoo[1] > 0 && minGoo[0] > 0 && minGoo[1] > 0)
            {
                meterGoo[2] = meterGoo[0] + meterGoo[1];
                double fkm = (double)meterGoo[2] / 1000;
                String km = Math.Round(fkm, 1) + " km";

                minGoo[2] = minGoo[0] + minGoo[1];
                double h = minGoo[2] / 3600;
                String hh = Math.Round(h, 0) + " h  ";
                double m = (minGoo[2] - (h * 3600)) / 60;
                String mm = Math.Round(m, 0) + " m";

                tbGoogleDistanceTT.Content = km;
                tbGoogleTimeTT.Content = hh + mm;
            }
            else
            {
                tbGoogleDistanceTT.Content = "......";
                tbGoogleTimeTT.Content = "......";
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

        private void btAddPayment_Click(object sender, RoutedEventArgs e)
        {
            if (currentOrder.id == 0)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show("Order must be saved before adding payments\n\nSave the order?", "Add payment", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                if (result == MessageBoxResult.Cancel) return;
                else saveOrder(); // user pushed OK

                if (currentOrder.id == 0) return; // if saving faled
            }
            
            //creating new Payment
            Payment p = new Payment();
            p.orderId = currentOrder.id;

            DlgAddPayment dlgP = new DlgAddPayment(p);
            if (dlgP.ShowDialog() == true)
            {
                refreshPaymentsList();
            }
        }

        private void refreshPaymentsList()
        {
            try
            {
                List<Payment> payments = Globals.db.getAllPayments(currentOrder.id);
                lvPayments.ItemsSource = payments;
                currentOrder.orderPayments = payments;
                //set Total
                decimal pTotal=0;
                if (payments.Count > 0)
                {
                    foreach(Payment p in payments)
                    {
                        pTotal += p.amount;
                    }
                    tbPaymentsTotal.Text = pTotal + "";
                    cbComplete.IsEnabled = true;
                }
                else
                {
                    tbPaymentsTotal.Text = "";
                    cbComplete.IsChecked = false;
                    cbComplete.IsEnabled = false;
                }
            }
            catch (MySqlException exx)
            {
                System.Windows.MessageBox.Show("Payments were not loaded:\n\n" + exx.Message, "Database error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                return;
            }
        }

        private void lvPayments_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Payment p = (Payment)lvPayments.SelectedItem;
            if (p == null)
            {
                return;
            }
            DlgAddPayment dlgP = new DlgAddPayment(p);
            if (dlgP.ShowDialog() == true)
            {
                refreshPaymentsList();
            }
        }
    }
}
