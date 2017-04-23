using bitirme_mobile_app.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class RecommendationSession
    {
        public List<MovieRateListViewItem> ratedMovies { get; set; }
        public List<MovieListViewItem> recommendedMovies { get; set; } 

        public RecommendationSession()
        {
            ratedMovies = new List<MovieRateListViewItem>();
            recommendedMovies = new List<MovieListViewItem>();
        }
    }
}
