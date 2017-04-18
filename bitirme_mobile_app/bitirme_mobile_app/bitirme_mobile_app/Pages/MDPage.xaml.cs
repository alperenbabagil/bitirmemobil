using bitirme_mobile_app.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace bitirme_mobile_app.Pages
{
    public partial class MDPage : MasterDetailPage
    {

        protected override void OnAppearing() //page on the screen
        {
            base.OnAppearing();
        }

        public MDPage()
        {
            InitializeComponent();
            
            Detail = new DummyPage();

            if (AuthManager.IsLoggedIn())
            {
                Detail = new NavigationPage(new MainPage());
                Master = new MasterPage() { Title = "Master" };
            }
            else
            {
                Detail = new NavigationPage(new LoginPage());
                Master = new DummyPage() { Title = "Master" };
            }
        }
    }
}
