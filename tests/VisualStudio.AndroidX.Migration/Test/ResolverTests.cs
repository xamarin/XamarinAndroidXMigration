using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace VisualStudio.AndroidX.Migration
{
	public class ResolverTests
	{
		[Fact]
		public void when_translation_resolver_doesnt_have_assemblies_it_still_initializes()
		{
			var resolver = new TranslationResolver(null, null);
		}
	}
}
