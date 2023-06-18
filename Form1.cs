using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace BurnabyWebReg
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }



        Thread workerThread1; 
        Thread workerThread2;
        private volatile bool _shouldStop = true;




        CommonClass cc = new CommonClass(); // common class
        Strings st = new Strings(); // all string variable
        WebControl wc = new WebControl(); // webrequest class
        UIControl uc = new UIControl();  // Form UI control class
















        // Login
        private void DoWork_Login()
        {
            try
            {
                // Home page
                wc.GetSend(st.StartUrl);

                // Login
                isLogin();

                // MyAccount page
                st.Result = wc.GetSend(st.MyAccountUrl);

                st.LoginName = cc.getBetween(st.Result, "Logged in as", "</div>");
                st.LoginName = cc.RemoveWhitespace(st.LoginName);
                st.LoginName = st.LoginName.Replace("&nbsp;", "");
                uc.changeControlText(label7, st.LoginName);

                st.CurrentBalance = cc.getBetween(st.Result, "Current Balance:", "</td>");
                st.FutureBalance = cc.getBetween(st.Result, "Future Balance:", "</td>");
                uc.changeControlText(label8, st.CurrentBalance);
                uc.changeControlText(label9, st.FutureBalance);

                // ClientID(Participant) - Different from Client Number when logging in
                // Required when adding to your shopping cart                
                string participantSource = cc.getBetween(st.Result, "id=\"registrants-details\"", "id=\"registration-history\"");
                string[] participantArr = Regex.Split(participantSource, "MyAccountHistoryDetails.asp");

                foreach (string participant in participantArr)
                {
                    if (participant.Contains("Clients=") && participant.Contains("title=\""))
                    {
                        st.ParticipantID = cc.getBetween(participant, "Clients=", "\"");
                        st.ParticipantName = cc.getBetween(participant, "title=\"", ":");                        

                        if (st.ParticipantID != "" && st.ParticipantName != "")
                        {
                            // Add if duplicate key does not exist
                            if (!SharedClient.ClientDic.ContainsKey(st.ParticipantName))
                            {
                                SharedClient.ClientDic.Add(st.ParticipantName, st.ParticipantID);
                            }
                        }

                    } // if
                } // foreach

                // Current / Future Registrations
                string registrationSource = cc.getBetween(st.Result, "id=\"registration-history\"", "id=\"footer\"");
                string[] registrationArr = Regex.Split(registrationSource, "</tr>"); // Array separation

                // Clear dataGridView               
                this.Invoke(new MethodInvoker(delegate ()
                {
                    dataGridView2.Rows.Clear();
                }));  // invoke                               

                foreach (string registration in registrationArr)
                {
                    if (registration.Contains("headers=\"Client\""))
                    {
                        st.ClientName = cc.getBetween(registration, "headers=\"Client\"", "</td>");
                        st.ClientName = cc.getBetween(st.ClientName, "<a", "/a>");
                        st.ClientName = cc.getBetween(st.ClientName, ">", "<");
                        st.ClientName = cc.RemoveSpace(st.ClientName);

                        st.ClientCourseTitle = cc.getBetween(registration, "headers=\"Client CourseTitle\"", "</td>");
                        st.ClientCourseTitle = cc.getBetween(st.ClientCourseTitle, "<a", "/a>");
                        st.ClientCourseTitle = cc.getBetween(st.ClientCourseTitle, ">", "<");
                        st.ClientCourseTitle = cc.RemoveSpace(st.ClientCourseTitle);

                        st.ClientType = cc.getBetween(registration, "headers=\"Client Type\"", "/td>");
                        st.ClientType = cc.getBetween(st.ClientType, ">", "<");
                        st.ClientType = cc.RemoveSpace(st.ClientType);

                        st.ClientBarCode = cc.getBetween(registration, "headers=\"Client BarCode\"", "/td>");
                        st.ClientBarCode = cc.getBetween(st.ClientBarCode, ">", "<");
                        st.ClientBarCode = cc.ReplaceSpecialString(st.ClientBarCode);
                        st.ClientBarCode = cc.RemoveSpace(st.ClientBarCode);

                        st.ClientDateSpan = cc.getBetween(registration, "headers=\"Client DateSpan\"", "/td>");
                        st.ClientDateSpan = cc.getBetween(st.ClientDateSpan, ">", "<");
                        st.ClientDateSpan = cc.RemoveSpace(st.ClientDateSpan);

                        st.ClientFee = cc.getBetween(registration, "headers=\"Client Fee\"", "/td>");
                        st.ClientFee = cc.getBetween(st.ClientFee, ">", "<");
                        st.ClientFee = cc.RemoveSpace(st.ClientFee);

                        if (st.ClientName != "")
                        {
                            // Cross-thread protection
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                // add DatagridView
                                dataGridView2.Rows.Add(st.ClientName, st.ClientCourseTitle, st.ClientType,
                                    st.ClientBarCode, st.ClientDateSpan, st.ClientFee);
                            }));  // invoke
                        }

                    } // if                  
                } // foreach  

                if (st.ClientName == "")
                {
                    MessageBox.Show("Invalid login");
                }

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }


        // Keyword Search
        private void DoWork_Search()
        {
            try
            {
                // Start Page
                wc.GetSend(st.StartUrl);


                if (radioButton1.Checked)
                {
                    // Keyword Search
                    st.Keyword = textBox5.Text.Trim();
                    if (st.Keyword == "")
                    {
                        MessageBox.Show("Please enter a keyword!");
                        return;
                    }
                    // Keyword Encode
                    st.Keyword = HttpUtility.UrlEncode(st.Keyword);
                    st.PostData = "AllCatogerySubcatogerySelectedHintText=All+categories%2Fsubcategories+&KeywordSearch=" + st.Keyword + "&SuperDropDownFrom=dd-mm-yyyy&SuperDropDownFrom=0&SuperDropDownFrom=0&SuperDropDownFrom=0&DateRangeFrom=&SuperDropDownTo=dd-mm-yyyy&SuperDropDownTo=0&SuperDropDownTo=0&SuperDropDownTo=0&DateRangeTo=&chkWeekDay8=9&RegistrantAgeSearch=&AgeSearch=Y&chkKeywordRegistrationAvailable=&ajax=true";
                    st.Result = wc.PostSend(st.PostData, st.SearchUrl + "?AdvSearch=true");

                    if (st.Result.Contains("BlockedSessionCookies") || st.Result.Contains("sessioncheck"))
                    {
                        wc.GetSend(st.StartUrl);
                        Thread.Sleep(100);
                        st.Result = wc.PostSend(st.PostData, st.SearchUrl + "?AdvSearch=true");
                    }

                    // Clear dataGridView               
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        dataGridView1.Rows.Clear();
                    }));  // invoke 

                    // Search Result
                    string activitySource = cc.getBetween(st.Result, "class=\"search-result\"", "<tfoot>");
                    string[] activityArr = Regex.Split(activitySource, "</tr>"); 

                    foreach (string activity in activityArr)
                    {
                        if (activity.Contains("class=\"activity-title\""))
                        {
                            st.ActivityTitle = cc.getBetween(activity, "class=\"activity-title\"", "/div>");
                            st.ActivityTitle = cc.getBetween(st.ActivityTitle, ">", "<");
                            st.ActivityTitle = cc.ReplaceSpecialString(st.ActivityTitle);
                            st.ActivityTitle = cc.RemoveSpace(st.ActivityTitle);

                            st.ActivityDesc = cc.getBetween(activity, "class=\"activity-desc\"", "/div>");
                            st.ActivityDesc = cc.getBetween(st.ActivityDesc, ">", "<");
                            st.ActivityDesc = cc.ReplaceSpecialString(st.ActivityDesc);
                            st.ActivityDesc = cc.RemoveSpace(st.ActivityDesc);

                            st.Aid = cc.getBetween(activity, "showActivityDetail", ")");
                            st.Aid = cc.getBetween(st.Aid, "aid=", "'");
                            
                            this.Invoke(new MethodInvoker(delegate ()
                            {
                                // add DataGridView
                                dataGridView1.Rows.Add(st.ActivityTitle, st.ActivityDesc, "Show Courses", st.Aid);
                            }));  // invoke

                        } // if
                    } // foreach

                    // If the total record is more than 10, you need to search more
                    if (st.Result.Contains("Activity-Records-Total-input"))
                    {
                        string recordsTotalInput = cc.getBetween(st.Result, "Activity-Records-Total-input", ">");
                        recordsTotalInput = cc.getBetween(recordsTotalInput, "value=\"", "\"");
                        int recordsTotal = Int32.Parse(recordsTotalInput) - 1;
                        int iDivide = recordsTotal / 10;
                        for (int i = 1; i <= iDivide; i++)
                        {
                            DoWork_SearchNext(i * 10, i);
                        }
                    }
                }

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }

        // search Next Page
        private void DoWork_SearchNext(int iDisplayStart, int sEcho)
        {
            try
            {
                // Get send
                string url = "https://webreg.burnaby.ca/webreg/Activities/ActivitiesAdvSearch.asp?GetPagingData=true&GetCourses=true&sEcho=" + sEcho + "&iColumns=1&sColumns=&iDisplayStart=" + iDisplayStart + "&iDisplayLength=10&ajax=true";
                st.Result = wc.GetSend(url);

                // Search Result               
                string[] activityArr = Regex.Split(st.Result, "</tr>"); 

                foreach (string activity in activityArr)
                {
                    if (activity.Contains("class=\"activity-title\""))
                    {
                        st.ActivityTitle = cc.getBetween(activity, "class=\"activity-title\"", "/div>");
                        st.ActivityTitle = cc.getBetween(st.ActivityTitle, ">", "<");
                        st.ActivityTitle = cc.ReplaceSpecialString(st.ActivityTitle);
                        st.ActivityTitle = cc.RemoveSpace(st.ActivityTitle);

                        st.ActivityDesc = cc.getBetween(activity, "class=\"activity-desc\"", "/div>");
                        st.ActivityDesc = cc.getBetween(st.ActivityDesc, ">", "<");
                        st.ActivityDesc = cc.ReplaceSpecialString(st.ActivityDesc);
                        st.ActivityDesc = cc.RemoveSpace(st.ActivityDesc);

                        st.Aid = cc.getBetween(activity, "showActivityDetail", ")");
                        st.Aid = cc.getBetween(st.Aid, "aid=", "'");
                       
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            // add DataGridView
                            dataGridView1.Rows.Add(st.ActivityTitle, st.ActivityDesc, "Show Courses", st.Aid);
                        }));  // invoke

                    } // if
                } // foreach  

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }





        // Login
        private void button1_Click(object sender, EventArgs e)
        {
            st.ClientBarcode = textBox1.Text.Trim();
            st.AccountPIN = textBox2.Text.Trim();

            if (st.ClientBarcode == "" || st.AccountPIN == "")
            {
                MessageBox.Show("Please enter a valid login ID");
                return;
            }

            // Thread
            workerThread1 = new Thread(new ThreadStart(DoWork_Login));
            // If the thread did not start
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(DoWork_Login));
                workerThread1.Start();
                // To prevent the Main function from terminating the worker thread before it even runs                    
                while (!workerThread1.IsAlive) ;
            }

            // Start Page - Explore every 10 minutes to ensure that the session is not disconnected
            timer3.Interval = 60000 * 10;

            timer3.Start();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            // Time setting - to 10 a.m
            dateTimePicker1.Value = DateTime.Today.AddHours(10);

            comboBox1.SelectedIndex = 0;

            string myexePath = Application.StartupPath; 
            string filename = "setting.ini";
            string pathfilename = myexePath + @"\" + filename;
            FileInfo fi = new FileInfo(pathfilename);
            // FileInfo.Exists
            if (fi.Exists)
            {
                // read Ini file
                IniFile ini = new IniFile();
                ini.Load(pathfilename);
                uc.changeControlText(textBox1, ini["Client"]["ClientBarcode"].GetString());

                uc.changeControlText(textBox5, ini["Client"]["Keyword"].GetString());
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string clientBarcode = textBox1.Text.Trim();
            if (clientBarcode != "")
            {
                // write to Ini file
                IniFile ini = new IniFile();
                ini["Client"]["ClientBarcode"] = clientBarcode;
                string myexePath = Application.StartupPath;
                string filename = "setting.ini";
                string pathfilename = myexePath + @"\" + filename;
                ini.Save(pathfilename);

                string keyword = textBox5.Text.Trim();
                if (keyword != "")
                {
                    ini["Client"]["Keyword"] = keyword;
                    ini.Save(pathfilename);
                }
            }

            // Stop all Timers
            timer1.Stop();
            timer2.Stop();
            timer3.Stop();
        }






        // Search
        private void button5_Click(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                // Barcode Search
                st.Barcode = textBox5.Text.Trim();
                if (st.Barcode == "")
                {
                    MessageBox.Show("Please enter a valid course bar code number!");
                    return;
                }

                // Send data to form3 via constructor
                Form3 form3 = new Form3();
                form3.Show();

                // Send data in form3 using method               
                form3.GetDataFromForm(st.Barcode);
            }
            else
            {
                // Keyword Search
                workerThread1 = new Thread(new ThreadStart(DoWork_Search));
                if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                {
                    workerThread1 = new Thread(new ThreadStart(DoWork_Search));
                    workerThread1.Start();
                    while (!workerThread1.IsAlive) ;
                }
            }
        }




        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns[2].Index)
            {
                st.Aid = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();

                // Send data to form2 via constructor
                Form2 form2 = new Form2();
                form2.Show();

                // Send data to form2 using method                
                form2.GetDataFromForm(st.Aid);
            }
        }

        // Search TextBox
        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            // If Enter Key occurs
            if (e.KeyCode == Keys.Enter)
            {
                // Search Button Click
                button5_Click(this, new EventArgs());
            }
        }

        // Login TextBox
        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            // If Enter Key occurs
            if (e.KeyCode == Keys.Enter)
            {
                // Login Button Click
                button1_Click(this, new EventArgs());
            }
        }

        // View Full Purchase History
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Form6 form6 = new Form6();
            form6.Show();
        }

        // Refresh icon click
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            workerThread2 = new Thread(new ThreadStart(DoWork_CurrentRegistrations));
            if ((workerThread2.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread2 = new Thread(new ThreadStart(DoWork_CurrentRegistrations));
                workerThread2.Start();
                while (!workerThread2.IsAlive) ;
            }
        }

        // Current / Future Registrations
        private void DoWork_CurrentRegistrations()
        {
            try
            {
                // MyAccount page
                st.Result = wc.GetSend(st.MyAccountUrl);

                // Current / Future Registrations
                string registrationSource = cc.getBetween(st.Result, "id=\"registration-history\"", "id=\"footer\"");
                string[] registrationArr = Regex.Split(registrationSource, "</tr>");

                // Clear dataGridView               
                this.Invoke(new MethodInvoker(delegate ()
                {
                    dataGridView2.Rows.Clear();
                }));  // invoke 

                foreach (string registration in registrationArr)
                {
                    if (registration.Contains("headers=\"Client\""))
                    {
                        st.ClientName = cc.getBetween(registration, "headers=\"Client\"", "</td>");
                        st.ClientName = cc.getBetween(st.ClientName, "<a", "/a>");
                        st.ClientName = cc.getBetween(st.ClientName, ">", "<");
                        st.ClientName = cc.RemoveSpace(st.ClientName);

                        st.ClientCourseTitle = cc.getBetween(registration, "headers=\"Client CourseTitle\"", "</td>");
                        st.ClientCourseTitle = cc.getBetween(st.ClientCourseTitle, "<a", "/a>");
                        st.ClientCourseTitle = cc.getBetween(st.ClientCourseTitle, ">", "<");
                        st.ClientCourseTitle = cc.RemoveSpace(st.ClientCourseTitle);

                        st.ClientType = cc.getBetween(registration, "headers=\"Client Type\"", "/td>");
                        st.ClientType = cc.getBetween(st.ClientType, ">", "<");
                        st.ClientType = cc.RemoveSpace(st.ClientType);

                        st.ClientBarCode = cc.getBetween(registration, "headers=\"Client BarCode\"", "/td>");
                        st.ClientBarCode = cc.getBetween(st.ClientBarCode, ">", "<");
                        st.ClientBarCode = cc.RemoveSpace(st.ClientBarCode);

                        st.ClientDateSpan = cc.getBetween(registration, "headers=\"Client DateSpan\"", "/td>");
                        st.ClientDateSpan = cc.getBetween(st.ClientDateSpan, ">", "<");
                        st.ClientDateSpan = cc.RemoveSpace(st.ClientDateSpan);

                        st.ClientFee = cc.getBetween(registration, "headers=\"Client Fee\"", "/td>");
                        st.ClientFee = cc.getBetween(st.ClientFee, ">", "<");
                        st.ClientFee = cc.RemoveSpace(st.ClientFee);

                        if (st.ClientName != "")
                        {                           
                            this.Invoke(new MethodInvoker(delegate ()
                            {                               
                                dataGridView2.Rows.Add(st.ClientName, st.ClientCourseTitle, st.ClientType,
                                    st.ClientBarCode, st.ClientDateSpan, st.ClientFee);
                            }));  // invoke
                        }

                    } // if                  
                } // foreach  

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }

        // Welcome to WebReg
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // Open Homepage 
            wc.OpenUrl(st.StartUrl);                        
        }

        // Hover event to pictureBox
        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox1, "Refresh Current Registrations");
        }









        // Reservation Tabcontrol  

        // Reservation Add
        private void button2_Click(object sender, EventArgs e)
        {            
            st.Barcode = textBox3.Text.Trim();
            if (st.Barcode == "")
            {
                MessageBox.Show("Enter a Barcode!");
                return;
            }

            int index = comboBox1.SelectedIndex;
            if (index == 0)
            {
                MessageBox.Show("Select a Client!");
                return;
            }

            // Get courseDetails and Add a data to dataGridView
            CourseDetails();            
        }

        // Reservation
        private void button3_Click(object sender, EventArgs e)
        {
            button3.Enabled = false;
            button4.Enabled = true;

            // Reservation timer

            // Set the timer interval to 1 second
            timer1.Interval = 1000;

            // Start the timer
            timer1.Start();


            // CountDown timer

            // Set the initial selected time to the DateTimePicker
            st.SelectedTime = dateTimePicker1.Value;

            // Set the interval of the timer to 1 second
            timer2.Interval = 1000;

            // Start the timer
            timer2.Start();            
        }

        // Stop
        private void button4_Click(object sender, EventArgs e)
        {
            button3.Enabled = true;
            button4.Enabled = false;

            timer1.Stop();
            timer2.Stop();

            _shouldStop = true; // Stop working flag                

            if (workerThread1.IsAlive == true)  // If the thread is working
            {
                workerThread1.Interrupt();  // Stop Thread 1 
            }
        }

        // Reservation
        private void timer1_Tick(object sender, EventArgs e)
        {
            // Get the current time
            DateTime currentTime = DateTime.Now;

            // Get the value of the DateTimePicker
            DateTime pickerValue = dateTimePicker1.Value;

            // Check if the current time matches the value of the DateTimePicker
            if (currentTime.Hour == pickerValue.Hour &&
                currentTime.Minute == pickerValue.Minute &&
                currentTime.Second == pickerValue.Second)
            {
                // Call the function to be executed when the current time matches the DateTimePicker value
                _shouldStop = false;  // Start Job Flag
               
                workerThread1 = new Thread(new ThreadStart(DoWork_Reservation));
                if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
                {
                    workerThread1 = new Thread(new ThreadStart(DoWork_Reservation));
                    workerThread1.Start();
                    while (!workerThread1.IsAlive) ;
                }

                // Stop timer
                timer1.Stop();
                timer2.Stop();
            }  
        }

        // Reservation
        private void DoWork_Reservation()
        {
            // Cart success
            bool cartOK = false;

            // Checkout Payment  
            bool checkoutOK = false;              

            try
            {

                while (!_shouldStop)
                {
                    int dgvCount = dataGridView3.Rows.Count;

                    // Iterate over each row in the DataGridView
                    for (int row = 0; row < dgvCount; row++)
                    {
                        st.Barcode = dataGridView3.Rows[row].Cells[0].Value.ToString();
                        st.ParticipantName = dataGridView3.Rows[row].Cells[1].Value.ToString();
                        st.ParticipantID = dataGridView3.Rows[row].Cells[4].Value.ToString(); 

                        // Get Aid and Cid
                        GetCid();

                        // Check for success in cart
                        while (!cartOK)
                        {                           
                            cartOK = isCart();
                            Thread.Sleep(100);
                        }                                               

                        Thread.Sleep(100);
                    }  // for


                    // Checkout Success Check
                    while (!checkoutOK)
                    {
                        checkoutOK = isCheckout();
                        Thread.Sleep(100);
                    }


                    // If it's completed
                    if (checkoutOK)
                    {
                        // Exit the program
                        _shouldStop = true;
                        timer1.Stop();

                        // Refresh Current / Future Registrations
                        DoWork_CurrentRegistrations();

                        MessageBox.Show("All Completed!");
                        break;
                    }

                    Thread.Sleep(100);                

                } // while                

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }

        // Get Course Details, Add data to dataGridView
        private void CourseDetails()
        {
            try
            {
                // Initialization
                st.Course = "";
                st.Times = "";
                st.Dates = "";
                st.ParticipantName = "";
                st.ParticipantID = "";

                st.Barcode = textBox3.Text.Trim();
                st.ParticipantName = comboBox1.Text;

                // Get the selected item Value from the ComboBox. (Key, Value Pair)
                //comboBox1.DisplayMember = "Display";  // Data visible in form, text can be changed
                //comboBox1.ValueMember = "Value";  // Data to be used internally, text can be changed
                var selectedItem = comboBox1.SelectedItem.GetType().GetProperty(comboBox1.ValueMember).GetValue(comboBox1.SelectedItem);
                st.ParticipantID = selectedItem.ToString();

                // Start Page
                wc.GetSend(st.StartUrl);

                st.PostData = "SearchType=IVRSearch&cbarcode=" + st.Barcode + "&ajax=true";

                st.Result = wc.PostSend(st.PostData, st.SearchUrl);

                string activityCourseRow = cc.getBetween(st.Result, "id=\"activity-course-row\"", "</tbody>");
                if (activityCourseRow.Contains("headers=\"Course\""))
                {
                    st.Course = cc.getBetween(activityCourseRow, "headers=\"Course\"", "</td>");
                    st.Course = cc.getBetween(st.Course, "<div", "div>");
                    st.Course = cc.getBetween(st.Course, "\">", "</");
                    st.Course = cc.RemoveSpace(st.Course);

                    st.Times = cc.getBetween(activityCourseRow, "headers=\"Times\"", "<td");
                    st.Times = cc.getBetween(st.Times, ">", "</td>");
                    st.Times = cc.RemoveSpace(st.Times);
                    st.Times = cc.remove_html_tag(st.Times);

                    st.Dates = cc.getBetween(activityCourseRow, "headers=\"Dates\"", "<td");
                    st.Dates = cc.getBetween(st.Dates, ">", "</td>");
                    st.Dates = cc.RemoveSpace(st.Dates);
                    st.Dates = cc.remove_html_tag(st.Dates);                    
                }
              
                this.Invoke(new MethodInvoker(delegate ()
                {                   
                    dataGridView3.Rows.Add(st.Barcode, st.ParticipantName, st.Course,
                        st.Dates + " " + st.Times, st.ParticipantID);
                }));  // invoke

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }


        // Get Aid, Cid from Course Details
        // If the course is activated, we can know the Cid
        // Aid = Product Category, Cid = Unique Number to Put in Cart
        private void GetCid()
        {
            try
            {               
                st.Aid = "";
                st.Cid = "";

                // Start Page
                wc.GetSend(st.StartUrl);

                st.PostData = "SearchType=IVRSearch&cbarcode=" + st.Barcode + "&ajax=true";

                st.Result = wc.PostSend(st.PostData, st.SearchUrl);


                string activityCourseRow = cc.getBetween(st.Result, "id=\"activity-course-row\"", "</tbody>");
                if (activityCourseRow.Contains("headers=\"Course\""))
                {
                    st.Aid = cc.getBetween(activityCourseRow, "aid=", "&");

                    st.Cid = cc.getBetween(activityCourseRow, "cid=", "\"");
                }                            

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }
        }

        // When tabControl page is clicked
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // When selecting the Reservation Tab
            if (tabControl1.SelectedIndex == 2)
            {
                int clientIDCount = SharedClient.ClientDic.Count;                

                if (clientIDCount == 0)
                {
                    // If you don't have a ClientID (if your family doesn't have a ClientID)
                    MessageBox.Show("Please log in first!");

                    // Select Login Tab
                    tabControl1.SelectedIndex = 0;

                    return;
                }

                // Initialize combo box and add data              
                this.Invoke(new MethodInvoker(delegate ()
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("-Select a Participant-");
                }));  // invoke
               
                comboBox1.DisplayMember = "Display";  // Data visible in form, text can be changed
                comboBox1.ValueMember = "Value";  // Data to be used internally, text can be changed

                // Get ClientID
                foreach (KeyValuePair<string, string> item in SharedClient.ClientDic)
                {                   
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        st.ParticipantName = item.Key;
                        st.ParticipantID = item.Value;

                        // Add ComboBox Items
                        comboBox1.Items.Add(new { Display = st.ParticipantName, Value = st.ParticipantID });
                    }));  // invoke
                }
            }            
        }

        // CountDown
        private void timer2_Tick(object sender, EventArgs e)
        {
            // Calculate the remaining time by subtracting the selected time from the current time
            TimeSpan remainingTime = st.SelectedTime - DateTime.Now;           

            // Display the remaining time in a label
            uc.changeControlText(label13, $"Time remaining: {remainingTime.ToString(@"hh\:mm\:ss")}");

            // Check if the countdown has reached zero
            if (remainingTime <= TimeSpan.Zero)
            {
                // Stop the timer and display a message box
                timer2.Stop();                
                uc.changeControlText(label13, "Time's up!");
            }
        }


        // Successful login status
        private bool isLogin()
        {
            bool loginOK = false;

            try
            {
                st.ClientBarcode = textBox1.Text.Trim();
                st.AccountPIN = textBox2.Text.Trim();               

                // Home page
                wc.GetSend(st.StartUrl);

                // Login
                st.PostData = "Referrer=&ClientBarcode=" + st.ClientBarcode + "&AccountPIN=" + st.AccountPIN +
                                  "&FullPage=false&ajax=true&OverlayRequest=true";
                st.Result = wc.PostSend(st.PostData, st.LoginUrl);

                // Alert:Session Timed Out - the browsing session was idle for too long or was closed by the website for some other reason.
                // You will be directed to the start page in 5 seconds, or you can Click Here
                // (https://webreg.burnaby.ca/webreg/Start/start.asp)
                // https://webreg.burnaby.ca/webreg/General/BlockedSessionCookies.asp?LanguageId=1&LanguageIndex=1
                if (st.Result.Contains("BlockedSessionCookies") || st.Result.Contains("sessioncheck"))
                {
                    wc.GetSend(st.StartUrl);
                    Thread.Sleep(100);
                    st.Result = wc.PostSend(st.PostData, st.LoginUrl);
                }

                if (st.Result.Contains("Logged"))
                {
                    loginOK = true;
                    uc.updateStatusBar(toolStripStatusLabel1, "Login succeed!");
                } else
                {
                    uc.updateStatusBar(toolStripStatusLabel1, "Login failed!");
                }

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }

            return loginOK;
        }

        // Cart success or failure
        private bool isCart()
        {
            bool cartOK = false;

            try
            {
                // Put in cart

                // Put in cart 1 - You have to do the process twice
                st.GetData = "?Type=1&AddCourse=" + st.Aid + "~" + st.Cid + "~0~0~0~0~0&AjaxRequest=true&ajax=true";
                st.Result = wc.GetSend(st.MyBasketUrl + st.GetData);

                // Put in cart 2
                st.GetData = "?Type=1&AjaxRequest=true&ajax=true&ItemId=" + st.Cid;
                st.Result = wc.GetSend(st.MyBasketUrl + st.GetData);

                // Add Applicant
                st.PostData = "ClientID=" + st.ParticipantID + "&cbxRegQuantity=1&FullPage=false&update=no&Checkout=&MyBasketPage=1&TransactionStarted=0&CurrentPageUrl=&OnClickScheduleCheckboxName=&ajax=true&OverlayRequest=true";
                st.Result = wc.PostSend(st.PostData, st.MyBasketUrl + "?update=Update%20Basket&ClientID=" + st.ParticipantID);
                // End of cart loading

                if (st.Result.Contains(st.ParticipantID))
                {
                    cartOK = true;
                    uc.updateStatusBar(toolStripStatusLabel1, "Add to Cart succeed!");
                }
                else
                {
                    uc.updateStatusBar(toolStripStatusLabel1, "Add to Cart failed!");
                }

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message); 
            }

            return cartOK;
        }

        // Checkout successful
        private bool isCheckout()
        {
            bool checkoutOK = false;

            try
            {
                // Checkout
                wc.GetSend(st.MyBasketCheckoutUrl + "?ajax=true");

                // Need to do it twice
                wc.GetSend(st.MyBasketCheckoutUrl);

                st.PostData = "ReSubmitCheckout=ReSubmitted&ajax=true";
                st.Result = wc.PostSend(st.PostData, st.MyBasketCheckoutUrl + "?ApplyPayment=true");

                wc.GetSend(st.MyBasketCheckoutUrl + "?URLAddress=/webreg/MyBasket/MyBasketCheckout.asp&PayAuthorizeWait=Yes");

                wc.GetSend(st.MyBasketCheckoutUrl + "?PayAuthorizeWait=Yes");

                st.Result = wc.GetSend(st.MyBasketCheckoutUrl);

                // "id=\"receipt-body\"" character, if present, is Completed
                if (st.Result.Contains("id=\"receipt-body\""))
                {
                    checkoutOK = true;
                    uc.updateStatusBar(toolStripStatusLabel1, "Checkout succeed!");
                }
                else
                {
                    uc.updateStatusBar(toolStripStatusLabel1, "Checkout failed!");
                }

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message);
            }

            return checkoutOK;
        }

        // In order to maintain login, start page is searched every 10 minutes.
        private void timer3_Tick(object sender, EventArgs e)
        {
            // Start page
            wc.GetSend(st.StartUrl);
        }

    }  // public class
} // namespace
