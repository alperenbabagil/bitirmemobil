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

        //public ICommand openRateMoviePageCommand { get; private set; }



        public RateMoviePageViewModel(RatingMoviesPage rateMoviePage)
        {
            _rateMoviePage = rateMoviePage;
            MoviesToRateListViewRefreshCommand = new Command(getMoviesToRate);
            SendRecommendationRequestCommand = new Command(sendRecommendationRequest);
            //MoviesToRateListViewItemClickCommand = new Command<MovieRateListViewItem>(movieToRateItemClick);
            //openRateMoviePageCommand = new Command(openRateMoviePage);
        }

        private void listViewItemClick(MovieRateListViewItem lvi)
        {
            //TODO: movie info popup will be showed
        }

        private async void getMoviesToRate()
        {
            var ids = await new RestService().getTop250Ids();
            var idx = new Random().Next() % 80;
            var movies = await new RestService().getMovieInfoFromWeb(ids, idx, idx+15);
            var lvis = new List<MovieRateListViewItem>();

            foreach (var movie in movies)
            {
                lvis.Add(new MovieRateListViewItem(this)
                {
                    Movie = movie,
                });
            }

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
            await new RestService().sendRecommendationRequest(RatedMovies,id);
            IsBusy = false;
            await _rateMoviePage.DisplayAlert("Info", "Request is sent successfully. Your movies will come soon as a push notification", "Ok");
            await DBHelper.updateDB();
            GeneralHelper.quitApp();
        }
        //private void addSessionToDb()
        //{
        //    var session = new RecommendationSession() { ratedMovies=RatedMovies.ToList()};
            
        //    DBHelper.addRecommendationSession(session);
        //}
    }
}
