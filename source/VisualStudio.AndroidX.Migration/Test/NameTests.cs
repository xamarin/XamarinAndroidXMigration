using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VisualStudio.AndroidX.Migration
{
	[TestClass]
	public class NameTests : TestBase
	{
		[TestMethod]
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

			var methodFixer = new TypeRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.IsTrue(root.Contains("AndroidX.Lifecycle.ViewModelProviders"));
			Assert.IsFalse(root.Contains("Android.Arch.Lifecycle.ViewModelProviders"));
		}

		[TestMethod]
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

			var methodFixer = new TypeRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.IsTrue(root.Contains("AndroidX.Lifecycle.ViewModelProviders"));
			Assert.IsFalse(root.Contains("Android.Arch.Lifecycle.ViewModelProviders"));
		}

		[TestMethod]
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

			var methodFixer = new TypeRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.IsTrue(root.Contains("public void Method(AndroidX.Lifecycle.ViewModelProviders modelProvider)"));
			Assert.IsFalse(root.Contains("public void Method(Android.Arch.Lifecycle.ViewModelProviders modelProvider)"));
		}

		[TestMethod]
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

			var methodFixer = new TypeRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.IsTrue(root.Contains("AndroidX.Lifecycle.ViewModelProviders"));
			Assert.IsFalse(root.Contains("Android.Arch.Lifecycle.ViewModelProviders"));
		}


		[TestMethod]
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

			var methodFixer = new TypeRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.IsTrue(root.Contains("AndroidX.Leanback.Widget.Visibility"));
			Assert.IsFalse(root.Contains("Android.Support.V17.Leanback.Widget.Visibility"));
		}
	}
}
