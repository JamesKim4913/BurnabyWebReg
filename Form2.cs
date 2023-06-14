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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }


        // Show Courses


        Thread workerThread1; // 스레드  

        CommonClass cc = new CommonClass(); // common class
        Strings st = new Strings(); // all string variable
        WebControl wc = new WebControl(); // webrequest class




        // Form1에서부터 값을 받는 메소드
        public void GetDataFromForm(string data)
        {
            st.Aid = data;            
        }





        // Show Courses
        private void DoWork_ShowCourses()
        {
            try
            {
                // Start Page
                wc.GetSend(st.StartUrl);


                string url = "https://webreg.burnaby.ca/webreg/Activities/ActivitiesDetails.asp?AdvPage=true&ProcessWait=N&aid=" + st.Aid + "&ComplexId=0&sEcho=1&iColumns=9&sColumns=&iDisplayStart=0&iDisplayLength=10&ajax=true";                

                st.Result = wc.GetSend(url); 

                // Result               
                string[] coursesArr = Regex.Split(st.Result, "</tr>"); // 배열로분리 

                foreach (string course in coursesArr)
                {
                    if (course.Contains("headers=\"Course\""))
                    {
                        st.Course = cc.getBetween(course, "headers=\"Course\"", "</td>");
                        st.Course = cc.getBetween(st.Course, "<div", "/div>");
                        st.Course = cc.getBetween(st.Course, ">", "<");
                        st.Course = cc.RemoveSpace(st.Course);

                        st.Barcode = cc.getBetween(course, "headers=\"Barcode\"", "/td>");
                        st.Barcode = cc.getBetween(st.Barcode, ">", "<");
                        st.Barcode = cc.RemoveSpace(st.Barcode);

                        st.Days = cc.getBetween(course, "headers=\"Day\"", "/td>");
                        st.Days = cc.getBetween(st.Days, ">", "<");
                        st.Days = cc.RemoveSpace(st.Days);

                        st.Times = cc.getBetween(course, "headers=\"Times\"", "td>");
                        st.Times = cc.getBetween(st.Times, ">", "</");
                        st.Times = cc.remove_html_tag(st.Times);
                        st.Times = cc.RemoveSpace(st.Times);                        

                        st.Dates = cc.getBetween(course, "headers=\"Dates\"", "td>");
                        st.Dates = cc.getBetween(st.Dates, ">", "</");
                        st.Dates = cc.remove_html_tag(st.Dates);
                        st.Dates = cc.RemoveSpace(st.Dates);                        

                        st.Complex = cc.getBetween(course, "headers=\"Complex\"", "td>");
                        st.Complex = cc.getBetween(st.Complex, ">", "</");
                        st.Complex = cc.remove_html_tag2(st.Complex);
                        st.Complex = cc.RemoveSpace(st.Complex);                        

                        st.Sessions = cc.getBetween(course, "headers=\"Classes\"", "/td>");
                        st.Sessions = cc.getBetween(st.Sessions, ">", "<");
                        st.Sessions = cc.RemoveSpace(st.Sessions);

                        st.SpacesAvail = cc.getBetween(course, "headers=\"Available\"", "/td>");
                        st.SpacesAvail = cc.getBetween(st.SpacesAvail, ">", "<");
                        st.SpacesAvail = cc.RemoveSpace(st.SpacesAvail);

                        st.ButtonName = cc.getBetween(course, "headers=\"BasketLink\"", "</td>");
                        if(st.ButtonName.Contains("AddCourse="))
                        {
                            st.ButtonName = cc.getBetween(st.ButtonName, "AddCourse=", "/a>");
                            st.ButtonName = cc.getBetween(st.ButtonName, ">", "<");
                        } else
                        {
                            st.ButtonName = "Not Yet";
                        }                       

                        st.Cid = cc.getBetween(course, "cid=", "\"");                        

                        //크로스스레드방지
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            // DataGridView에 추가
                            dataGridView1.Rows.Add(st.Course, st.Barcode, st.Days, st.Times,
                                st.Dates, st.Complex, st.Sessions, st.SpacesAvail, "View Details", st.ButtonName, st.Cid);
                        }));  // invoke
                    } // if
                } // foreach


                // 전체 레코드가 10 이상이면 더 작업함
                if (st.Result.Contains("<records-total>"))
                {
                    st.RecordsTotal = cc.getBetween(st.Result, "<records-total>", "</records-total>");
                    int recordsTotal = Int32.Parse(st.RecordsTotal);
                    int iDivide = recordsTotal / 10;
                    for (int i = 1; i <= iDivide; i++)
                    {
                        DoWork_ShowCoursesNext(i * 10, i + 1);
                    }                    
                }

            } // try
            catch (Exception ex)
            {
                //UpdateStatusBar(ex.Message); // 상태표시줄
            }
        }


        // Show Courses next page
        private void DoWork_ShowCoursesNext(int iDisplayStart, int sEcho)
        {
            try
            {
                string url = "https://webreg.burnaby.ca/webreg/Activities/ActivitiesDetails.asp?AdvPage=true&ProcessWait=N&aid=" + st.Aid + "&ComplexId=0&sEcho=" + sEcho + "&iColumns=9&sColumns=&iDisplayStart=" + iDisplayStart + "&iDisplayLength=10&ajax=true";                

                st.Result = wc.GetSend(url);

                // Result               
                string[] coursesArr = Regex.Split(st.Result, "</tr>"); // 배열로분리 

                foreach (string course in coursesArr)
                {
                    if (course.Contains("headers=\"Course\""))
                    {
                        st.Course = cc.getBetween(course, "headers=\"Course\"", "</td>");
                        st.Course = cc.getBetween(st.Course, "<div", "/div>");
                        st.Course = cc.getBetween(st.Course, ">", "<");
                        st.Course = cc.RemoveSpace(st.Course);

                        st.Barcode = cc.getBetween(course, "headers=\"Barcode\"", "/td>");
                        st.Barcode = cc.getBetween(st.Barcode, ">", "<");
                        st.Barcode = cc.RemoveSpace(st.Barcode);

                        st.Days = cc.getBetween(course, "headers=\"Day\"", "/td>");
                        st.Days = cc.getBetween(st.Days, ">", "<");
                        st.Days = cc.RemoveSpace(st.Days);

                        st.Times = cc.getBetween(course, "headers=\"Times\"", "td>");
                        st.Times = cc.getBetween(st.Times, ">", "</");
                        st.Times = cc.remove_html_tag(st.Times);
                        st.Times = cc.RemoveSpace(st.Times);

                        st.Dates = cc.getBetween(course, "headers=\"Dates\"", "td>");
                        st.Dates = cc.getBetween(st.Dates, ">", "</");
                        st.Dates = cc.remove_html_tag(st.Dates);
                        st.Dates = cc.RemoveSpace(st.Dates);

                        st.Complex = cc.getBetween(course, "headers=\"Complex\"", "td>");
                        st.Complex = cc.getBetween(st.Complex, ">", "</");
                        st.Complex = cc.remove_html_tag2(st.Complex);
                        st.Complex = cc.RemoveSpace(st.Complex);

                        st.Sessions = cc.getBetween(course, "headers=\"Classes\"", "/td>");
                        st.Sessions = cc.getBetween(st.Sessions, ">", "<");
                        st.Sessions = cc.RemoveSpace(st.Sessions);

                        st.SpacesAvail = cc.getBetween(course, "headers=\"Available\"", "/td>");
                        st.SpacesAvail = cc.getBetween(st.SpacesAvail, ">", "<");
                        st.SpacesAvail = cc.RemoveSpace(st.SpacesAvail);

                        st.ButtonName = cc.getBetween(course, "headers=\"BasketLink\"", "</td>");
                        if (st.ButtonName.Contains("AddCourse="))
                        {
                            st.ButtonName = cc.getBetween(st.ButtonName, "AddCourse=", "/a>");
                            st.ButtonName = cc.getBetween(st.ButtonName, ">", "<");
                        }
                        else
                        {
                            st.ButtonName = "Not Yet";
                        }

                        st.Cid = cc.getBetween(course, "cid=", "\"");

                        //크로스스레드방지
                        this.Invoke(new MethodInvoker(delegate ()
                        {
                            // DataGridView에 추가
                            dataGridView1.Rows.Add(st.Course, st.Barcode, st.Days, st.Times,
                                st.Dates, st.Complex, st.Sessions, st.SpacesAvail, "View Details", st.ButtonName, st.Cid);
                        }));  // invoke
                    } // if
                } // foreach

            } // try
            catch (Exception ex)
            {
                //UpdateStatusBar(ex.Message); // 상태표시줄
            }
        }


        private void Form2_Load(object sender, EventArgs e)
        {
            // 스레드
            workerThread1 = new Thread(new ThreadStart(DoWork_ShowCourses));           
            if ((workerThread1.ThreadState & ThreadState.Unstarted) == ThreadState.Unstarted)
            {
                workerThread1 = new Thread(new ThreadStart(DoWork_ShowCourses));
                workerThread1.Start();                                 
                while (!workerThread1.IsAlive);
            }          
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // View Details 버튼 클릭시
            if (e.ColumnIndex == dataGridView1.Columns[8].Index)
            {
                st.Cid = dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString();               
                st.Barcode = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();

                //생성자를 통한 form4로 데이터 보내기
                Form4 form4 = new Form4();
                form4.Show();

                //메서드를 이용한 form4로 데이터 전송                
                form4.GetDataFromForm(st.Barcode, st.Aid, st.Cid);
            }

            // Add 버튼 클릭시
            if (e.ColumnIndex == dataGridView1.Columns[9].Index)
            {
                st.Cid = dataGridView1.Rows[e.RowIndex].Cells[10].Value.ToString();
                st.Course = dataGridView1.Rows[e.RowIndex].Cells[0].Value.ToString();
                st.Barcode = dataGridView1.Rows[e.RowIndex].Cells[1].Value.ToString();
                st.Times = dataGridView1.Rows[e.RowIndex].Cells[3].Value.ToString();
                st.Dates = dataGridView1.Rows[e.RowIndex].Cells[4].Value.ToString();                 
                st.ButtonName = dataGridView1.Rows[e.RowIndex].Cells[9].Value.ToString();

                if(st.ButtonName == "Not Yet")
                {
                    MessageBox.Show("This course is not presently available for Internet Registration");
                    return;
                }

                //생성자를 통한 form5로 데이터 보내기
                Form5 form5 = new Form5();
                form5.Show();

                //메서드를 이용한 form5 데이터 전송                
                form5.GetDataFromForm(st.Course, st.Barcode, st.Times, st.Dates, st.Aid, st.Cid);                
            } 
        }


    } // class
}  // namespace
