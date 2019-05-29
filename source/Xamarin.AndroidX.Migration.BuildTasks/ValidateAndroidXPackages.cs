using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Xamarin.AndroidX.Migration.BuildTasks
{
	public class ValidateAndroidXPackages : Task
	{
		[Required]
		public ITaskItem[] ResolvedAssemblies { get; set; }

		public bool Verbose { get; set; }

		public override bool Execute()
		{
			// if there are no assemblies, then we are done
			if (ResolvedAssemblies == null || ResolvedAssemblies.Length == 0)
			{
				Log.LogMessage($"There were no assembies to ckeck.");
				return true;
			}

			var assemblyNames = ResolvedAssemblies.Select(a => Path.GetFileNameWithoutExtension(a.ItemSpec));
			var orderedNames = new SortedSet<string>(assemblyNames);

			var mapping = new AndroidXAssembliesCsvMapping();

			bool hasError = false;

			foreach (var assembly in assemblyNames)
			{
				// if there was no mapping found, then we don't care
				if (!mapping.TryGetAndroidXAssembly(assembly, out var xAssembly))
					continue;

				if (Verbose)
					Log.LogMessage($"Making sure that the Android Support assembly '{assembly}' has a replacement Android X assembly...");

				// make sure the mapped assembly is referenced
				if (orderedNames.Contains(xAssembly))
				{
					if (Verbose)
						Log.LogMessage($"Correctly replacing the Android Support assembly '{assembly}' with Android X '{xAssembly}'.");

					continue;
				}

				// the mapped assembly was not found, so this is an error
				hasError = true;
				mapping.TryGetAndroidXPackage(assembly, out var package);
				Log.LogError($"Could not find the Android X replacement assembly '{xAssembly}' for '{assembly}'. Make sure the Android X NuGet package '{package}' is installed.");
			}

			return !hasError;
		}
	}
}
