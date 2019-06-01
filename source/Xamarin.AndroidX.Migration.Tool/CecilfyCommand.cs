﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Options;
using Xamarin.AndroidX.Migration;

namespace AndroidXMigrator
{
	public class CecilfyCommand : BaseCommand
	{
		private bool skipEmbeddedResources;
		private bool renameTypes;

		public CecilfyCommand()
			: base("cecilfy", "Migrates a .NET assembly to AndroidX.")
		{
		}

		public Dictionary<string, string> Assemblies { get; } = new Dictionary<string, string>();

		protected override OptionSet OnCreateOptions() => new OptionSet
		{
			{ "a|assembly=", "One or more assemblies to cecilfy", v => AddAssembly(v) },
			{ "skipEmbedded", "Do not Jetify the embedded resources", v => skipEmbeddedResources = true },
			{ "renameTypes", "Rename the types inside the assembly (INTERNAL)", v => renameTypes = true },
		};

		protected override bool OnValidateArguments(IEnumerable<string> extras)
		{
			var hasError = false;

			if (Assemblies.Count == 0)
			{
				Console.Error.WriteLine($"{Program.Name}: At least one assembly is required `--assembly=PATH` or `--assembly=PATH|OUTPUT` or `--assembly=PATH=OUTPUT`.");
				hasError = true;
			}

			var missing = Assemblies.Keys.Where(i => !File.Exists(i));
			if (missing.Any())
			{
				foreach (var file in missing)
					Console.Error.WriteLine($"{Program.Name}: File does not exist: `{file}`.");
				hasError = true;
			}

			return !hasError;
		}

		protected override void OnInvoke(IEnumerable<string> extras)
		{
			var assemblyPairs = Assemblies.Select(a => new MigrationPair(a.Key, a.Value)).ToArray();

			var migrator = new CecilMigrator();
			migrator.Verbose = Program.Verbose;
			migrator.SkipEmbeddedResources = skipEmbeddedResources;
			migrator.RenameTypes = renameTypes;
			migrator.Migrate(assemblyPairs);
		}

		private void AddAssembly(string assembly)
		{
			if (string.IsNullOrWhiteSpace(assembly))
				return;

			var parts = assembly.Split('|', '=');

			if (parts.Length < 1)
				return;

			var input = parts[0];
			var output = parts.Length == 1 ? input : parts[1];

			Assemblies.Add(input, output);
		}
	}
}
