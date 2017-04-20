using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class RecommendationSession
    {
        public List<Movie> ratedMovies { get; set; }
        public List<Movie> recommendedMovies { get; set; }
    }
}
