﻿using bitirme_mobile_app.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace bitirme_mobile_app.Pages
{
    /// <summary>
    /// Log in for user. 
    /// TODO: Will be implemented
    /// </summary>
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();

            BindingContext = new LoginPageViewModel(this);
        }
    }
}
