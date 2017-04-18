using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class RecommendationRequest
    {
        public int movieId { get; set; }
        public double rating { get; set; }
    }
}
