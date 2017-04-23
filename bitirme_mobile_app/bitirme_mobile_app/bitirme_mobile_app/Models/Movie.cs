using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    public class Movie
    {
        public String ImdbId { get; set; }
        public string Name { get; set; }
        public List<string> Genres { get; set; }
        public string ImdbImageUrl { get; set; }
        public double Rating { get; set; }
        //***
    }
}
