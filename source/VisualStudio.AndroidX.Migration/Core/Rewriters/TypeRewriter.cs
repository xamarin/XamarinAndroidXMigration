using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VisualStudio.AndroidX.Migration
{
	public class TypeRewriter : MigrationRewriter
	{
		private ITranslationResolver assemblyResolver;

		internal override bool UsesSemantic => false;

		public TypeRewriter(ITranslationResolver assemblyResolver, IProgress<string> progress) : base(assemblyResolver, progress)
		{
			this.assemblyResolver = assemblyResolver;
		}

		public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
		{
			var name = node.WithoutTrivia().ToFullString();
			if (TryGetValue(name, out var typeName))
				return SyntaxFactory.ParseName(typeName)
					.WithLeadingTrivia(node.GetLeadingTrivia())
					.WithTrailingTrivia(node.GetTrailingTrivia());
			else
				return base.VisitQualifiedName(node);
		}

		public override SyntaxNode VisitAttribute(AttributeSyntax node)
		{
			var typeName = string.Empty;
			var identifier = node.Name + "Attribute";
			if (TryGetValue(identifier, out typeName))
			{
				if (typeName.EndsWith("Attribute"))
				{
					typeName = typeName.Remove(typeName.Length - 9);
					return node.WithName(SyntaxFactory.ParseName(typeName)
						.WithLeadingTrivia(node.GetLeadingTrivia())
						.WithTrailingTrivia(node.GetTrailingTrivia()));
				}
			}
			return base.VisitAttribute(node);
		}
	}
}
