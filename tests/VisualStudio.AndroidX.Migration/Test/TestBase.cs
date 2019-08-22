using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisualStudio.AndroidX.Migration
{
	public class TestBase
	{
		// the xUnit runner needs this otherwise it loses the assembly when running on CI
		public static readonly Microsoft.CodeAnalysis.CSharp.LanguageVersion DummyType;

		internal static ITranslationResolver resolver;

		static TestBase()
		{
			var codeBase = new Uri(typeof(TestBase).Assembly.CodeBase).AbsolutePath;
			var binDirectory = Path.GetDirectoryName(codeBase);

			var assembliesDirectory = Path.Combine(binDirectory, "Assemblies", "Android");

			resolver = new TranslationResolver(Directory.GetFiles(assembliesDirectory), new List<string> { });
		}


		public static Solution CreateSolution(string file)
		{
			var workspace = new AdhocWorkspace();
			var project = workspace.AddProject("MethodTest", "C#");
			foreach (var assembly in resolver.AndroidAssemblies)
				project = project.AddMetadataReference(MetadataReference.CreateFromFile(assembly.Location));
			var document = project.AddDocument("Class1.cs", file);
			return document.Project.Solution;
		}

		internal static string GetText(Solution solution)
		{
			return solution.Projects.First().Documents.First().GetSyntaxRootAsync().Result.ToFullString();
		}

		internal static Document GetFirstDocument(Solution solution)
		{
			return solution.Projects.First().Documents.First();
		}
	}
}