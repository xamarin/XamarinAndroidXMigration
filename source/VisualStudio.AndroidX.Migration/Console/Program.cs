using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudio.AndroidX.Migration
{
	class Program
	{
		static void Main(string[] args)
		{
			var runner = new MigrationRunner(new ConsoleProgress());
			runner.MigrateSolution(args[0]);
		}
	}
}
