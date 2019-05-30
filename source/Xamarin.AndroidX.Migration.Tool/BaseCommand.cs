using Mono.Options;
using System;
using System.Collections.Generic;

namespace AndroidXMigrator
{
	public abstract class BaseCommand : Command
	{
		protected BaseCommand(string name, string help)
			: base(name, help)
		{
			var actualOptions = OnCreateOptions();

			Options = new OptionSet
			{
				$"usage: {Program.Name} {Name} [OPTIONS]",
				"",
				Help,
				"",
			};

			foreach (var o in actualOptions)
				Options.Add(o);

			Options.Add("?|h|help", "Show this message and exit", _ => ShowHelp = true);
		}

		public bool ShowHelp { get; private set; }

		public override int Invoke(IEnumerable<string> args)
		{
			try
			{
				var extras = Options.Parse(args);

				if (ShowHelp)
				{
					Options.WriteOptionDescriptions(CommandSet.Out);
					return 0;
				}

				if (!OnValidateArguments())
				{
					Console.Error.WriteLine($"{Program.Name}: Use `{Program.Name} help {Name}` for details.");
					return 1;
				}

				OnInvoke(extras);

				return 0;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"{Program.Name}: An error occurred: `{ex.Message}`.");
				if (Program.Verbose)
					Console.Error.WriteLine(ex);
				return 1;
			}
		}

		protected abstract OptionSet OnCreateOptions();

		protected virtual bool OnValidateArguments() => true;

		protected abstract void OnInvoke(IEnumerable<string> extras);
	}
}
