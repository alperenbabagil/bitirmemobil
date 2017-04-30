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

        public static void addRecommendationSession(RecommendationSession session)
        {
            App.RecommendationSessionHolder.addNewSession(session);
            updateDB();
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
            if (App.RecommendationSessionHolder != null)
            {
                if (App.RecommendationSessionHolder.getSessions() != null)
                {
                    //TODO: Remove
                    var sessions = App.RecommendationSessionHolder.getSessions();
                    var sessions2 = App.RecommendationSessionHolder.getSessions().LastOrDefault();
                    return App.RecommendationSessionHolder.getSessions().LastOrDefault();
                }
            }
            //if (App.Current.Properties.ContainsKey("RecommendationSessionHolder"))
            //{
            //    var holder = JsonConvert.DeserializeObject<RecommendationSessionHolder>(App.Current.Properties["RecommendationSessionHolder"] as string);
            //    return holder.recommendationSessions.LastOrDefault();
            //}
            return null;
        }

        public static async void DBInit()
        {
            if (App.Current.Properties.ContainsKey("RecommendationSessionHolder"))
            {
                App.RecommendationSessionHolder = JsonConvert.DeserializeObject<RecommendationSessionHolder>(App.Current.Properties["RecommendationSessionHolder"] as string);
            }
            else
            {
                App.RecommendationSessionHolder = new RecommendationSessionHolder();
                await updateDB();
            }
        }

        public async static Task updateDB()
        {
            App.Current.Properties["RecommendationSessionHolder"] = JsonConvert.SerializeObject(App.RecommendationSessionHolder);
            await Application.Current.SavePropertiesAsync();
        }

        public static async Task<RecommendationSession> addIdsToSession(List<string> ids, int id)
        {
            var ses = getSessionWithId(id);
            if (ses != null)
            {
                ses.recommendedMovieIds = ids;
                ses.hasJustRecommendedIds = true;

                await updateDB();
                return ses;

            }
            return null;
        }

        public static RecommendationSession getSessionWithId(int id)
        {
            if (App.RecommendationSessionHolder != null)
            {
                foreach (var ses in App.RecommendationSessionHolder.getSessions())
                {
                    if (id == ses.id) return ses;
                }
            }
            return null;
        }
    }
}
