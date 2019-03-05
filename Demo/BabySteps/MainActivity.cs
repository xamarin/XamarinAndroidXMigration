using Android.App;
using Android.OS;
using System;

namespace BabySteps
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
	public class MainActivity : Android.Support.V7.App.AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			var actualBase = GetType().BaseType.FullName;
			Console.WriteLine("FullName => " + actualBase);

			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.activity_main);
		}

		protected override void OnResume()
		{
			base.OnResume();
		}
	}
}
