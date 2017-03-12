using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace bitirme_mobile_app.Droid
{
    [Activity(Label = "bitirme_mobile_app", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            Xamarin.Auth.Presenters.OAuthLoginPresenter.PlatformLogin = (authenticator) =>
            {
                var oAuthLogin = new OAuthLoginPresenter();
                oAuthLogin.Login(authenticator);
            };

            LoadApplication(new App());
        }
    }
}

