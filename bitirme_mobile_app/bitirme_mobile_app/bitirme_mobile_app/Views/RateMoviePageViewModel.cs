using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
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

namespace bitirme_mobile_app.Views
{
    /// <summary>
    /// Viewmodel for Movie Rating page
    /// </summary>
    public class RateMoviePageViewModel : ViewModelBase
    {
        private RatingMoviesPage _rateMoviePage;

        #region listview bindings
        private Command _moviesToRateListViewRefreshCommand;

        public Command MoviesToRateListViewRefreshCommand
        {
            get
            {
                return _moviesToRateListViewRefreshCommand;
            }

            set
            {
                _moviesToRateListViewRefreshCommand = value;
                notifyProperty("MoviesToRateListViewRefreshCommand");
            }
        }

        //private Command _moviesToRateListViewItemClickCommand;

        //public Command MoviesToRateListViewItemClickCommand
        //{
        //    get
        //    {
        //        return _moviesToRateListViewItemClickCommand;
        //    }

        //    set
        //    {
        //        _moviesToRateListViewItemClickCommand = value;
        //        notifyProperty("MoviesToRateListViewItemClickCommand");
        //    }
        //}

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

        MovieRateListViewItem _moviesToRateLVSelectedItem;

        public MovieRateListViewItem MoviesToRateLVSelectedItem
        {
            get
            {
                return _moviesToRateLVSelectedItem;
            }

            set
            {
                _moviesToRateLVSelectedItem = null;
                notifyProperty("MoviesToRateLVSelectedItem");
                listViewItemClick(value);
            }
        }

        MovieRateListViewItem _ratedMoviesLVSelectedItem;

