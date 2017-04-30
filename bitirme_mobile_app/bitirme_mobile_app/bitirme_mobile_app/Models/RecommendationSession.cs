using bitirme_mobile_app.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class RecommendationSession
    {
        public ObservableCollection<MovieRateListViewItem> ratedMovies { get; set; }
        public ObservableCollection<MovieListViewItem> recommendedMovies { get; set; }
        public List<string> recommendedMovieIds { get; set; }
        public DateTime CreateDate { get; set; }

        //Movie data downladed 
        public bool isCompleted { get; set; }

        // ids avaliable but data must be gathered from network in a screen with a waiting msg
        public bool hasJustRecommendedIds { get; set; }



        public int id { get; set; }

        public RecommendationSession()
        {
            ratedMovies = new ObservableCollection<MovieRateListViewItem>();
            recommendedMovies = new ObservableCollection<MovieListViewItem>();
            recommendedMovieIds = new List<string>();
        }
    }
}
