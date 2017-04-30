using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
using bitirme_mobile_app.Pages;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace bitirme_mobile_app
{
    //TODO: Page titles will be put
    //TODO: Master page will list previous recommendation sessions
    //TODO: Listview must show genres of movies and imdb or other rates
    //TODO: Main page rated movies ratings must not be canged by user
    //TODO: App design will be made
    //TODO: Notification and app icon will be changed
    //TODO: Filtering must be implemented when get movies for rating **
    //TODO: Logout functionality
    //TODO: Signin with facebook,google etc. authentication

    public class App : Application
    {
        public static User CurrentUser;
        public static RecommendationSessionHolder RecommendationSessionHolder;
        private MDPage mdPage;
        private static List<string> recommendedMovies = null;

        //will be checked when side menu opened
        public static bool masterPageListMustBeUpdated { get; set; } = false;
        public App()
        {
            DBHelper.DBInit();
            CurrentUser = DBHelper.getUser();
            mdPage= new MDPage(recommendedMovies);
            //MainPage = new NavigationPage(mainPage);
            MainPage = mdPage;
            Logger.Logcat(" MainPage = mdPage;");

            try
            {
                OneSignal.Current.StartInit("db4e3be5-9a31-4843-ba4b-d513c7bdcd8e")
                    .HandleNotificationReceived(HandleNotificationReceived)
                    .HandleNotificationOpened(HandleNotificationOpened)
                  .EndInit();
            }
            catch (Exception e)
            {
                //TODO: cannot initiate error. App must not go on
            }

            
        }

        private static void HandleNotificationReceived(OSNotification notification)
        {
            System.Diagnostics.Debug.WriteLine("HandleNotificationReceived");
            OSNotificationPayload payload = notification.payload;
            string message = payload.body;

            System.Diagnostics.Debug.WriteLine("GameControllerExample:HandleNotificationReceived: " + message);
            System.Diagnostics.Debug.WriteLine("displayType: " + notification.displayType);
            System.Diagnostics.Debug.WriteLine("Notification received with text: " + message);
        }

        // Called when a notification is opened.
        // The name of the method can be anything as long as the signature matches.
        // Method must be static or this object should be marked as DontDestroyOnLoad
        private static async void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            System.Diagnostics.Debug.WriteLine("HandleNotificationOpened");
            OSNotificationPayload payload = result.notification.payload;
            Dictionary<string, object> additionalData = payload.additionalData;
            string message = payload.body;
            string actionID = result.action.actionID;

            System.Diagnostics.Debug.WriteLine("GameControllerExample:HandleNotificationOpened: " + message);
            var extraMessage = "Notification opened with text: " + message;

            if (additionalData != null)
            {
                if (additionalData.ContainsKey("movies"))
                {
                    try
                    {
                        var typ = additionalData["movies"].GetType();
                        var str = additionalData["movies"].ToString();
                        var sesionId = Int32.Parse(additionalData["sessionId"].ToString());
                        Logger.Logcat("HandleNotificationOpened");
                        var movies = JsonConvert.DeserializeObject<List<string>>(str);
                        recommendedMovies = movies;
                        await DBHelper.addIdsToSession(movies, sesionId);
                        
                    }
                    catch (Exception e)
                    {
                        Logger.Logcat("HandleNotificationOpened error");
                    }

                    //App.Current.MainPage.DisplayAlert("Alert", typ.ToString(), "OK");
                    //extraMessage = (string)additionalData["foo"];
                    // Take user to your store.
                    //App.Current.MainPage.DisplayAlert("Alert", extraMessage, "OK");

                }
            }
            if (actionID != null)
            {
                // actionSelected equals the id on the button the user pressed.
                // actionSelected will equal "__DEFAULT__" when the notification itself was tapped when buttons were present.
                extraMessage = "Pressed ButtonId: " + actionID;
            }
            System.Diagnostics.Debug.WriteLine(extraMessage);
        }

        private void IdsAvailable(string userID, string pushToken)
        {
            System.Diagnostics.Debug.WriteLine("UserID: " + userID);
            System.Diagnostics.Debug.WriteLine("pushToken: " + pushToken);
            //("UserID:" + userID);
            //print("pushToken:" + pushToken);
        }


        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
