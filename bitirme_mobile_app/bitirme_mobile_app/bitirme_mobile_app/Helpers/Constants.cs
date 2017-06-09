using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Helpers
{
    public class Constants
    {
        public static string ip
        {
            get
            {
                if (App.Current.Properties.ContainsKey("serverIp"))
                {
                    return App.Current.Properties["serverIp"] as string;
                }
                return null;
            }
            set
            {
                //TODO: Not a best way
                App.Current.Properties["serverIp"] =value;
            }
        }

        public static int MovieRateLimit
        {
            get
            {
                if (App.Current.Properties.ContainsKey("MovieRateLimit"))
                {
                    return (int) App.Current.Properties["MovieRateLimit"];
                }
                else
                {
                    App.Current.Properties["MovieRateLimit"] = 10;
                }
                return 10;
            }
            set
            {
                //TODO: Not a best way
                App.Current.Properties["MovieRateLimit"] = value;
            }
        }

    }
}
