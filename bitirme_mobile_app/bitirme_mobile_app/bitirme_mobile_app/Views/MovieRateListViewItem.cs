using bitirme_mobile_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace bitirme_mobile_app.Views
{
    public class MovieRateListViewItem : ViewModelBase
    {
        public Movie Movie { get; set; }
        //public bool IsImageLoading { get; set; }
        private int _rating;

        public int Rating {
            get
            {
                return _rating;
            }
            set
            {
                _rating = value;
                notifyProperty("Rating");
            }
        }


        public MovieRateListViewItem()
        {
            // configure the TapCommand with a method
            starTapCommand = new Command<string>((string rate) => 
            {
                Rating = Int32.Parse(rate);
            });
        }

        public ICommand starTapCommand { get; private set; }

    }
}
