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

        #region Lists

        public RangeEnabledObservableCollection<Movie> RatedMovies
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
        public RangeEnabledObservableCollection<Movie> RecommendedMovies
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

        RangeEnabledObservableCollection<Movie> _ratedMovies = new RangeEnabledObservableCollection<Movie>();
        RangeEnabledObservableCollection<Movie> _recommendedMovies = new RangeEnabledObservableCollection<Movie>();

        #endregion

        //public ICommand openRateMoviePageCommand { get; private set; }

        public MainPageViewModel(MainPage mainPage)
        {
            _mainPage = mainPage;
            //openRateMoviePageCommand = new Command(openRateMoviePage);

            var lastSession = DBHelper.getLastSession();
            if (lastSession == null)
            {
                NoRecommendationSessionBefore = true;
            }
            else fillLists(lastSession);


        }

        private void fillLists(RecommendationSession session)
        {
            RatedMovies.InsertRange(session.ratedMovies);
            RecommendedMovies.InsertRange(session.recommendedMovies);
        }

       

    }
}
