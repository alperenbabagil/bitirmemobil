using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Views
{
    public class SettingsPageViewModel : ViewModelBase
    {
        public string LocalIp
        {
            get
            {
                return Constants.ip;
            }
            set
            {
                Constants.ip=value;
            }
        }

        public int MovieRateLimit
        {
            get
            {
                return Constants.MovieRateLimit;
            }
            set
            {
                Constants.MovieRateLimit = value;
            }
        }

        public SettingsPageViewModel(SettingsPage page)
        {

        }

        
    }
}
