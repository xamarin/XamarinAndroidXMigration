using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace VisualStudio.AndroidX.Migration
{
	public class SemanticRewriter : MigrationRewriter
	{
		private ITranslationResolver assemblyResolver;

		internal override bool UsesSemantic => true;

		public SemanticRewriter(ITranslationResolver assemblyResolver, IProgress<string> progress) : base(assemblyResolver, progress)
		{
			this.assemblyResolver = assemblyResolver;
		}

		public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
		{
			var symbol = Semantic.GetSymbolInfo(node);
			if (symbol.Symbol != null && node.ToFullString() != symbol.Symbol.ToDisplayString())
				return SyntaxFactory.ParseName(symbol.Symbol.ToDisplayString())
					.WithLeadingTrivia(node.GetLeadingTrivia())
					.WithTrailingTrivia(node.GetTrailingTrivia());
			else
				return base.VisitQualifiedName(node);
		}
	}
}
