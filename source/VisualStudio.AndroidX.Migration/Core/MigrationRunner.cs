using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using VisualStudio.AndroidX.Migration;
using System.Reflection;

namespace Core
{
	public class MigrationRunner
	{
		private TranslationResolver resolver;
        private IProgress<string> progress;

        public MigrationRunner(IProgress<string> progress)
		{
			var androidDirectory = Path.Combine(Path.GetDirectoryName(typeof(MigrationRunner).Assembly.Location), "Assemblies\\Android");
			var androidXDirectory = Path.Combine(Path.GetDirectoryName(typeof(MigrationRunner).Assembly.Location), "Assemblies\\AndroidX");

			resolver = new TranslationResolver(null, null);
            this.progress = progress;
		}

		public void MigrateSolution(string solutionPath, bool addMigrationNuget = false, bool migrateCode = false)
		{
			var workspace = MSBuildWorkspace.Create();
			workspace.WorkspaceFailed += (s,e) => { Console.WriteLine(e.Diagnostic.Message); };
			var solution = workspace.OpenSolutionAsync(solutionPath).Result;

			var newSolution = solution;

            progress.Report("Migrating Solution");

			foreach (var project in solution.Projects)
			{
                progress.Report(project.Name);
				newSolution = MigrateProject(project, newSolution, addMigrationNuget, migrateCode);
			}

			workspace.TryApplyChanges(newSolution);
		}

		public Solution MigrateProject(Project project, Solution solution, bool addMigrationNuget, bool migrateCode)
		{
			//foreach (var reference in project.MetadataReferences)
			//	if (resolver.Nugets.Keys.Contains(Path.GetFileNameWithoutExtension(reference.Display)) && !resolver.AndroidAssemblies.Any(a => Path.GetFileName(a.CodeBase) == Path.GetFileName(reference.Display)))
			//		resolver.AndroidAssemblies.Add(Assembly.ReflectionOnlyLoadFrom(reference.Display));
			//solution = MigrateDocuments(project, solution);
			MigrateNugets(project, addMigrationNuget);
			//foreach (var reference in project.MetadataReferences)
			//	if (resolver.Nugets.Values.Any(v => v.Key == (Path.GetFileNameWithoutExtension(reference.Display))) && !resolver.AndroidXAssemblies.Any(a => Path.GetFileName(a.CodeBase) == Path.GetFileName(reference.Display)))
			//		resolver.AndroidXAssemblies.Add(Assembly.ReflectionOnlyLoadFrom(reference.Display));
			//solution = PostProcessProject(project, solution);
			Jetify(project);

			return solution;
		}

		private Solution MigrateDocuments(Project project, Solution solution)
		{
			var rewriters = new List<IRewriter>
			{
				new TypeRewriter(resolver, progress),
				new SemanticRewriter(resolver, progress)
			};

			foreach (var document in project.Documents)
			{
				solution = MigrateDocument(document, solution, rewriters);
			}

			return solution;
		}

		private void MigrateNugets(Project project, bool addMigrationNuget)
		{
			var rewriter = new ProjectRewriter(resolver, progress);
			rewriter.RewriteProject(project.FilePath, addMigrationNuget);
		}

		private Solution PostProcessProject(Project project, Solution solution)
		{
			var rewriters = new List<IRewriter>
			{
				new NamespaceRewriter(resolver, progress)
			};

			foreach (var document in project.Documents)
			{
				solution = MigrateDocument(document, solution, rewriters);
			}

			return solution;
		}

		private Solution MigrateDocument(Document document, Solution solution, List<IRewriter> rewriters)
		{
			foreach(var rewriter in rewriters)
			{
				solution = rewriter.Visit(solution, document);
			}

			return solution;
		}

		private void Jetify(Project project)
		{
		}
	}
}
