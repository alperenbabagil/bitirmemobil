using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Xamarin.Forms;
using Natra.Droid;
using bitirme_mobile_app.Interfaces;

[assembly: Dependency(typeof(CloseApplication))]
namespace Natra.Droid
{
    public class CloseApplication : ICloseApplication
    {
        public void closeApplication()
        {
            var activity = (Activity)Forms.Context;
            activity.FinishAffinity();

            //Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}