using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;



public class WebControl
{
    public string UserAgent = "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36";




    // Get send
    public string GetSend(string url)
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        webRequest.Method = "GET";
        webRequest.CookieContainer = SharedCookie.CookieJar;
        webRequest.KeepAlive = true;
        webRequest.ContentType = "text/html; Charset=utf-8";
        webRequest.UserAgent = UserAgent;


        // Get the response
        int statusCode = 0;
        string sourceCode = string.Empty;

        try
        {
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            statusCode = (int)webResponse.StatusCode;

            StreamReader readContent = new StreamReader(webResponse.GetResponseStream());
            sourceCode = readContent.ReadToEnd();

            webResponse.Close();
            webResponse = null;
        }
        catch (WebException xc)
        {
            if (xc.Response is HttpWebResponse)
            {
                HttpWebResponse rs = xc.Response as HttpWebResponse;
                StreamReader readContent = new StreamReader(rs.GetResponseStream());
                if (readContent != null)
                {
                    sourceCode = readContent.ReadToEnd();
                }

                statusCode = (int)rs.StatusCode;
            }
            else
            {
                statusCode = (int)xc.Status;
                sourceCode = xc.Message;
            }
        }
        catch (Exception xc)
        {
            sourceCode = xc.Message;
        }

        return sourceCode;
    }


    // Post send
    public string PostSend(string postdata, string url)
    {
        HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
        webRequest.Method = "POST";
        webRequest.CookieContainer = SharedCookie.CookieJar;
        webRequest.KeepAlive = true;
        webRequest.ContentType = "application/x-www-form-urlencoded";
        webRequest.UserAgent = UserAgent;

        byte[] buffer = Encoding.UTF8.GetBytes(postdata);

        webRequest.ContentLength = buffer.Length;

        Stream dataStream = webRequest.GetRequestStream();
        dataStream.Write(buffer, 0, buffer.Length);
        dataStream.Close();


        // Get the response
        int statusCode = 0;
        string sourceCode = string.Empty;

        try
        {
            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            statusCode = (int)webResponse.StatusCode;

            StreamReader readContent = new StreamReader(webResponse.GetResponseStream());
            sourceCode = readContent.ReadToEnd();

            webResponse.Close();
            webResponse = null;
        }
        catch (WebException xc)
        {
            if (xc.Response is HttpWebResponse)
            {
                HttpWebResponse rs = xc.Response as HttpWebResponse;
                StreamReader readContent = new StreamReader(rs.GetResponseStream());
                if (readContent != null)
                {
                    sourceCode = readContent.ReadToEnd();
                }

                statusCode = (int)rs.StatusCode;
            }
            else
            {
                statusCode = (int)xc.Status;
                sourceCode = xc.Message;
            }
        }
        catch (Exception xc)
        {
            sourceCode = xc.Message;
        }

        return sourceCode;
    }



    public void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                throw;
            }
        }
    }

}  // class

