using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Generators
{
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        public Dictionary<string,SyntaxNode> typesToMock = new Dictionary<string, SyntaxNode>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ObjectCreationExpressionSyntax syntax && 
                syntax.Type is GenericNameSyntax generic &&
                generic.Identifier.ValueText == "Mok" &&
                generic.TypeArgumentList.Arguments.Count == 1)
            {
                var type = generic.TypeArgumentList.Arguments[0];
                typesToMock[type.ToString()] = type;
            }
              
        }
    }
}
