using Microsoft.CodeAnalysis;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudio.AndroidX.Migration
{
	public class NamespaceTests : TestBase
	{
		[Fact]
		public void when_defining_namespaces_then_replace_namespace()
		{
			var solution = CreateSolution(@"
		using System;
		using Android.Arch.Lifecycle;

		namespace ClassLibrary1 
		{
			public class Class1 
			{
			}
		}");

			var methodFixer = new NamespaceRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("using AndroidX.Lifecycle;", root);
			Assert.DoesNotContain("using Android.Arch.Lifecycle;", root);
		}

		[Fact]
		public void when_defining_aliased_namespaces_then_replace_namespace()
		{
			var solution = CreateSolution(@"
		using System;
		using life = Android.Arch.Lifecycle;

		namespace ClassLibrary1 
		{
			public class Class1 
			{
			}
		}");

			var methodFixer = new NamespaceRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains("using life = AndroidX.Lifecycle;", root);
			Assert.DoesNotContain("using life = Android.Arch.Lifecycle;", root);
		}

		[Fact]
		public void when_defining_aliased_namespaces_then_fully_qualify_if_needed()
		{
			var solution = CreateSolution(@"
		using System;
		using support = Android.Support;

		namespace ClassLibrary1 
		{
			public class MainActivity : support.V7.App.AppCompatActivity 
			{
			}
		}");

			var methodFixer = new SemanticRewriter(resolver);
			var typeFixer = new TypeRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));
			solution = typeFixer.Visit(solution, GetFirstDocument(solution));

			var root = GetText(solution);

			Assert.Contains(": AndroidX.AppCompat.App.AppCompatActivity", root);
			Assert.DoesNotContain(": support.V7.App.AppCompatActivity", root);
		}

		[Fact]
		public void when_defining_types_then_dont_break_fully_qualified()
		{
			var solution = CreateSolution(@"
		using System;

		namespace ClassLibrary1 
		{
			public class MainActivity : Android.Support.V7.App.AppCompatActivity 
			{
			}
		}");

			var methodFixer = new SemanticRewriter(resolver);
			solution = methodFixer.Visit(solution, GetFirstDocument(solution));
			
			var root = GetText(solution);

			Assert.Contains(": Android.Support.V7.App.AppCompatActivity", root);
		}
	}
}
