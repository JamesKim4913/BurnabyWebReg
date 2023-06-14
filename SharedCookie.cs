using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;


// Another form can share this cookie
public static class SharedCookie
{
    public static CookieContainer CookieJar = new CookieContainer();
}
