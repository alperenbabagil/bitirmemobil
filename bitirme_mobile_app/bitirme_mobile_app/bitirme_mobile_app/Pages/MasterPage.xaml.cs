using bitirme_mobile_app.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace bitirme_mobile_app.Pages
{
    public partial class MasterPage : ContentPage
    {
        MasterPageViewModel vm;
        public MasterPage(MDPage mdPage)
        {
            InitializeComponent();
            vm = new MasterPageViewModel(mdPage);
            BindingContext = vm;
        }

        public void updateList()
        {
            vm.updateList();
        }


    }
}
