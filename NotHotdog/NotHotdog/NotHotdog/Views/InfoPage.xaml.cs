using System;
using System.Collections.Generic;
using NotHotdog.Services;
using NotHotdog.ViewModels;
using Xamarin.Forms;

namespace NotHotdog.Views
{
    public partial class InfoPage : ContentPage
    {
        public InfoPage()
        {
            InitializeComponent();

			InfoViewModel vm = new InfoViewModel(DependencyService.Get<IHotDogRecognitionService>(), Navigation);
            BindingContext = vm;
        }
    }
}
