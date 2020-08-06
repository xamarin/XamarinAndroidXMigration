using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Xunit;

namespace VisualStudio.AndroidX.Migration
{
	public class NameTests : TestBase
	{
		[Fact]
		public void when_defining_fully_qualified_fields_then_replace_namespace()
		{
			var solution = CreateSolution(@"
		using System;

		namespace ClassLibrary1 
		{
			public class Class1 
			{
				Android.Arch.Lifecycle.ViewModelProviders modelProvider;
			}
		}");

			var methodFixer = new TypeRewriter(resolver, new NullProgress());
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("AndroidX.Lifecycle.ViewModelProviders", root);
			Assert.DoesNotContain("Android.Arch.Lifecycle.ViewModelProviders", root);
		}

		[Fact]
		public void when_defining_fully_qualified_properties_then_replace_namespace()
		{
			var file = @"
		using System;

		namespace ClassLibrary1 
		{
			public class Class1 
			{
				Android.Arch.Lifecycle.ViewModelProviders modelProvider { get; set; }
			}
		}";

			var solution = CreateSolution(file);

			var methodFixer = new TypeRewriter(resolver, new NullProgress());
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("AndroidX.Lifecycle.ViewModelProviders", root);
			Assert.DoesNotContain("Android.Arch.Lifecycle.ViewModelProviders", root);
		}

		[Fact]
		public void when_defining_fully_qualified_parameters_then_replace_namespace()
		{
			var solution = CreateSolution(@"
		using System;

		namespace ClassLibrary1 
		{
			public class Class1 
			{
				public void Method(Android.Arch.Lifecycle.ViewModelProviders modelProvider)
				{
				}
			}
		}");

			var methodFixer = new TypeRewriter(resolver, new NullProgress());
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("public void Method(AndroidX.Lifecycle.ViewModelProviders modelProvider)", root);
			Assert.DoesNotContain("public void Method(Android.Arch.Lifecycle.ViewModelProviders modelProvider)", root);
		}

		[Fact]
		public void when_defining_fully_qualified_return_then_replace_namespace()
		{
			var solution = CreateSolution(@"
		using System;

		namespace ClassLibrary1 
		{
			public class Class1 
			{
				public Android.Arch.Lifecycle.ViewModelProviders Method()
				{
					return null;
				}
			}
		}");

			var methodFixer = new TypeRewriter(resolver, new NullProgress());
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("AndroidX.Lifecycle.ViewModelProviders", root);
			Assert.DoesNotContain("Android.Arch.Lifecycle.ViewModelProviders", root);
		}


		[Fact]
		public void when_defining_attribute_then_replace_attribute_name()
		{
			var solution = CreateSolution(@"
		using System;

		namespace ClassLibrary1 
		{
			[Android.Support.V17.Leanback.Widget.Visibility]
			public class Class1 
			{
			}
		}");

			var methodFixer = new TypeRewriter(resolver, new NullProgress());
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("AndroidX.Leanback.Widget.Visibility", root);
			Assert.DoesNotContain("Android.Support.V17.Leanback.Widget.Visibility", root);
		}
	}
}
