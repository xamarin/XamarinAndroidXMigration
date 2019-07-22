using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisualStudio.AndroidX.Migration
{
	public class TestBase
	{
		internal static ITranslationResolver resolver;

		static TestBase()
		{
			var assembliesDirectory = Path.Combine(Path.GetDirectoryName(typeof(NameTests).Assembly.Location), "Assemblies\\Android");

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