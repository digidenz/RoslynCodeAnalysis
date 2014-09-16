using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DiagnosticAndCodeFix
{
	/// <summary>
	///		Discover any instances of a variable having ServiceType.None assigned to it.
	/// </summary>
	[DiagnosticAnalyzer]
	[ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
	public class ServiceTypeAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
	{
		public const string DiagnosticId = "ServiceTypeAnalyzer";
		internal const string Description = "Service type is set to 'None'";
		internal const string MessageFormat = "'{0}' is set to service type 'None'";
		internal const string Category = "ServiceType";

		internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true);

		/// <summary>
		///		Return the supported diagnostic rules.
		/// </summary>
		public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
		{
			get
			{
				return ImmutableArray.Create(Rule);
			}
		}

		/// <summary>
		///		Return the kind of syntax that we are interested in analysing.
		/// </summary>
		public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
		{
			get
			{
				return ImmutableArray.Create(
					SyntaxKind.ExpressionStatement,
					SyntaxKind.LocalDeclarationStatement
				);
			}
		}

		public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
		{
			LocalDeclarationStatementSyntax localDeclaration = node as LocalDeclarationStatementSyntax;
			if (localDeclaration != null)
			{
				foreach (VariableDeclaratorSyntax variableDeclarator in localDeclaration.Declaration.Variables)
				{
					// Check if the variable is initialized with a value.
					if (variableDeclarator.Initializer == null)
						continue;

					EqualsValueClauseSyntax equalsValueClause = variableDeclarator.Initializer as EqualsValueClauseSyntax;
					if (equalsValueClause == null)
						continue;

					// Check if the value being initialized is ServiceType.None.
					if (IsNoneServiceType(equalsValueClause.Value))
						addDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), variableDeclarator.Identifier));
				}
			}

			ExpressionStatementSyntax expression = node as ExpressionStatementSyntax;
			if (expression != null)
			{
				// Check only simple assignments (eg. '=', but not '+=' which makes no sense for ServiceType anyway)
				if (!expression.Expression.IsKind(SyntaxKind.SimpleAssignmentExpression))
					return;
				BinaryExpressionSyntax binaryExpression = expression.Expression as BinaryExpressionSyntax;
				if (binaryExpression == null)
					return;

				// Check if the value being assigned is ServiceType.None.
				if (IsNoneServiceType(binaryExpression.Right))
					addDiagnostic(Diagnostic.Create(Rule, node.GetLocation(), binaryExpression.Left));
			}
		}

		/// <summary>
		///		Check whether the specified node is a MemberAccessExpressionSyntax that represents ServiceType.None.
		/// </summary>
		/// <param name="node"></param>
		/// <returns></returns>
		private bool IsNoneServiceType(SyntaxNode node)
		{
			var memberAccess = node as MemberAccessExpressionSyntax;
			if (memberAccess == null)
				return false;

			// TODO: Need to find a better way to compare "ServiceType" and "None".
			if (memberAccess.Expression.ToString() == "ServiceType" && memberAccess.Name.ToString() == "None")
			{
				return true;
			}

			return false;
		}
	}
}