using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace VisualStudio.AndroidX.Migration
{
	public class NamespaceRewriter : MigrationRewriter
	{
		private ITranslationResolver assemblyResolver;

		internal override bool UsesSemantic => false;

		public NamespaceRewriter(ITranslationResolver assemblyResolver) : base(assemblyResolver)
		{
			this.assemblyResolver = assemblyResolver;
		}

		public override SyntaxNode VisitUsingDirective(UsingDirectiveSyntax node)
		{
			var name = node.Name.WithoutTrivia().ToFullString();
			if (assemblyResolver.Namespaces.ContainsKey(name))
				return base.VisitUsingDirective(node.WithName(SyntaxFactory.ParseName(assemblyResolver.Namespaces[name])
					.WithLeadingTrivia(node.Name.GetLeadingTrivia())
					.WithTrailingTrivia(node.Name.GetTrailingTrivia())));
			else
				return base.VisitUsingDirective(node);
		}
	}
}
