using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Views
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                notifyProperty("IsBusy");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;   

        public void notifyProperty(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propName));
            }
        }
    }
}
