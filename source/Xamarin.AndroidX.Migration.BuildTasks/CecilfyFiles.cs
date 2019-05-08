using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.AndroidX.Migration.BuildTasks
{
	public class CecilfyFiles : Task
	{
		[Required]
		public ITaskItem[] Assemblies { get; set; }

		public bool Verbose { get; set; }

		public override bool Execute()
		{
			var pairs = new List<MigrationPair>(Assemblies.Length);

			foreach (var file in Assemblies)
			{
				pairs.Add(new MigrationPair(file.ItemSpec, file.ItemSpec));
			}

			var cecilfier = new CecilMigrator
			{
				Verbose = Verbose
			};

			try
			{
				var result = cecilfier.Migrate(pairs);

				Log.LogMessage($"Result of cecilfication: {result}");
			}
			catch (Exception ex)
			{
				Log.LogErrorFromException(ex, true);

				return false;
			}

			return true;
		}
	}
}
