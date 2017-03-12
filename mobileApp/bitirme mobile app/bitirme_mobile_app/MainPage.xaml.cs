using bitirme_mobile_app.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Auth;
using Xamarin.Forms;

namespace bitirme_mobile_app
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            //var auth = new OAuth2Authenticator(
            //  Constants.ClientId,
            //  Constants.ClientSecret,
            //  Constants.Scope,
            //  new Uri(Constants.AuthorizeUrl),
            //  new Uri(Constants.RedirectUrl),
            //  new Uri(Constants.AccessTokenUrl));

            //var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            //presenter.Login(auth);

            textimiz.Text = "aliveli";
        }
    }
}
