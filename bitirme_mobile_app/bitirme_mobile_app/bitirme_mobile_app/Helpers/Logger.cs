using bitirme_mobile_app.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace bitirme_mobile_app.Helpers
{
    public class Logger
    {
        public static void errLog(string key, Exception e)
        {
            string msg = "";
            if (e != null) msg = e.Message;
            System.Diagnostics.Debug.WriteLine(string.Format("natra err: {0}  -  {1}", key, msg));
        }

        public static void Logcat(string msg)
        {
            var logger = DependencyService.Get<IAndroidLog>();
            if (logger != null)
            {
                logger.log(msg);
            }
        }
    }
}
