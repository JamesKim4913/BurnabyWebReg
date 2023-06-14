using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BurnabyWebReg
{
    public partial class Form5 : Form
    {
        public Form5()
        {
            InitializeComponent();
        }





        // Cart


        Thread workerThread1; // 스레드  

        CommonClass cc = new CommonClass(); // common class
        Strings st = new Strings(); // all string variable
        WebControl wc = new WebControl(); // webrequest class
        UIControl uc = new UIControl();  // Form UI control class



        // Form2에서부터 값을 받는 메소드
        public void GetDataFromForm(string course, string barcode, string times, string dates, string aid, string cid)
        {
            st.Course = course;
            st.Barcode = barcode;
            st.Times = times;
            st.Dates = dates;
            st.Aid = aid;
            st.Cid = cid;

            // 화면에 표시
            DisplayControl();
        }



        // 화면에 표시
        private void DisplayControl()
        {  
            uc.changeControlText(label2, st.Barcode + " - " + st.Course);
            uc.changeControlText(label6, st.Dates + " " + st.Times);            
        }


        // Add ComboBox 가족들 세팅 
        private void AddParticipant()
        {
            try
            {
                // 콤보박스 초기화 및 데이터추가
                //크로스스레드방지
                this.Invoke(new MethodInvoker(delegate ()
                {
                    comboBox1.Items.Clear();
                    comboBox1.Items.Add("-Select a Participant-");
                }));  // invoke

                comboBox1.DisplayMember = "Display";  // 폼에서 보이는 데이터, 텍스트변경가능
                comboBox1.ValueMember = "Value";  // 내부적으로 쓰일 데이터, 텍스트변경가능

                // Get ClientID
                foreach (KeyValuePair<string, string> item in SharedClient.ClientDic)
                {
                    //크로스스레드방지
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        st.ParticipantName = item.Key;
                        st.ParticipantID = item.Value;

                        // Add ComboBox Items
                        comboBox1.Items.Add(new { Display = st.ParticipantName, Value = st.ParticipantID });
                    }));  // invoke
                }

                // ClientID 가 없다면
                int clientIDCount = SharedClient.ClientDic.Count;

                if (clientIDCount == 0)
                {
                    MessageBox.Show("Please log in first!");

                    // Form1 폼으로 활성화 이동
                    foreach (Form openForm in Application.OpenForms)
                    {
                        if (openForm.Name == "Form1")
                        {
                            openForm.Activate();
                            break;
                        }
                    }

                    // 현재 Form Close
                    this.Close();
                }
                
            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message); // 상태표시줄
            }
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            AddParticipant();            

            comboBox1.SelectedIndex = 0;
        }



        // Add Cart
        private void DoWork_AddCart()
        {
            try
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    // 콤보박스 선택한 것의 Value 값
                    st.ParticipantName = (comboBox1.SelectedItem as dynamic).Display;

                    // 콤보박스 선택한 것의 Value 값
                    st.ParticipantID = (comboBox1.SelectedItem as dynamic).Value;
                }));  // invoke 

                // 장바구니에 담기 1 - 두번과정을 해야함
                st.GetData = "?Type=1&AddCourse=" + st.Aid + "~" + st.Cid + "~0~0~0~0~0&AjaxRequest=true&ajax=true";
                st.Result = wc.GetSend(st.MyBasketUrl + st.GetData);

                // 장바구니에 담기 2
                st.GetData = "?Type=1&AjaxRequest=true&ajax=true&ItemId=" + st.Cid;
                st.Result = wc.GetSend(st.MyBasketUrl + st.GetData);                
                
                // 신청자 추가
                st.PostData = "ClientID=" + st.ParticipantID + "&cbxRegQuantity=1&FullPage=false&update=no&Checkout=&MyBasketPage=1&TransactionStarted=0&CurrentPageUrl=&OnClickScheduleCheckboxName=&ajax=true&OverlayRequest=true";
                st.Result = wc.PostSend(st.PostData, st.MyBasketUrl + "?update=Update%20Basket&ClientID=" + st.ParticipantID);               

                if (st.Result.Contains("id=\"cart-subtotal\""))
                {
                    st.CartSubTotal = cc.getBetween(st.Result, "id=\"cart-subtotal\"", "/span>");
                    st.CartSubTotal = cc.getBetween(st.CartSubTotal, ">", "<");
                    st.CartSubTotal = cc.ReplaceSpecialString(st.CartSubTotal);                   
                }

                if (st.Result.Contains("id=\"grandtotal\""))
                {
                    st.CartGrandTotal = cc.getBetween(st.Result, "id=\"grandtotal\"", "/span>");
                    st.CartGrandTotal = cc.getBetween(st.CartGrandTotal, ">", "<");
                    st.CartGrandTotal = cc.ReplaceSpecialString(st.CartGrandTotal);

                    uc.changeControlText(label5, st.CartGrandTotal);
                }

                //크로스스레드방지
                this.Invoke(new MethodInvoker(delegate ()
                {
                    // DataGridView에 추가
                    dataGridView1.Rows.Add(st.ParticipantName, st.Course, st.Dates, st.Times, st.CartSubTotal, "Remove", st.ParticipantID);
                }));  // invoke                         

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message); // 상태표시줄
            }
        }



        // MyBasket Checkout
        private void DoWork_Checkout()
        {
            try
            {
                wc.GetSend(st.MyBasketCheckoutUrl + "?ajax=true");

                wc.GetSend(st.MyBasketCheckoutUrl);

                st.PostData = "ReSubmitCheckout=ReSubmitted&ajax=true";
                st.Result = wc.PostSend(st.PostData, st.MyBasketCheckoutUrl + "?ApplyPayment=true");

                wc.GetSend(st.MyBasketCheckoutUrl + "?URLAddress=/webreg/MyBasket/MyBasketCheckout.asp&PayAuthorizeWait=Yes");

                wc.GetSend(st.MyBasketCheckoutUrl + "?PayAuthorizeWait=Yes");

                st.Result = wc.GetSend(st.MyBasketCheckoutUrl);

                // "id=\"receipt-body\"" 문자가 없다면 Return
                if (!st.Result.Contains("id=\"receipt-body\""))
                {
                    return;
                }


                // Display Receipt
                string receiptSource = cc.getBetween(st.Result, "id=\"receipt-body\"", "id=\"course-league-item-info\"");
                string[] receiptArr = Regex.Split(receiptSource, "</tr>");

                foreach (string receipt in receiptArr)
                {
                    if (receipt.Contains("Receipt #:"))
                    {
                        st.Receipt = Regex.Split(receipt, "Receipt #:")[1];
                        st.Receipt = cc.remove_html_tag(st.Receipt);
                        st.Receipt = cc.RemoveSpace(st.Receipt);
                    }

                    if (receipt.Contains("Issued:"))
                    {
                        st.ReceiptIssued = Regex.Split(receipt, "Issued:")[1];
                        st.ReceiptIssued = cc.remove_html_tag(st.ReceiptIssued);
                        st.ReceiptIssued = cc.RemoveSpace(st.ReceiptIssued);
                    }

                    if (receipt.Contains("User:"))
                    {
                        st.ReceiptUser = Regex.Split(receipt, "User:")[1];
                        st.ReceiptUser = cc.remove_html_tag(st.ReceiptUser);
                        st.ReceiptUser = cc.RemoveSpace(st.ReceiptUser);
                    }

                    if (receipt.Contains("Previous Account Balance:"))
                    {
                        st.ReceiptPreviousAccountBalance = Regex.Split(receipt, "Previous Account Balance:")[1];
                        st.ReceiptPreviousAccountBalance = cc.remove_html_tag(st.ReceiptPreviousAccountBalance);
                        st.ReceiptPreviousAccountBalance = cc.RemoveSpace(st.ReceiptPreviousAccountBalance);
                    }
                } // foreach

                string CourseLeagueItemSource = cc.getBetween(st.Result, "id=\"course-league-item-info\"", "id=\"receipt-buttons\"");
                string[] CourseLeagueItemArr = Regex.Split(CourseLeagueItemSource, "</tr>");

                for (int i = 0; i < CourseLeagueItemArr.Length; i++)
                {
                    if (i == 0) // Registration:
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptRegistration = tdArr[1];
                        st.ReceiptRegistration = cc.remove_html_tag(st.ReceiptRegistration);
                        st.ReceiptRegistration = cc.RemoveSpace(st.ReceiptRegistration);
                    }
                    else if (i == 1) // CourseTitle
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptCourseTitle = tdArr[1];
                        st.ReceiptCourseTitle = cc.remove_html_tag(st.ReceiptCourseTitle);
                        st.ReceiptCourseTitle = cc.RemoveSpace(st.ReceiptCourseTitle);
                    }
                    else if (i == 2) // Session
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptSession = tdArr[1];
                        st.ReceiptSession = cc.remove_html_tag(st.ReceiptSession);
                        st.ReceiptSession = cc.RemoveSpace(st.ReceiptSession);
                    }
                    else if (i == 3) // Classes, Hours
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptClasses = tdArr[2];
                        st.ReceiptClasses = cc.remove_html_tag(st.ReceiptClasses);
                        st.ReceiptClasses = cc.RemoveSpace(st.ReceiptClasses);

                        st.ReceiptHours = tdArr[4];
                        st.ReceiptHours = cc.remove_html_tag(st.ReceiptHours);
                        st.ReceiptHours = cc.RemoveSpace(st.ReceiptHours);
                    }
                    else if (i == 4) // Days
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptDays = tdArr[2];
                        st.ReceiptDays = cc.remove_html_tag(st.ReceiptDays);
                        st.ReceiptDays = cc.RemoveSpace(st.ReceiptDays);
                    }
                    else if (i == 5) // Starts
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptStarts = tdArr[2];
                        st.ReceiptStarts = cc.remove_html_tag(st.ReceiptStarts);
                        st.ReceiptStarts = cc.RemoveSpace(st.ReceiptStarts);
                    }
                    else if (i == 6) // Ends
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptEnds = tdArr[2];
                        st.ReceiptEnds = cc.remove_html_tag(st.ReceiptEnds);
                        st.ReceiptEnds = cc.RemoveSpace(st.ReceiptEnds);
                    }
                    else if (i == 7) // Location
                    {
                        string[] tdArr = Regex.Split(CourseLeagueItemArr[i], "</td>");
                        st.ReceiptLocation = tdArr[2];
                        st.ReceiptLocation = cc.remove_html_tag(st.ReceiptLocation);
                        st.ReceiptLocation = cc.RemoveSpace(st.ReceiptLocation);
                    }
                    else if (i == 9) // Cost
                    {
                        st.ReceiptCost = cc.getBetween(CourseLeagueItemArr[i], "<td", "/td>");
                        st.ReceiptCost = cc.getBetween(st.ReceiptCost, ">", "<");
                        st.ReceiptCost = cc.RemoveSpace(st.ReceiptCost);
                    }

                } // for


                if (CourseLeagueItemSource.Contains("GST:"))
                {
                    st.ReceiptGST = cc.getBetween(CourseLeagueItemSource, "GST:", "<tr");
                    st.ReceiptGST = cc.remove_html_tag(st.ReceiptGST);
                    st.ReceiptGST = cc.ReplaceSpecialString(st.ReceiptGST);
                    st.ReceiptGST = cc.RemoveSpace(st.ReceiptGST);
                }

                if (CourseLeagueItemSource.Contains("Payment from account:"))
                {
                    st.ReceiptPaymentFromAccount = cc.getBetween(CourseLeagueItemSource, "Payment from account:", "</tr>");
                    st.ReceiptPaymentFromAccount = cc.remove_html_tag(st.ReceiptPaymentFromAccount);
                    st.ReceiptPaymentFromAccount = cc.ReplaceSpecialString(st.ReceiptPaymentFromAccount);
                    st.ReceiptPaymentFromAccount = cc.RemoveSpace(st.ReceiptPaymentFromAccount);
                }

                if (CourseLeagueItemSource.Contains("New Account Balance:"))
                {
                    st.ReceiptNewAccountBalance = cc.getBetween(CourseLeagueItemSource, "New Account Balance:", "</tr>");
                    st.ReceiptNewAccountBalance = cc.remove_html_tag(st.ReceiptNewAccountBalance);
                    st.ReceiptNewAccountBalance = cc.ReplaceSpecialString(st.ReceiptNewAccountBalance);
                    st.ReceiptNewAccountBalance = cc.RemoveSpace(st.ReceiptNewAccountBalance);
                }

                // 화면에 표시
                uc.changeControlText(label23, st.Receipt);
                uc.changeControlText(label24, st.ReceiptIssued);
                uc.changeControlText(label25, st.ReceiptUser);
                uc.changeControlText(label26, st.ReceiptPreviousAccountBalance);
                uc.changeControlText(label27, st.ReceiptRegistration);
                uc.changeControlText(label28, st.ReceiptCourseTitle);
                uc.changeControlText(label29, st.ReceiptSession);
                uc.changeControlText(label30, st.ReceiptClasses);
                uc.changeControlText(label31, st.ReceiptHours);
                uc.changeControlText(label32, st.ReceiptDays);
                uc.changeControlText(label33, st.ReceiptStarts);
                uc.changeControlText(label34, st.ReceiptEnds);
                uc.changeControlText(label35, st.ReceiptLocation);
                uc.changeControlText(label40, st.ReceiptCost);
                uc.changeControlText(label36, st.ReceiptGST);
                uc.changeControlText(label37, st.ReceiptPaymentFromAccount);
                uc.changeControlText(label38, st.ReceiptNewAccountBalance);

            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message); // 상태표시줄
            }
        }



        // Add Cart
        private void button2_Click(object sender, EventArgs e)
        {            
            int index = comboBox1.SelectedIndex;
            if (index <= 0)
            {
                // Select a Participant
                MessageBox.Show("Select a Participant!");
                return;
            }            

            // 스레드
            workerThread1 = new Thread(new ThreadStart(DoWork_AddCart));
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(DoWork_AddCart));
                workerThread1.Start();
                while (!workerThread1.IsAlive);
            }
        }

        // Clear Cart
        private void button1_Click(object sender, EventArgs e)
        {
            // 스레드
            workerThread1 = new Thread(new ThreadStart(DoWork_ClearCart));
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(DoWork_ClearCart));
                workerThread1.Start();
                while (!workerThread1.IsAlive) ;
            }
        }

        // Clear Cart
        private void DoWork_ClearCart()
        {
            try
            {
                st.GetData = "?ClearBasket=True&PrintWidth=640&PrintHeight=490&CheckoutPage=0";
                st.PostData = "cbxRegQuantity=1&FullPage=false&update=no&Checkout=&MyBasketPage=1&TransactionStarted=0&CurrentPageUrl=&OnClickScheduleCheckboxName=&ajax=true&OverlayRequest=true";
                st.Result = wc.PostSend(st.PostData, st.MyBasketUrl + st.GetData);

                // Cart Grand Total
                uc.changeControlText(label5, "$0.00");

                // Clear dataGridView               
                this.Invoke(new MethodInvoker(delegate ()
                {
                    dataGridView1.Rows.Clear();
                }));  // invoke 
            } // try
            catch (Exception ex)
            {
                uc.updateStatusBar(toolStripStatusLabel1, ex.Message); // 상태표시줄
            }
        }

        // Checkout
        private void button3_Click(object sender, EventArgs e)
        {
            // 스레드
            workerThread1 = new Thread(new ThreadStart(DoWork_Checkout));
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(DoWork_Checkout));
                workerThread1.Start();
                while (!workerThread1.IsAlive);
            }

            groupBox1.Visible = true;
        }

        // DataGridView Click
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Remove 버튼 클릭시
            // check if the clicked cell is the DeleteButton and a valid row
            if (e.ColumnIndex == dataGridView1.Columns[5].Index && e.RowIndex >= 0)
            {
                // Remove Cart current Registrant 
                st.GetData = "?Type=1&ClearCourse=" + st.Aid + "~" + st.Cid + "~" + st.ParticipantID + "~1368864";
                st.PostData = "cbxRegQuantity=1&FullPage=false&update=no&Checkout=&MyBasketPage=1&TransactionStarted=0&CurrentPageUrl=&OnClickScheduleCheckboxName=&ajax=true&OverlayRequest=true";
                st.Result = wc.PostSend(st.PostData, st.MyBasketUrl + st.GetData);

                if (st.Result.Contains("id=\"grandtotal\""))
                {
                    st.CartGrandTotal = cc.getBetween(st.Result, "id=\"grandtotal\"", "/span>");
                    st.CartGrandTotal = cc.getBetween(st.CartGrandTotal, ">", "<");
                    st.CartGrandTotal = cc.ReplaceSpecialString(st.CartGrandTotal);
                    // Cart Grand Total
                    uc.changeControlText(label5, st.CartGrandTotal);
                }                

                // Remove DataGridView Row
                this.Invoke(new MethodInvoker(delegate ()
                {
                    // remove the selected row
                    dataGridView1.Rows.RemoveAt(e.RowIndex);
                }));  // invoke
            }
        }


    }  // class
}  // namespace
