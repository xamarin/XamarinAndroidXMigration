using System.IO;

namespace Hackathon
{
	public class Program
	{
		static void Main(string[] args)
		{
			var filename = args.Length > 0 ? args[0] : string.Empty;
			var migrated = args.Length > 1 ? args[1] : Path.ChangeExtension(filename, "migrated.dll");

			var root = Path.GetDirectoryName(typeof(CsvMapping).Assembly.Location);
			var path = Path.Combine(root, "androidx-mapping.csv");

			var migrator = new Migrator(path);
			migrator.Migrate(filename, migrated, true);
		}
	}
}