        public MovieRateListViewItem RatedMoviesLVSelectedItem
        {
            get
            {
                return _ratedMoviesLVSelectedItem;
            }

            set
            {
                _ratedMoviesLVSelectedItem = null;
                notifyProperty("RatedMoviesLVSelectedItem");
                listViewItemClick(value);

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
        public RangeEnabledObservableCollection<MovieRateListViewItem> MoviesToRate
        {
            get
            {
                return _moviesToRate;
            }

            set
            {
                _moviesToRate = value;
                notifyProperty("MoviesToRate");
            }
        }

        

        RangeEnabledObservableCollection<MovieRateListViewItem> _ratedMovies = new RangeEnabledObservableCollection<MovieRateListViewItem>();
        RangeEnabledObservableCollection<MovieRateListViewItem> _moviesToRate = new RangeEnabledObservableCollection<MovieRateListViewItem>();

        #endregion

        private ICommand _sendRecommendationRequestCommand;

        public ICommand SendRecommendationRequestCommand
        {
            get
            {
                return _sendRecommendationRequestCommand;
            }

            set
            {
                _sendRecommendationRequestCommand = value;
            }
        }

        private ICommand _loadMoreCommand;

        public ICommand LoadMoreCommand
        {
            get
            {
                return _loadMoreCommand;
            }

            set
            {
                _loadMoreCommand = value;
                notifyProperty("LoadMoreCommand");
            }
        }

        //public ICommand openRateMoviePageCommand { get; private set; }

        public static Command UnrateMovieCommand { get; private set; }




        public RateMoviePageViewModel(RatingMoviesPage rateMoviePage)
        {
            _rateMoviePage = rateMoviePage;
            MoviesToRateListViewRefreshCommand = new Command(getMoviesToRate);
            SendRecommendationRequestCommand = new Command(sendRecommendationRequest);
            UnrateMovieCommand = new Command<MovieRateListViewItem>(unrateMovie);
            LoadMoreCommand = new Command(loadMoreFunction);
            //MoviesToRateListViewItemClickCommand = new Command<MovieRateListViewItem>(movieToRateItemClick);
            //openRateMoviePageCommand = new Command(openRateMoviePage);
        }

        private void loadMoreFunction()
        {
            if (!IsRefreshing)
            {
                IsRefreshing = true;
                System.Diagnostics.Debug.WriteLine("last");
                getMoviesToRate();
            }
            
        }

        private void unrateMovie(MovieRateListViewItem item)
        {
            RatedMovies.Remove(item);
            MoviesToRate.Add(item);
        }

        private void listViewItemClick(MovieRateListViewItem lvi)
        {
            //TODO: movie info popup will be showed
        }

        private async void getMoviesToRate()
        {
            var ids = await new RestService().getTop250Ids();

            if (ids == null)
            {
                await _rateMoviePage.DisplayAlert("Info","Connection error","Ok");
                _rateMoviePage.finishSelf();
                return;
            }
            //filtering some movies
            else
            {
                for(int i=ids.Count-1;i>-1;i--)
                {
                    if (ids[i] == "0119508") ids.Remove(ids[i]);
                }
            }

            //generates random distinct indexes to get movies that will be rated.
            var randomIndexes = Enumerable.Range(1, 200).OrderBy(g => Guid.NewGuid()).Take(Constants.MovieRateLimit).ToArray();
            var idlist = new List<string>();
            foreach(var idx in randomIndexes)
            {
                idlist.Add(ids[idx]);
            }
            var movies = await new RestService().getMovieInfoFromWeb(idlist);

            //filling listview 
            var lvis = new List<MovieRateListViewItem>();
            foreach (var movie in movies)
            {
                lvis.Add(new MovieRateListViewItem(this)
                {
                    Movie = movie,
                });
            }
            //to prevent rate same movies
            for(int i=lvis.Count-1;i>-1;i--)
            {
                foreach (var oldmovie in MoviesToRate)
                {
                    if(lvis.ElementAtOrDefault(i) != null)
                    {
                        if (lvis[i].Movie.ImdbId == oldmovie.Movie.ImdbId)
                        {
                            lvis.Remove(lvis[i]);
                        }
                    }
                }
            }

            //to prevent rate same movies
            for (int i = lvis.Count - 1; i > -1; i--)
            {
                foreach (var oldmovie in RatedMovies)
                {
                    if (lvis.ElementAtOrDefault(i) != null)
                    {
                        if (lvis[i].Movie.ImdbId == oldmovie.Movie.ImdbId)
                        {
                            lvis.Remove(lvis[i]);
                        }
                    }
                }
            }

            //MoviesToRate.Clear();
            MoviesToRate.InsertRange(lvis);
            IsRefreshing = false;
        }

        /// <summary>
        /// A delay for user see clicking on a star is worked.
        /// </summary>
        // is this the correct way?
        private MovieRateListViewItem tempMovieRateListViewItem;
        public void onMovieStarClicked(MovieRateListViewItem movieRateListViewItem)
        {
            tempMovieRateListViewItem = movieRateListViewItem;
            Device.StartTimer(TimeSpan.FromMilliseconds(300), swapMovies);
            
        }

        private bool swapMovies()
        {
            MoviesToRate.Remove(tempMovieRateListViewItem);
            RatedMovies.Add(tempMovieRateListViewItem);
            return false;
        }

        private async void sendRecommendationRequest()
        {
            var session = new RecommendationSession() { ratedMovies = RatedMovies,CreateDate=DateTime.Now };
            int id=App.RecommendationSessionHolder.addNewSession(session);         
            IsBusy = true;
            string result = "";
            result= await new RestService().sendRecommendationRequest(RatedMovies,id);
            IsBusy = false;
            if (result == "success")
            {
                await _rateMoviePage.DisplayAlert("Info", "Request is sent successfully. Your movies will come soon as a push notification", "Ok");
                await DBHelper.updateDB();
                GeneralHelper.quitApp();
            }
            else
            {
                await _rateMoviePage.DisplayAlert("Info", "Connection error", "Ok");
            }
            
        }
        //private void addSessionToDb()
        //{
        //    var session = new RecommendationSession() { ratedMovies=RatedMovies.ToList()};
            
        //    DBHelper.addRecommendationSession(session);
        //}
    }
}
