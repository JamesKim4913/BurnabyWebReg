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
    public partial class Form6 : Form
    {
        public Form6()
        {
            InitializeComponent();
        }

        // View Full Purchase History


        Thread workerThread1;

        CommonClass cc = new CommonClass(); // common class
        Strings st = new Strings(); // all string variable
        WebControl wc = new WebControl(); // webrequest class



        // Get Full Purchase History
        private void Dowork_HistoryDetails()
        {
            try
            {
                st.Result = wc.GetSend(st.MyAccountHistoryDetailsUrl + "?Clients=all");

                // View Full Purchase History
                string historySource = cc.getBetween(st.Result, "id=\"history-detail\"", "id=\"footer\"");
                string[] historyArr = Regex.Split(historySource, "</tr>");

                foreach (string history in historyArr)
                {
                    if (history.Contains("headers=\"Client\""))
                    {
                        st.ClientName = cc.getBetween(history, "headers=\"Client\"", "</td>");
                        st.ClientName = cc.getBetween(st.ClientName, "<a", "/a>");
                        st.ClientName = cc.getBetween(st.ClientName, ">", "<");
                        st.ClientName = cc.RemoveSpace(st.ClientName);

                        st.ClientCourseTitle = cc.getBetween(history, "headers=\"Client CourseTitle\"", "</td>");
                        st.ClientCourseTitle = cc.getBetween(st.ClientCourseTitle, "<a", "/a>");
                        st.ClientCourseTitle = cc.getBetween(st.ClientCourseTitle, ">", "<");
                        st.ClientCourseTitle = cc.RemoveSpace(st.ClientCourseTitle);

                        st.ClientType = cc.getBetween(history, "headers=\"Client Type\"", "/td>");
                        st.ClientType = cc.getBetween(st.ClientType, ">", "<");
                        st.ClientType = cc.RemoveSpace(st.ClientType);

                        st.ClientBarCode = cc.getBetween(history, "headers=\"Client BarCode\"", "/td>");
                        st.ClientBarCode = cc.getBetween(st.ClientBarCode, ">", "<");
                        st.ClientBarCode = cc.ReplaceSpecialString(st.ClientBarCode);
                        st.ClientBarCode = cc.RemoveSpace(st.ClientBarCode);

                        st.ClientDateSpan = cc.getBetween(history, "headers=\"Client DateSpan\"", "/td>");
                        st.ClientDateSpan = cc.getBetween(st.ClientDateSpan, ">", "<");
                        st.ClientDateSpan = cc.RemoveSpace(st.ClientDateSpan);

                        st.ClientFee = cc.getBetween(history, "headers=\"Client Fee\"", "/td>");
                        st.ClientFee = cc.getBetween(st.ClientFee, ">", "<");
                        st.ClientFee = cc.RemoveSpace(st.ClientFee);

                        if (st.ClientName != "")
                        {                          
                            this.Invoke(new MethodInvoker(delegate ()
                            {                              
                                dataGridView1.Rows.Add(st.ClientName, st.ClientCourseTitle, st.ClientType,
                                    st.ClientBarCode, st.ClientDateSpan, st.ClientFee);
                            }));  // invoke
                        }

                    } // if                  
                } // foreach  

            } // try
            catch (Exception ex)
            {
                
            }
        }


        private void Form6_Load(object sender, EventArgs e)
        {          
            workerThread1 = new Thread(new ThreadStart(Dowork_HistoryDetails));
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(Dowork_HistoryDetails));
                workerThread1.Start();
                while (!workerThread1.IsAlive) ;
            }
        }

        



    } // class
}  // namespace
