using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static FenGen.Misc;

namespace FenGen;

internal static class ParseKeywordGen
{
    internal static void Generate(string sourceFile, string destFile)
    {
        SyntaxNode[] nodes = GetNodes(sourceFile);

        (MemberDeclarationSyntax member, AttributeSyntax attr) = GetAttrMarkedItem(
            nodes,
            SyntaxKind.MethodDeclaration,
            GenAttributes.FenGen_ParseKeywordAttribute);

        const int reqArgCount = 4;

        if (attr.ArgumentList is not { Arguments.Count: reqArgCount })
        {
            ThrowErrorAndTerminate(nameof(GenAttributes.FenGen_ParseKeywordAttribute) + " had wrong number of args (should be " + reqArgCount + ")");
            return;
        }

        string getByteFunctionName = GetStringParamValue(attr, 0);
        string bufferName = GetStringParamValue(attr, 1);
        string incrementFunctionName = GetStringParamValue(attr, 2);
        string destFunctionName = GetStringParamValue(attr, 3);

        if (member is not MethodDeclarationSyntax method)
        {
            ThrowErrorAndTerminate("Attribute " + GenAttributes.FenGen_ParseKeywordAttribute + " was not on a function");
            return;
        }

        List<string> usingLines = new();
        foreach (SyntaxNode node in nodes)
        {
            if (node is UsingDirectiveSyntax uds)
            {
                usingLines.Add(uds.ToString());
            }
            else if (node is BaseNamespaceDeclarationSyntax)
            {
                break;
            }
        }

        method = method.RemoveNode(attr.Parent!, SyntaxRemoveOptions.KeepNoTrivia)!;

        TextSpan nameSpan = method.Identifier.Span;

        string text = method.GetText().ToString();
        text = text.Substring(0, nameSpan.Start) + destFunctionName + text.Substring(nameSpan.End);

        text = text.Replace(
            getByteFunctionName + "(" + incrementFunctionName + "())",
            bufferName + "[" + incrementFunctionName + "()]"
        );

        text =
            "// Generated version that doesn't do manual bounds checking, for when we know we're far enough from the end of the buffer" +
            Environment.NewLine +
            text;

        WriteDestFile(destFile, text, usingLines: usingLines);
    }
}
