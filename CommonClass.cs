using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;  // 파일관련
using System.Net;
using System.Web;    // add reference  System.Web
using System.Text.RegularExpressions;  // 정규표현식
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Net.Http;


class CommonClass
{
    // 인터넷띄우기
    public void OpenBrowser(string openurl, int openwidth, int openheight)
    {
        StringBuilder query = new StringBuilder(openurl);

        string u = query.ToString();
        object urls = u;
        System.Type oType = System.Type.GetTypeFromProgID("InternetExplorer.Application");
        object o = System.Activator.CreateInstance(oType);
        //o.GetType().InvokeMember("menubar", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { 0 });
        //o.GetType().InvokeMember("toolbar", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { 0 });
        //o.GetType().InvokeMember("statusBar", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { 0 });
        //o.GetType().InvokeMember("addressbar", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { 0 });
        //o.GetType().InvokeMember("width", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { openwidth });
        //o.GetType().InvokeMember("height", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { openheight });
        //o.GetType().InvokeMember("Resizable", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { true });
        o.GetType().InvokeMember("Visible", System.Reflection.BindingFlags.SetProperty, null, o, new object[] { true });
        o.GetType().InvokeMember("Navigate", System.Reflection.BindingFlags.InvokeMethod, null, o, new object[] { urls });
    }


    // 소스전체에서 일부 추출
    public string getBetween(string strSource, string strStart, string strEnd)
    {
        int Start, End;
        if (strSource.Contains(strStart) && strSource.Contains(strEnd))
        {
            Start = strSource.IndexOf(strStart, 0) + strStart.Length;
            End = strSource.IndexOf(strEnd, Start);
            return strSource.Substring(Start, End - Start);
        }
        else
        {
            return "";
        }
    }


      

    // 특수문자 변경
    public string ReplaceSpecialString(string str)
    {
        Dictionary<string, string> dicSpStr = new Dictionary<string, string>
            {
                {"&nbsp;", " "}
                ,{"&lt;", "<"}
                ,{"&gt;", ">"}
                ,{"&amp;", "&"}
                ,{"&quot;", "\""}
                ,{"&lsquo;", "'"}
                ,{"&rsquo;", "'"}
                ,{"&middot;", "·"}
                ,{"&#8228;", "·"}
                ,{"\"", ""}
                ,{"'", ""}
            };

        foreach (KeyValuePair<string, string> spStr in dicSpStr)
        {
            str = str.Replace(spStr.Key, spStr.Value);
        }

        return str;
    }


    public string RemoveWhitespace(string str)
    {
        return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
    }


    // Remove Extra Whitespace
    public string RemoveSpace(string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        str = str.Trim().Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
        str = (new System.Text.RegularExpressions.Regex(" +")).Replace(str, " ");

        return str;
    }



    public string RemoveSpecial(string content)
    {
        content = Regex.Replace(content, @"[^\p{L}\p{M}p{N}]+", " "); // 유니코드 이모티콘 제거       
        content = remove_html_tag(content);  // 태그제거
        content = RemoveSpace(content);  // 화이트 스페이스 제거   
        content = Regex.Replace(content, @"[^0-9a-zA-Zㄱ-힗]+", " ", RegexOptions.Singleline);  // 특수문자 제거 
        content = Regex.Replace(content, @"[^a-zA-Z0-9가-힣]", " ", RegexOptions.Singleline);  // 특수문자 제거 
        content = Regex.Replace(content, "\n", " ", RegexOptions.IgnoreCase);
        content = content.Replace("n", " ");
        content = content.Trim();
        return content;
    }


    // 태그제거
    public string remove_html_tag(string html_str)
    {
        // 정규표현을 이용한 HTML태그 삭제
        return Regex.Replace(html_str, @"[<][a-z|A-Z|/](.|\n)*?[>]", "");
    }

    // 태그제거후 스페이스로 한칸 띄워줌
    public string remove_html_tag2(string html_str)
    {
        // 정규표현을 이용한 HTML태그 삭제
        return Regex.Replace(html_str, @"[<][a-z|A-Z|/](.|\n)*?[>]", " ");
    }



    // 랜덤숫자 구하기
    public int Get_Randomnum(Random random, int minnum, int maxnum)
    {
        int RandomNumber = 0;
        //  Random random = new Random();
        RandomNumber = random.Next(minnum, maxnum);
        return RandomNumber;
    }





