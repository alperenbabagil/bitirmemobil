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
using Android.Util;
using bitirme_mobile_app.Interfaces;

[assembly: Dependency(typeof(Logger))]
namespace Natra.Droid
{
    public class Logger : IAndroidLog
    {
        public void log(string s)
        {
            Log.Debug("natraLog",s);
        }
    }
}