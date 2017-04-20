using bitirme_mobile_app.Models;
using bitirme_mobile_app.Pages;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace bitirme_mobile_app.Views
{
    public partial class GoToMovieRatePagePopup : PopupPage
    {
        MainPage mainPage;

        public GoToMovieRatePagePopup(MainPage mainPage)
        {
            InitializeComponent();
            this.mainPage = mainPage;
            goToMovieRatePageButton.Clicked += (s, e) =>
             {
                 mainPage.openRateMoviePage();
                 mainPage.Navigation.PopAllPopupAsync(true);
             };

            
        }

        

        protected override bool OnBackgroundClicked()
        {
            mainPage.popupDisappeared();
            return false;
        }

        protected override bool OnBackButtonPressed()
        {
            mainPage.popupDisappeared();
            return false;
        }
    }
}
