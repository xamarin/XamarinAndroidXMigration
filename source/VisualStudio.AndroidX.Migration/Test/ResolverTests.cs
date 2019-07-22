using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudio.AndroidX.Migration
{
	[TestClass]
	public class ResolverTests
	{
		[TestMethod]
		public void when_translation_resolver_doesnt_have_assemblies_it_still_initializes()
		{
			var resolver = new TranslationResolver(null, null);
		}
	}
}
