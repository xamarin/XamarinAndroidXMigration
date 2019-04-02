using Android.OS;
using Android.Support.V7.App;
using System;

namespace ManagedOnlyLibrary
{
	public class LibraryActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			var actualBase = GetType().BaseType.FullName;
			Console.WriteLine("LibraryActivity.Base => " + actualBase);

			base.OnCreate(savedInstanceState);
		}
	}
}
