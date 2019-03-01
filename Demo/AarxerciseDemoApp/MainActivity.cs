using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;

using Com.Xamarin.Aarxercise;
using ManagedAarxercise;

namespace AarxerciseDemoApp
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity
	{
		private ManagedAarxerciser managed;
		private Aarxerciser native;

		private ManagedSimpleFragment managedFragment;
		private SimpleFragment nativeFragment;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			Android.Support.V7.Widget.Toolbar toolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
			fab.Click += FabOnClick;

			managed = new ManagedAarxerciser();
			native = new Aarxerciser();

			managedFragment = managed.CreateSimpleFragment(this);
			nativeFragment = native.CreateSimpleFragment(this);

			base.SupportFragmentManager.BeginTransaction()
				.Replace(Resource.Id.topFrame, managedFragment)
				.Replace(Resource.Id.bottomFrame, nativeFragment)
				.Commit();
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			int id = item.ItemId;
			if (id == Resource.Id.action_settings)
			{
				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		private void FabOnClick(object sender, EventArgs eventArgs)
		{
			native.UpdateSimpleFragment(nativeFragment, "Java Fragment");
			managed.UpdateSimpleFragment(managedFragment, "C# Fragment");
		}
	}
}
