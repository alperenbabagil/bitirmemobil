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

        private bool _noMovieRecommended = false;

        public bool NoMovieRecommended
        {
            get
            {
                return _noMovieRecommended;
            }

            set
            {
                _noMovieRecommended = value;
                notifyProperty("NoMovieRecommended");
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
                notifyProperty("WaitingForRecommendation");
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

        public ICommand OpenRateMoviePageCommand { get; private set; }

        /// <summary>
        /// to make clear selections of listviews
        /// </summary>
        private Object _listViewSelectedItem;
        public Object ListViewSelectedItem
        {
            get
            {
                return _listViewSelectedItem;
            }
            set
            {
                _listViewSelectedItem = null;
                notifyProperty("ListViewSelectedItem");
            }
        }

        public MainPageViewModel(MainPage mainPage, List<string> recommendedMoviesList)
        {
            _mainPage = mainPage;
            OpenRateMoviePageCommand = new Command(openRateMoviePage);
            var lastSession = DBHelper.getLastSession();

            if (lastSession == null)
            {
                // it is checked in MainPage onAppearing
                NoRecommendationSessionBefore = true;
            }
            else
            {
                fillListViews(lastSession);
            }

            //if (recommendedMoviesList == null)
            //{                
            //    if (lastSession == null)
            //    {
            //        NoRecommendationSessionBefore = true;
            //    }
            //    else
            //    {
            //        RatedMovies.InsertRange(lastSession.ratedMovies);
            //        //TODO: What if no movie recommended by server ?
            //        if (lastSession.recommendedMovies!=null && lastSession.recommendedMovies.Count == 0)
            //        {
            //            WaitingForRecommendation = true;
            //        }         
            //        else RecommendedMovies.InsertRange(lastSession.recommendedMovies);
            //    }
            //}
            //else
            //{
            //    fillLists(lastSession, recommendedMoviesList);
            //}
        }

        


        public async void fillListViews(RecommendationSession session)
        {
            RatedMovies.Clear();
            RecommendedMovies.Clear();
            RatedMovies.InsertRange(session.ratedMovies);
            if (session.isCompleted)
            {
                // movie data is taken before
                RecommendedMovies.InsertRange(session.recommendedMovies);
                NoMovieRecommended = RecommendedMovies.Count == 0 ? true : false;

            }
            else
            {
                if (session.hasJustRecommendedIds)
                {
                    //TODO: NO refreshing activity indicator
                    IsBusy = true;
                    // get movie data
                    var movies = await new RestService().getMovieInfoFromWeb(session.recommendedMovieIds, 0, session.recommendedMovieIds.Count);
                    //convert movies to movielistviewitems
                    var movieLvis = MovieListViewItem.convertMovieListToMovieListViewItemList(movies);
                    session.recommendedMovies = movieLvis;
                    //because it is completed now
                    session.hasJustRecommendedIds = false;
                    session.isCompleted = true;
                    await DBHelper.updateDB();
                    WaitingForRecommendation = false;
                    // inserting movies inlo listview's list
                    App.masterPageListMustBeUpdated = true;
                    RecommendedMovies.InsertRange(movieLvis);
                    NoMovieRecommended = RecommendedMovies.Count == 0 ? true : false;
                    IsBusy = false;
                    
                }
                else WaitingForRecommendation = true;
            }
        }

        public void openRateMoviePage()
        {
            _mainPage.Navigation.PushAsync(new RatingMoviesPage());
        }

        //private async void  fillLists(RecommendationSession session, List<string> recommendedMoviesList)
        //{
        //    RatedMovies.InsertRange(session.ratedMovies);
        //    IsRefreshing = true;
        //    var movies= await new RestService().getMovieInfoFromWeb(recommendedMoviesList, 0, recommendedMoviesList.Count);
        //    var movieLvis = MovieListViewItem.convertMovieListToMovieListViewItemList(movies);
        //    IsRefreshing = false;
        //    RecommendedMovies.InsertRange(movieLvis);
        //    Logger.Logcat(movieLvis.ToString());
        //    var ls=DBHelper.getLastSession();

        //    // recommendations came
        //    if(ls.recommendedMovies==null || (ls.recommendedMovies!=null && ls.recommendedMovies.Count == 0))
        //    {
        //        ls.recommendedMovies.AddRange(movieLvis);
        //        DBHelper.updateDB();
        //    }

        //}

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
