

using Xamarin.Auth;

namespace bitirme_mobile_app.Droid
{
    public class OAuthLoginPresenter
    {
        public void Login(Authenticator authenticator)
        {
            
            Xamarin.Forms.Forms.Context.StartActivity(authenticator.GetUI(Xamarin.Forms.Forms.Context).GetType());
        }
    }
}