    // txt 파일 저장
    public void Save_Txt(string filename, string content)
    {
        try
        {
            string myexePath = Application.StartupPath;  // 실행파일이 있는 폴더
            string pathfilename = myexePath + @"\" + filename + @".txt";  // 파일명폴더경로 

            //Pass the filepath and filename to the StreamWriter Constructor
            // StreamWriter sw = new StreamWriter(pathfilename, false, Encoding.GetEncoding("EUC-KR"));
            StreamWriter sw = new StreamWriter(pathfilename, false, Encoding.Default);

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message); // 상태표시줄
        }
    }

    // txt 파일 읽기
    public string Read_Txt(string filename)
    {
        string content = string.Empty;
        string strFileLine = string.Empty;
        try
        {
            string myexePath = Application.StartupPath;  // 실행파일이 있는 폴더
            string pathfilename = myexePath + @"\" + filename + @".txt";  // 파일명폴더경로

            if (File.Exists(pathfilename)) // 파일이 존재하면
            {
                // 텍스트 파일 읽어와서  
                // StreamReader SRead = new StreamReader(pathfilename, System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
                StreamReader SRead = new StreamReader(pathfilename, Encoding.Default);

                while ((!SRead.EndOfStream))  // (strFileLine = SRead.ReadLine() != string.Empty))
                {
                    strFileLine = SRead.ReadLine();  // 한줄씩 읽음
                    if ((strFileLine != string.Empty))  // 데이터가 있으면
                    {
                        content = content + strFileLine + Environment.NewLine;
                    }  // if strFileLine
                } // while

                SRead.Close();
            }
        } // try
        catch (Exception ex)
        {
            // UpdateStatusBar(ex.Message); // 상태표시줄
        }
        return content;
    }


    // txt 파일 저장 기존파일에 이어서
    public void Save_Txt_Conti(string filename, string content)
    {
        try
        {
            string myexePath = Application.StartupPath;  // 실행파일이 있는 폴더
            string pathfilename = myexePath + @"\" + filename + @".txt";  // 파일명폴더경로 

            //Pass the filepath and filename to the StreamWriter Constructor
            // StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.GetEncoding("EUC-KR"));
            StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.Default);  // 중간에 옵션을 true 해주면 기존파일에 이어서 추가됨

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            // UpdateStatusBar(ex.Message); // 상태표시줄
        }
    }



    // txt 파일 읽기
    public string ReadAll_Txt(string filename)
    {
        string content = string.Empty;
        string strFileLine = string.Empty;
        try
        {
            string myexePath = Application.StartupPath;  // 실행파일이 있는 폴더
            string pathfilename = myexePath + @"\" + filename + @".txt";  // 파일명폴더경로

            if (File.Exists(pathfilename)) // 파일이 존재하면
            {
                // 텍스트 파일 읽어와서  
                // StreamReader SRead = new StreamReader(pathfilename, System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
                StreamReader SRead = new StreamReader(pathfilename, Encoding.Default);

                content = SRead.ReadToEnd();

                // 한줄씩 읽기
                /*
                while ((!SRead.EndOfStream))  // (strFileLine = SRead.ReadLine() != string.Empty))
                {
                    strFileLine = SRead.ReadLine();  // 한줄씩 읽음
                    if ((strFileLine != string.Empty))  // 데이터가 있으면
                    {
                        content = content + strFileLine + Environment.NewLine;
                    }  // if strFileLine
                } // while
                */
                // 한줄씩읽기 끝

                SRead.Close();
            }
        } // try
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message); // 상태표시줄
        }
        return content;
    }


    // txt 파일 저장 풀경로로 기존파일에 이어서
    public void Save_fullpathTxt_Conti(string pathfilename, string content)
    {
        try
        {
            // string myexePath = Application.StartupPath;  // 실행파일이 있는 폴더
            // string pathfilename = myexePath + @"\" + filename + @".txt";  // 파일명폴더경로 

            //Pass the filepath and filename to the StreamWriter Constructor
            // StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.GetEncoding("EUC-KR"));
            StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.Default);

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message); // 상태표시줄
        }
    }


    // txt 파일 저장 풀경로로
    public void Save_fullpathTxt(string pathfilename, string content)
    {
        try
        {
            // string myexePath = Application.StartupPath;  // 실행파일이 있는 폴더
            // string pathfilename = myexePath + @"\" + filename + @".txt";  // 파일명폴더경로 

            //Pass the filepath and filename to the StreamWriter Constructor
            // StreamWriter sw = new StreamWriter(pathfilename, false, Encoding.GetEncoding("EUC-KR"));
            StreamWriter sw = new StreamWriter(pathfilename, false, Encoding.Default);

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message); // 상태표시줄
        }
    }


    // txt 파일 읽기
    public string Read_fullpathTxt(string pathfilename)
    {
        string content = string.Empty;
        string strFileLine = string.Empty;
        try
        {
            if (File.Exists(pathfilename)) // 파일이 존재하면
            {
                // 텍스트 파일 읽어와서  
                // StreamReader SRead = new StreamReader(pathfilename, System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
                StreamReader SRead = new StreamReader(pathfilename, Encoding.Default);

                content = SRead.ReadToEnd();

                // 한줄씩 읽기
                /*
                while ((!SRead.EndOfStream))  // (strFileLine = SRead.ReadLine() != string.Empty))
                {
                    strFileLine = SRead.ReadLine();  // 한줄씩 읽음
                    if ((strFileLine != string.Empty))  // 데이터가 있으면
                    {
                        content = content + strFileLine + Environment.NewLine;
                    }  // if strFileLine
                } // while
                */
                // 한줄씩읽기 끝

                SRead.Close();
            }
        } // try
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message); // 상태표시줄
        }
        return content;
    }




}  // class

