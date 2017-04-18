using bitirme_mobile_app.Models;
using bitirme_mobile_app.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace bitirme_mobile_app
{
    public class App : Application
    {
        public static User CurrentUser;
        private MainPage mainPage;
        public App()
        {
            mainPage = new Pages.MainPage();
            //MainPage = new NavigationPage(mainPage);
            MainPage = new MDPage();
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
