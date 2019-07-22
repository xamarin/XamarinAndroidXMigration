using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console
{
	class Program
	{
		static void Main(string[] args)
		{
			var runner = new MigrationRunner();
			runner.MigrateSolution(args[0]);
		}
	}
}
