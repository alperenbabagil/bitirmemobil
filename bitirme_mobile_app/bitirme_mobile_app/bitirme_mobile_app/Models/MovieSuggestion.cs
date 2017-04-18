using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Models
{
    /// <summary>
    /// This model used when getting movie name from movie initials. 
    /// Not taking all movie information because it must be fast
    /// </summary>
    public class MovieSuggestion
    {
        public string Name { get; set; }
        public int id { get; set; }
        public double rate { get; set; }
    }
}
