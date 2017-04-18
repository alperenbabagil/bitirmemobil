using bitirme_mobile_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace bitirme_mobile_app.Views
{
    public class MovieListViewItem
    {
        public Movie Movie { get; set; }
        //public bool IsImageLoading { get; set; }
        Image Image { get; set; }
    }
}
