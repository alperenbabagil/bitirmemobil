using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
using bitirme_mobile_app.Pages;
using Com.OneSignal;
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

        private  void sendSignupRequest()
        {
            OneSignal.Current.IdsAvailable(IdsAvailable);
        }

        private async void  IdsAvailable(string userID, string pushToken)
        {

            User user = new User { Username = Name, Password = Password, DeviceId = userID };

            IsBusy = true;
            var isLoggedIn = await new RestService().signup(user);

            
            if (isLoggedIn)
            {
                DBHelper.addUser(user);
                App.CurrentUser = user;
                IsBusy = false;
                await _page.DisplayAlert("Info", "Signed up", null, "Ok");               
                await MDPage.Instance.OpenMainPage();
            }
            else
            {
                IsBusy = false;
                await _page.DisplayAlert("Info", "Not Signed up", null, "Ok");
            }

            System.Diagnostics.Debug.WriteLine("UserID: " + userID);
            System.Diagnostics.Debug.WriteLine("pushToken: " + pushToken);
            //("UserID:" + userID);
            //print("pushToken:" + pushToken);
        }



    }
}
