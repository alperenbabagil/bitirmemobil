using bitirme_mobile_app.Interfaces;
using bitirme_mobile_app.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace bitirme_mobile_app.Helpers
{
    public class DBHelper
    {
        public static bool checkLoggedIn()
        {
            return App.Current.Properties.ContainsKey("User");
            //try
            //{
            //    if (!App.Current.Properties.ContainsKey("User"))
            //    {
            //        App.Current.Properties["LoginStatus"] = "false";
            //        App.Current.SavePropertiesAsync();
            //        DependencyService.Get<IAndroidLog>().log("checkLoggedIn2");
            //        return false;
            //    }
            //    var res = (App.Current.Properties["LoginStatus"].Equals("true"));
            //    DependencyService.Get<IAndroidLog>().log("checkLoggedIn3" + res);
            //    return res;

            //}
            //catch (Exception e)
            //{
            //    Logger.errLog("DBHelper - checkLoggedIn", e);
            //    throw e;
            //}
        }

        public static void addUser(User user)
        {
            App.Current.Properties["User"] = JsonConvert.SerializeObject(user);
            Application.Current.SavePropertiesAsync();
        }

        public static User getUser()
        {
            if(App.Current.Properties.ContainsKey("User"))
                return JsonConvert.DeserializeObject<User>(App.Current.Properties["User"] as string);
            return null;
        }

        public static void removeUser(User user)
        {
            App.Current.Properties.Remove("User");
        }

        public static RecommendationSession getLastSession()
        {
            if (App.Current.Properties.ContainsKey("RecommendationSessionHolder"))
            {
                var holder = JsonConvert.DeserializeObject<RecommendationSessionHolder>(App.Current.Properties["RecommendationSessionHolder"] as string);
                return holder.recommendationSessions.LastOrDefault();
            }
            return null;
        }
    }
}
