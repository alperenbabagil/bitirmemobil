using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
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

        private static MDPage _instance;

        public static MDPage Instance
        {
            get
            {
                return _instance;
            }

            set
            {
                _instance = value;
            }
        }

        protected override void OnAppearing() //page on the screen
        {
            base.OnAppearing();
        }

        List<string> _recommendedMovies;

        private MainPage _mainPage;

        private MasterPage _masterPage;

        public MDPage(List<string> recommendedMovies)
        {
            _instance = this;

            _recommendedMovies = recommendedMovies;

            InitializeComponent();
            
            Detail = new DummyPage();

            if (AuthManager.IsLoggedIn())
            {
                _mainPage = new MainPage(_recommendedMovies);
                Detail = new NavigationPage(_mainPage);
                _masterPage= new MasterPage(this) { Title = "Master" };
                Master = _masterPage;
            }
            else
            {
                Detail = new NavigationPage(new LoginPage());
                Master = new DummyPage() { Title = "Master" };
            }

            this.IsPresentedChanged += OnPresentedChanged;
        }

        public async Task<bool> OpenMainPage()
        {
            await Detail.Navigation.PopToRootAsync();
            Detail = new NavigationPage(new MainPage(_recommendedMovies));
            return true;
        }

        public void openLoginPage()
        {
            Detail = new NavigationPage(new LoginPage());
            Master = new DummyPage() { Title = "Master" };
        }

        public void changeRecommendationSessionOnMainPage(RecommendationSession session)
        {
            _mainPage.mpvm.fillListViews(session);
        }

        private void OnPresentedChanged(object sender, EventArgs e)
        {
            if (this.IsPresented && App.masterPageListMustBeUpdated)
            {
                _masterPage.updateList();
                App.masterPageListMustBeUpdated = false;
            }
        }
    }
}
