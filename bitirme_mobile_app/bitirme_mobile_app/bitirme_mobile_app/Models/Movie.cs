using Newtonsoft.Json;
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
        public string Year { get; set; }
        public List<string> Genres { get; set; }
        public string ImdbImageUrl { get; set; }
        public double Rating { get; set; }
        //***

        [JsonIgnore]
        public string GenreString
        {
            get
            {
                string s = "";
                if (Genres != null)
                {
                    foreach (var str in Genres)
                    {
                        s += str + ",";
                    }
                    s=s.Remove(s.Length - 1);
                }
                return s;
                
            }
        }

        [JsonIgnore]
        public string NameYearString
        {
            get
            {
                return Name+" "+Year;

            }
        }
    }
}
