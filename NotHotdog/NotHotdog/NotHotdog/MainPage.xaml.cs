﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NotHotdog.ViewModels;
using Xamarin.Forms;

namespace NotHotdog
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            MainViewModel vm = new MainViewModel();
            BindingContext = vm;

            photoButton.Text = "\uf030";
            shareButton.Text = "\uf1e0";
            codeButton.Text = "\uf09b";
        }
    }
}
