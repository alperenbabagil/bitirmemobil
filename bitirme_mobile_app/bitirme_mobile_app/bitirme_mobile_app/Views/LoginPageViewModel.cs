using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace bitirme_mobile_app.Views
{
    public class LoginPageViewModel :ViewModelBase
    {
        public ICommand sendLoginRequestCommand { get; private set; }
        public ICommand openSignupPageCommand { get; private set; }

        private string _name;
        private string _password;

        public string Password
        {
            get
            {
                return _password;
            }

            set
            {
                _password = value;
                notifyProperty("Password");
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
                notifyProperty("Name");
            }
        }

        private Page _page;

        public LoginPageViewModel(Page page)
        {
            _page = page;
            sendLoginRequestCommand = new Command(sendLoginRequest);
            openSignupPageCommand = new Command(openSignUpPage);
        }

        private async void sendLoginRequest()
        {
            IsBusy = true;
            var isLoggedIn=await new RestService().login(new Models.User { Username = Name, Password = Password, DeviceId = getDeviceId() });
            IsBusy = false;
            if (isLoggedIn)
            {
                await _page.DisplayAlert("Info", "Logged In", null , "Ok");
                await _page.Navigation.PopToRootAsync();
            }
            else
            {
                await _page.DisplayAlert("Info", "Not logged In", null, "Ok");
            }
            
        }

        private void openSignUpPage()
        {
            _page.Navigation.PushAsync(new SignUpPage());
        }

        private string getDeviceId()
        {
            return null;
        }
    }

    
}
