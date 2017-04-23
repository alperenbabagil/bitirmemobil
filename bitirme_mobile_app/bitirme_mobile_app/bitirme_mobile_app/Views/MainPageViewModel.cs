using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Pages;
using bitirme_mobile_app.Views;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace bitirme_mobile_app.Models
{
    /// <summary>
    /// In main page last recommended movies are shown. If there is none a button showed that
    /// directs user to rate movie page
    /// </summary>
    public class MainPageViewModel : ViewModelBase
    {
        private MainPage _mainPage;

        #region state variables

        private bool _noRecommendationSessionBefore = false;

        public bool NoRecommendationSessionBefore
        {
            get
            {
                return _noRecommendationSessionBefore;
            }

            set
            {
                _noRecommendationSessionBefore = value;
                notifyProperty("NoRecommendationSessionBefore");
            }
        }

        private bool _waitingForRecommendation = false;

        public bool WaitingForRecommendation
        {
            get
            {
                return _waitingForRecommendation;
            }

            set
            {
                _waitingForRecommendation = value;
            }
        }

        private bool _isRefreshing = false;

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                notifyProperty("IsRefreshing");
            }
        }

        #endregion

        #region Lists

        public RangeEnabledObservableCollection<MovieRateListViewItem> RatedMovies
        {
            get
            {
                return _ratedMovies;
            }

            set
            {
                _ratedMovies = value;
                notifyProperty("RatedMovies");
            }
        }
        public RangeEnabledObservableCollection<MovieListViewItem> RecommendedMovies
        {
            get
            {
                return _recommendedMovies;
            }

            set
            {
                _recommendedMovies = value;
                notifyProperty("RecommendedMovies");
            }
        }

        RangeEnabledObservableCollection<MovieRateListViewItem> _ratedMovies = new RangeEnabledObservableCollection<MovieRateListViewItem>();
        RangeEnabledObservableCollection<MovieListViewItem> _recommendedMovies = new RangeEnabledObservableCollection<MovieListViewItem>();

        #endregion

        //public ICommand openRateMoviePageCommand { get; private set; }

        public MainPageViewModel(MainPage mainPage, List<string> recommendedMoviesList)
        {
            _mainPage = mainPage;
            //openRateMoviePageCommand = new Command(openRateMoviePage);
            var lastSession = DBHelper.getLastSession();

            if (recommendedMoviesList == null)
            {                
                if (lastSession == null)
                {
                    NoRecommendationSessionBefore = true;
                }
                else
                {
                    RatedMovies.InsertRange(lastSession.ratedMovies);
                    //TODO: What if no movie recommented by server ?
                    if (lastSession.recommendedMovies!=null && lastSession.recommendedMovies.Count == 0)
                    {
                        WaitingForRecommendation = true;
                    }         
                    else RecommendedMovies.InsertRange(lastSession.recommendedMovies);
                }
            }
            else
            {
                fillLists(lastSession, recommendedMoviesList);
            }
        }

        private async void  fillLists(RecommendationSession session, List<string> recommendedMoviesList)
        {
            RatedMovies.InsertRange(session.ratedMovies);
            IsRefreshing = true;
            var movies= await new RestService().getMovieInfoFromWeb(recommendedMoviesList, 0, recommendedMoviesList.Count);
            var movieLvis = MovieListViewItem.convertMovieListToMovieListViewItemList(movies);
            IsRefreshing = false;
            RecommendedMovies.InsertRange(movieLvis);
            Logger.Logcat(movieLvis.ToString());
            var ls=DBHelper.getLastSession();
            if(ls.recommendedMovies==null || (ls.recommendedMovies!=null && ls.recommendedMovies.Count == 0))
            {
                ls.recommendedMovies.AddRange(movieLvis);
                DBHelper.updateDB();
            }
            
        }

        //private async Task<List<Movie>> getMoviesToRate(List<string> recommendedMoviesList)
        //{
        //    var movies = await new RestService().getMovieInfoFromWeb(recommendedMoviesList, 30, 40);
        //    var lvis = new List<MovieRateListViewItem>();

        //    foreach (var movie in movies)
        //    {
        //        lvis.Add(new MovieRateListViewItem(null)
        //        {
        //            Movie = movie,
        //        });
        //    }
        //    return lvis;
        //}



    }
}
