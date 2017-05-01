using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using bitirme_mobile_app.Models;
using System.Windows.Input;
using Xamarin.Forms;
using bitirme_mobile_app.Helpers;
using bitirme_mobile_app.Pages;

namespace bitirme_mobile_app.Views
{
    public class MasterPageViewModel : ViewModelBase
    {
        private RangeEnabledObservableCollection<RecommendationSession> _sessions;

        public RangeEnabledObservableCollection<RecommendationSession> Sessions
        {
            get
            {
                return _sessions;
            }

            set
            {
                _sessions = value;
                notifyProperty("Sessions");
            }
        }

        private string _userName;

        private RecommendationSession _selectedRecSession;

        public ICommand LogOutCommand
        {
            get
            {
                return _logOutCommand;
            }

            set
            {
                _logOutCommand = value;
                notifyProperty("LogOutCommand");
            }
        }

        public RecommendationSession SelectedRecSession
        {
            get
            {
                return _selectedRecSession;
            }

            set
            {
                _selectedRecSession = value;
                notifyProperty("SelectedRecSession");
                if (value != null)
                {
                    _mdPage.changeRecommendationSessionOnMainPage((RecommendationSession) value);
                    _mdPage.IsPresented = false;
                }
            }
        }

        public string UserName
        {
            get
            {
                return _userName;
            }

            set
            {
                _userName = value;
                notifyProperty("UserName");
            }
        }

        private ICommand _logOutCommand;

        private MDPage _mdPage;

        public MasterPageViewModel(MDPage mdPage)
        {
            _mdPage = mdPage;
            UserName = App.CurrentUser.Username;
            Sessions = new RangeEnabledObservableCollection<RecommendationSession>();
            Sessions.InsertRange(App.RecommendationSessionHolder.getSessions());
            Sessions.Reverse();
            LogOutCommand = new Command(logOutFunction);

        }

        private void logOutFunction()
        {
            DBHelper.removeUser(App.CurrentUser);
            _mdPage.openLoginPage();
            _mdPage.IsPresented = false;
        }

        public void updateList()
        {
            Sessions.Clear();
            Sessions.InsertRange(App.RecommendationSessionHolder.getSessions());
            Sessions.Reverse();
        }



    }
}
