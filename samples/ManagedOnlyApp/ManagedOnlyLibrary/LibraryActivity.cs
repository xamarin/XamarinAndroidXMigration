using System;
using Android.OS;
using Android.Support.V7.App;

namespace ManagedOnlyLibrary
{
	public class LibraryActivity : AppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			var actualBase = GetType().BaseType.FullName;
			Console.WriteLine($"\n*** LibraryActivity.Base => {actualBase}\n");

			base.OnCreate(savedInstanceState);
		}
	}
}
