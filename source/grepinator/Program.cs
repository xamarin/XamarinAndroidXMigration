using Mono.Options;

namespace grepinator
{
	class Program
	{
		public const string Name = "grepinator";

		public static bool Verbose;

		static int Main(string[] args)
		{
			var commands = new CommandSet(Name)
			{
				$"usage: {Name} COMMAND [OPTIONS]",
				"",
				"A utility that helps locate Java classes.",
				"",
				"Global options:",
				{ "v|verbose", "Use a more verbose output", _ => Verbose = true },
				"",
				"Available commands:",
				new SearchCommand(),
			};
			return commands.Run(args);
		}
	}
}
