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
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
        }

        // 바코드검색후 뜨는 폼
        // Course Details
        // Barcode Search 바코드로 검색할때는 바로 Course Details 화면이 뜬다.


        Thread workerThread1; // 스레드  
        
        CommonClass cc = new CommonClass(); // common class
        Strings st = new Strings(); // all string variable
        WebControl wc = new WebControl(); // webrequest class
        UIControl uc = new UIControl();  // Form UI control class



        // Form1에서부터 값을 받는 메소드
        public void GetDataFromForm(string barcode)
        {            
            st.Barcode = barcode;
        }


        // Course Details
        private void DoWork_CourseDetails()
        {
            try
            {
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

                    st.Aid = cc.getBetween(activityCourseRow, "aid=", "&");

                    st.Cid = cc.getBetween(activityCourseRow, "cid=", "\"");
                }

                // Add, Waitlist 있는지 확인
                st.AddCourse = cc.getBetween(st.Result, "AddCourse=", "a>");
                st.AddCourse = cc.getBetween(st.AddCourse, ">", "</");
                st.AddCourse = cc.remove_html_tag(st.AddCourse);               

                // 필요한 부분만 가져옴
                st.Result = Regex.Split(st.Result, "class=\"ajax-return\">")[1];

                // 이미지태그 전체주소추가
                st.Result = st.Result.Replace("src=\"/webreg", "src=\"https://webreg.burnaby.ca/webreg");
                // Remove Anchor tag
                string anchorTag = "<a" + cc.getBetween(st.Result, "<a", "</a>") + "</a>";
                st.Result = st.Result.Replace(anchorTag, "");                

                // 웹브라우저에 표시
                webBrowser1.DocumentText = st.Result;

                //크로스스레드방지
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
                //UpdateStatusBar(ex.Message); // 상태표시줄
            }
        }

        private void Form3_Load(object sender, EventArgs e)
        {   
            // 스레드
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
            //생성자를 통한 form5로 데이터 보내기
            Form5 form5 = new Form5();
            form5.Show();

            //메서드를 이용한 form5 데이터 전송                
            form5.GetDataFromForm(st.Course, st.Barcode, st.Times, st.Dates, st.Aid, st.Cid);

            // 현재 Form Close
            this.Close();
        }

       


    }  // class
} // namespace
