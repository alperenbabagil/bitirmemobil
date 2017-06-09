using bitirme_mobile_app.Interfaces;
using bitirme_mobile_app.Models;
using Com.OneSignal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace bitirme_mobile_app.Helpers
{
    public class GeneralHelper
    {
        private static string deviceId = "";
        public  static bool quitApp()
        {
            var closer = DependencyService.Get<ICloseApplication>();
            if (closer != null)
            {
                closer.closeApplication();
                return true;
            }
            return false;
        }


        public static string getDeviceId()
        {
            OneSignal.Current.IdsAvailable(IdsAvailable);
            return deviceId;
        }

        private static void IdsAvailable(string userID, string pushToken)
        {
            deviceId = userID;
        }

        //public static List<Movie> parseMovieRecommendationString(string str)
        //{
        //    var ids = JsonConvert.DeserializeObject<List<string>>(str);
        //    var movies = new List<Movie>();
        //    foreach (var id in ids)
        //    {
        //        movies.Add(new Movie() {  });
        //    }
        //}
    }
}
