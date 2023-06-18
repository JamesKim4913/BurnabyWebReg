using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BurnabyWebReg
{
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();
        }




        // Course Details


        Thread workerThread1; 

        CommonClass cc = new CommonClass(); // common class
        Strings st = new Strings(); // all string variable
        WebControl wc = new WebControl(); // webrequest class
        UIControl uc = new UIControl();  // Form UI control class



        // Method for receiving values from Form2
        public void GetDataFromForm(string barcode, string aid, string cid)
        {
            st.Barcode = barcode;
            st.Aid = aid;
            st.Cid = cid;
        }



        // Course Details
        private void DoWork_CourseDetails()
        {
            try
            {
                // Start Page
                wc.GetSend(st.StartUrl);

                st.PostData = "aid=" + st.Aid + "&cid=" + st.Cid + "&AjaxRequest=true&ajax=true";

                st.Result = wc.PostSend(st.PostData, st.CourseDetailsUrl);


                if (st.Result.Contains("title=\"Course Details\""))
                {
                    st.Course = cc.getBetween(st.Result, "CLASS=Title>", "</B>");
                    st.Course = Regex.Split(st.Course, "-")[0];

                    st.Dates = cc.getBetween(st.Result, "Meets:", "</tr>");
                    st.Dates = Regex.Split(st.Dates, "DateTime")[1];
                    st.Dates = cc.getBetween(st.Dates, ">", "<");
                    st.Dates = cc.RemoveSpace(st.Dates);

                    st.Times = cc.getBetween(st.Result, "Meets:", "</tr>");
                    st.Times = Regex.Split(st.Times, "DateTime")[2];
                    st.Times = cc.getBetween(st.Times, ">", "<");
                    st.Times = cc.RemoveSpace(st.Times);                    
                }

                // Check if "Add, Waitlist" is present
                st.AddCourse = cc.getBetween(st.Result, "AddCourse=", "a>");
                st.AddCourse = cc.getBetween(st.AddCourse, ">", "</");
                st.AddCourse = cc.remove_html_tag(st.AddCourse);

                // split only the required parts
                st.Result = Regex.Split(st.Result, "class=\"ajax-return\">")[1];

                // Add Image Tag Full Address
                st.Result = st.Result.Replace("src=\"/webreg", "src=\"https://webreg.burnaby.ca/webreg");
                // Remove Anchor tag
                string anchorTag = "<a" + cc.getBetween(st.Result, "<a", "</a>") + "</a>";
                st.Result = st.Result.Replace(anchorTag, "");

                // Displaying in a Web browser
                webBrowser1.DocumentText = st.Result;
               
                this.Invoke(new MethodInvoker(delegate ()
                {
                    if (st.AddCourse == "")
                    {
                        button1.Enabled = false;
                        button1.Text = "Not Yet";
                    }
                    else
                    {
                        button1.Enabled = true;
                        button1.Text = st.AddCourse;
                    }
                }));  // invoke  

            } // try
            catch (Exception ex)
            {
                //UpdateStatusBar(ex.Message);
            }
        }

        private void Form4_Load(object sender, EventArgs e)
        {         
            workerThread1 = new Thread(new ThreadStart(DoWork_CourseDetails));
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(DoWork_CourseDetails));
                workerThread1.Start();
                while (!workerThread1.IsAlive) ;
            }
        }

        // Add
        private void button1_Click(object sender, EventArgs e)
        {
            // Send data to form5 via constructor
            Form5 form5 = new Form5();
            form5.Show();

            // Send form5 data using methods              
            form5.GetDataFromForm(st.Course, st.Barcode, st.Times, st.Dates, st.Aid, st.Cid);
        }

    }  // class
}  // namespace
