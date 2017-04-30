using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Interfaces;
using bitirme_mobile_app.Models;
using bitirme_mobile_app.Views;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace bitirme_mobile_app.Pages
{
    /// <summary>
    /// In main page last recommended movies are shown. If there is none a button showed that
    /// directs user to rate movie page
    /// </summary>
    public partial class MainPage : TabbedPage
    {

        public MainPageViewModel mpvm { get; private set; }

        /// <summary>
        /// it shows popup here because popup needs to page be created first
        /// </summary>
        protected override void OnAppearing() //page on the screen
        {
            base.OnAppearing();
            if (mpvm.NoRecommendationSessionBefore) showGoToRatingPagePopup();
        }

        public MainPage(List<string> recommendedMoviesList)
        {

            InitializeComponent();

            mpvm = new MainPageViewModel(this, recommendedMoviesList);

            ToolbarItems.Add(new ToolbarItem() { Text="New Recommendation",Command=new Command(openRateMoviePage) });


            BindingContext = mpvm;

            //var auth = new OAuth2Authenticator(
            //  Constants.ClientId,
            //  Constants.ClientSecret,
            //  Constants.Scope,
            //  new Uri(Constants.AuthorizeUrl),
            //  new Uri(Constants.RedirectUrl),
            //  new Uri(Constants.AccessTokenUrl));

            //var presenter = new Xamarin.Auth.Presenters.OAuthLoginPresenter();
            //presenter.Login(auth);

            //openRatePage.Clicked += (s, e) =>
            //{
            //    Navigation.PushAsync(new RatingMoviesPage());
            //};

            //getRecommendationsBtn.Clicked += (s, e) =>
            //{
            //    var recReq1 = new RecommendationRequest() { movieId = 1, rating = 3.0 };
            //    var recReq2 = new RecommendationRequest() { movieId = 2, rating = 4.0 };

            //    var list = new List<RecommendationRequest>() { recReq1, recReq2 };
            //    getRecommendations(list);
            //};


            //getTop250.Clicked += async (s, e) =>
            //{
            //    var ids = await new RestService().getTop250Ids();
            //    var movies = await new RestService().getMovieInfoFromWeb(ids, 30, 40);
            //    moviesListView.ItemsSource = movies;
            //};
        }

        public async void showGoToRatingPagePopup()
        {
            try
            {
                var p = new GoToMovieRatePagePopup(this);
                await Navigation.PushPopupAsync(p);

            }
            catch (Exception e)
            {

            }
        }

        public void popupDisappeared()
        {
            GeneralHelper.quitApp();
        }

        public void openRateMoviePage()
        {
            Navigation.PushAsync(new RatingMoviesPage());
        }

        //private async void getRecommendations(List<RecommendationRequest> list)
        //{
        //    var list2 = await new RestService().getRecommendations(list);
        //}

        //private async void getTop(List<RecommendationRequest> list)
        //{

        //}
    }
}
