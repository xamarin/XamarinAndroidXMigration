using System.IO;

namespace Xamarin.AndroidX.Migration.Cecil
{
	public class Program
	{
		static void Main(string[] args)
		{
			var filename = args.Length > 0 ? args[0] : string.Empty;
			var migrated = args.Length > 1 ? args[1] : Path.ChangeExtension(filename, "migrated.dll");

			var migrator = new Migrator();
			migrator.Migrate(filename, migrated, true);
		}
	}
}
