using System;
using Android.OS;
using AndroidX.AppCompat.App;

namespace AndroidXOnlyLibrary
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
