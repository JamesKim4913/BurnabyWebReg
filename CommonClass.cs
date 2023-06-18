using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;  // control file
using System.Net;
using System.Web;    // add reference  System.Web
using System.Text.RegularExpressions;  // regular expression
using System.Windows.Forms;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Net.Http;


class CommonClass
{
    // open browser internet
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


    // Extract some from the entire source
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




    // Change Special Characters
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
        content = Regex.Replace(content, @"[^\p{L}\p{M}p{N}]+", " "); // Remove Unicode Emoticons       
        content = remove_html_tag(content);  // Remove tags
        content = RemoveSpace(content);  // Remove White Space   
        content = Regex.Replace(content, @"[^0-9a-zA-Zㄱ-힗]+", " ", RegexOptions.Singleline);  // Remove Special Characters
        content = Regex.Replace(content, @"[^a-zA-Z0-9가-힣]", " ", RegexOptions.Singleline);  
        content = Regex.Replace(content, "\n", " ", RegexOptions.IgnoreCase);
        content = content.Replace("n", " ");
        content = content.Trim();
        return content;
    }


    // Remove html tags
    public string remove_html_tag(string html_str)
    {       
        return Regex.Replace(html_str, @"[<][a-z|A-Z|/](.|\n)*?[>]", "");
    }

    // Remove the tag and float it to the space
    public string remove_html_tag2(string html_str)
    {      
        return Regex.Replace(html_str, @"[<][a-z|A-Z|/](.|\n)*?[>]", " ");
    }



    // Get random numbers
    public int Get_Randomnum(Random random, int minnum, int maxnum)
    {
        int RandomNumber = 0;
        //  Random random = new Random();
        RandomNumber = random.Next(minnum, maxnum);
        return RandomNumber;
    }





    // Save txt file
    public void Save_Txt(string filename, string content)
    {
        try
        {
            string myexePath = Application.StartupPath;  // Folder with executable files
            string pathfilename = myexePath + @"\" + filename + @".txt";  // File name folder path 

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
            //UpdateStatusBar(ex.Message);
        }
    }

    // Read txt file
    public string Read_Txt(string filename)
    {
        string content = string.Empty;
        string strFileLine = string.Empty;
        try
        {
            string myexePath = Application.StartupPath; 
            string pathfilename = myexePath + @"\" + filename + @".txt";

            if (File.Exists(pathfilename)) // If the file exists
            {               
                // StreamReader SRead = new StreamReader(pathfilename, System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
                StreamReader SRead = new StreamReader(pathfilename, Encoding.Default);

                while ((!SRead.EndOfStream))  // (strFileLine = SRead.ReadLine() != string.Empty))
                {
                    strFileLine = SRead.ReadLine();  // reading line by line
                    if ((strFileLine != string.Empty)) 
                    {
                        content = content + strFileLine + Environment.NewLine;
                    }  // if strFileLine
                } // while

                SRead.Close();
            }
        } // try
        catch (Exception ex)
        {
            // UpdateStatusBar(ex.Message);
        }
        return content;
    }


    // Save txt file Following the existing file
    public void Save_Txt_Conti(string filename, string content)
    {
        try
        {
            string myexePath = Application.StartupPath; 
            string pathfilename = myexePath + @"\" + filename + @".txt";

            //Pass the filepath and filename to the StreamWriter Constructor
            // StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.GetEncoding("EUC-KR"));

            // If the option is true in the middle, it will be added to the existing file
            StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.Default);  

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            // UpdateStatusBar(ex.Message); 
        }
    }



    // Read txt file
    public string ReadAll_Txt(string filename)
    {
        string content = string.Empty;
        string strFileLine = string.Empty;
        try
        {
            string myexePath = Application.StartupPath; 
            string pathfilename = myexePath + @"\" + filename + @".txt"; 

            if (File.Exists(pathfilename)) 
            {              
                // StreamReader SRead = new StreamReader(pathfilename, System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
                StreamReader SRead = new StreamReader(pathfilename, Encoding.Default);

                content = SRead.ReadToEnd();

                SRead.Close();
            }
        } // try
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message);
        }
        return content;
    }


    // Save txt file to full path, following existing file
    public void Save_fullpathTxt_Conti(string pathfilename, string content)
    {
        try
        {           
            StreamWriter sw = new StreamWriter(pathfilename, true, Encoding.Default);

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message);
        }
    }


    // Save txt file to full path
    public void Save_fullpathTxt(string pathfilename, string content)
    {
        try
        {           
            StreamWriter sw = new StreamWriter(pathfilename, false, Encoding.Default);

            //Write a line of text
            sw.WriteLine(content);

            //Close the file
            sw.Close();
        }
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message);
        }
    }


    // Read txt file
    public string Read_fullpathTxt(string pathfilename)
    {
        string content = string.Empty;
        string strFileLine = string.Empty;
        try
        {
            if (File.Exists(pathfilename)) 
            {               
                // StreamReader SRead = new StreamReader(pathfilename, System.Text.Encoding.GetEncoding("ks_c_5601-1987"));
                StreamReader SRead = new StreamReader(pathfilename, Encoding.Default);

                content = SRead.ReadToEnd();               

                SRead.Close();
            }
        } // try
        catch (Exception ex)
        {
            //UpdateStatusBar(ex.Message); 
        }
        return content;
    }




}  // class

