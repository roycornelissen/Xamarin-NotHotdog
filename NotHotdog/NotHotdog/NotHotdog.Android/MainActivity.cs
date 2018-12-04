using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.Permissions;
using Microsoft.Azure.Mobile.Push;
using Xamarin.Forms;
using NotHotdog.Services;
using NotHotdog.Droid.Services;
using Plugin.CurrentActivity;

namespace NotHotdog.Droid
{
    [Activity(Label = "CFood", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.SetTheme(Resource.Style.MainTheme);

            base.OnCreate(bundle);

			CrossCurrentActivity.Current.Init(this, bundle);
			global::Xamarin.Forms.Forms.Init(this, bundle);

			//DependencyService.Register<DroidHotDogRecognitionService>();

			LoadApplication(new App());
        }

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}

		protected override void OnNewIntent(Android.Content.Intent intent)
		{
			base.OnNewIntent(intent);
            Push.CheckLaunchedFromNotification(this, intent);
		}
    }
}

