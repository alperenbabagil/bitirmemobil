using bitirme_mobile_app.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace bitirme_mobile_app.Views
{
    public class MovieListViewItem : ViewModelBase
    {

        private Movie movie;
       
        public Movie Movie
        {
            get
            {
                return movie;
            }

            set
            {
                movie = value;
                notifyProperty("Movie");
            }
        }

        public static ObservableCollection<MovieListViewItem> convertMovieListToMovieListViewItemList(List<Movie> movies)
        {
            var lvis = new ObservableCollection<MovieListViewItem>();
            foreach (var mv in movies)
            {
                lvis.Add(new MovieListViewItem() { Movie = mv });
            }
            return lvis;
        }


    }
}
