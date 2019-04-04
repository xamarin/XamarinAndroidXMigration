using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace FormsDemo.Droid
{
	[Activity(Label = "FormsDemo", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			Console.WriteLine($"\n*** typeof(Android.Support.V7.App.AppCompatActivity) => {typeof(Android.Support.V7.App.AppCompatActivity)}\n");

			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			Forms.Init(this, savedInstanceState);
			LoadApplication(new App());
		}
	}
}
