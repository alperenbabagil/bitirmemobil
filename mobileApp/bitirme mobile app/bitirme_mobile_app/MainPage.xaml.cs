using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
using bitirme_mobile_app.Views;
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

            openRatePage.Clicked += (s, e) =>
             {
                 App.Current.MainPage.Navigation.PushAsync(new RatingMoviesPage());
             };

            getRecommendationsBtn.Clicked += (s, e) =>
              {
                  var recReq1 = new RecommendationRequest() { movieId = 1, rating = 3.0 };
                  var recReq2 = new RecommendationRequest() { movieId = 2, rating = 4.0 };

                  var list = new List<RecommendationRequest>() { recReq1, recReq2 };
                  getRecommendations(list);
              };


            getTop250.Clicked += async (s, e) =>
            {
                var ids = await new RestService().getTop250Ids();
                var movies = await new RestService().getMovieInfoFromWeb(ids, 30, 40);
                moviesListView.ItemsSource = movies;
            };









        }

        private async void getRecommendations(List<RecommendationRequest> list)
        {
            var list2 = await new RestService().getRecommendations(list);
        }

        private async void getTop(List<RecommendationRequest> list)
        {

        }
    }
}
