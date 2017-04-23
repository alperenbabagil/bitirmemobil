using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Models;
using bitirme_mobile_app.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace bitirme_mobile_app.Pages
{
    /// <summary>
    /// Page for get movies to rate, rate movies and send request for 
    /// recommendation.
    /// 
    /// TODO: put a close button to movies. In not rated tab, it will delete movie from the list
    /// In rated tab, it resets movie's rating and put it back to movies to rate list
    /// 
    ///
    /// </summary>
    public partial class RatingMoviesPage : TabbedPage
    {
        private bool listViewMustRefresh = true;
        //ObservableCollection<MovieRateListViewItem> listViwData;

        protected override void OnAppearing() //page on the screen
        {
            base.OnAppearing();

            //listViewMustRefresh = false;

            if (listViewMustRefresh)
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(500), listviewRefreshFunc);
                listViewMustRefresh = false;
            }
        }

        public RatingMoviesPage()
        {
            InitializeComponent();
            BindingContext = new RateMoviePageViewModel(this);
            //RatingMovieListView.RefreshCommand = new Command(fillListView);
        }

        //private async void fillListView()
        //{
        //    var ids = await new RestService().getTop250Ids();
        //    var movies = await new RestService().getMovieInfoFromWeb(ids, 30, 40);
        //    var lvis = new List<MovieRateListViewItem>();

        //    foreach (var movie in movies)
        //    {
        //        lvis.Add(new MovieRateListViewItem()
        //        {
        //            Movie = movie,
        //        });
        //    }

        //    listViwData = new ObservableCollection<MovieRateListViewItem>(lvis);
        //    RatingMovieListView.ItemsSource = listViwData;
        //    RatingMovieListView.IsRefreshing = false;
        //}

        private bool listviewRefreshFunc()
        {
            MoviesToRateListView.BeginRefresh();

            return false;
        }
    }
}
