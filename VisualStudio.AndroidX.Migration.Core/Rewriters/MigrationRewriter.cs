using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Text;

namespace VisualStudio.AndroidX.Migration
{
	public abstract class MigrationRewriter : CSharpSyntaxRewriter, IRewriter
	{
		public SemanticModel Semantic { get; set; }
		protected ITranslationResolver resolver;
        private IProgress<string> progress;

        internal abstract bool UsesSemantic { get; }

		public MigrationRewriter(ITranslationResolver resolver, IProgress<string> progress)
		{
			this.resolver = resolver;
            this.progress = progress;
		}

		public Solution Visit(Solution solution, Document document)
		{
			if (UsesSemantic)
				this.Semantic = document.Project.GetCompilationAsync().Result.GetSemanticModel(document.GetSyntaxRootAsync().Result.SyntaxTree);
			var root = this.Visit(document.GetSyntaxRootAsync().Result);
			return solution.WithDocumentSyntaxRoot(document.Id, root);
		}

		internal bool TryGetValue(string name, out string typeName)
		{
			typeName = string.Empty;
			if (resolver.FullTypeNames.ContainsKey(name))
			{
				typeName = resolver.FullTypeNames[name];
				return true;
			}

			return false;
		}

	}

	public interface IRewriter
	{
		Solution Visit(Solution solution, Document document);
	}
}
