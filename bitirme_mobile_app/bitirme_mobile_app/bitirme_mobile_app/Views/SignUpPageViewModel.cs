using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
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
    public class SignUpPageViewModel : ViewModelBase
    {

        public ICommand sendSignupRequestCommand { get; private set; }

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

        public SignUpPageViewModel(Page page)
        {
            _page = page;
            sendSignupRequestCommand = new Command(sendSignupRequest);
        }

        private async void sendSignupRequest()
        {
            User user = new User { Username = Name, Password = Password, DeviceId = getDeviceId() };
            IsBusy = true;
            var isLoggedIn = await new RestService().signup(user);
            IsBusy = false;
            if (isLoggedIn)
            {
                await _page.DisplayAlert("Info", "Signed up", null, "Ok");
                DBHelper.addUser(user);
                await MDPage.Instance.OpenMainPage();
            }
            else
            {
                await _page.DisplayAlert("Info", "Not Signed up", null, "Ok");
            }

        }

        private string getDeviceId()
        {
            return "qqq";
        }

    }
}
