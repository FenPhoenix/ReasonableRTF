using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using FenGen.Forms;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FenGen;

internal static partial class Misc
{
    private static readonly string[] SA_Linebreaks = new[] { "\r\n", "\r", "\n" };

    private static readonly CSharpParseOptions _parseOptions = new(
        languageVersion: LanguageVersion.Latest,
        documentationMode: DocumentationMode.None,
        SourceCodeKind.Regular);

    // Try to make Roslyn be as non-slow as possible. It's still going to be slow, but hey...
    private static SyntaxTree ParseTextFast(string text) => CSharpSyntaxTree.ParseText(text, _parseOptions);

    internal static SyntaxNode[] GetNodes(string file)
    {
        return ParseTextFast(File.ReadAllText(file)).GetRoot().DescendantNodesAndSelf().ToArray();
    }

    internal static (SyntaxNode[] Nodes, string Text) GetNodesAndText(string file)
    {
        string text = File.ReadAllText(file);
        SyntaxNode[] nodes = ParseTextFast(text).GetRoot().DescendantNodesAndSelf().ToArray();

        return (nodes, text);
    }

    private static (string CodeBlock, bool FileScopedNamespace)
    GetCodeBlock(string file, string genAttr)
    {
        bool fileScopedNamespace = false;

        (SyntaxNode[] nodes, string code) = GetNodesAndText(file);
        foreach (SyntaxNode node in nodes)
        {
            if (node is BaseNamespaceDeclarationSyntax)
            {
                fileScopedNamespace = node is FileScopedNamespaceDeclarationSyntax;
                break;
            }
        }

        ClassDeclarationSyntax targetClass;
        if (!genAttr.IsEmpty())
        {
            (MemberDeclarationSyntax member, _) = GetAttrMarkedItem(nodes, SyntaxKind.ClassDeclaration, genAttr);
            targetClass = (ClassDeclarationSyntax)member;
        }
        else
        {
            SyntaxNode? destClassNode = nodes.FirstOrDefault(static x => x.IsKind(SyntaxKind.ClassDeclaration));
            if (destClassNode is not ClassDeclarationSyntax destClass)
            {
                ThrowErrorAndTerminate("Class not found");
                return ("", false);
            }
            targetClass = destClass;
        }

        string targetClassString = targetClass.ToString();
        string classDeclLine = targetClassString.Substring(0, targetClassString.IndexOf('{'));

        code = code
            .Substring(0, targetClass.GetLocation().SourceSpan.Start + classDeclLine.Length)
            .TrimEnd() + "\r\n";

        return (code, fileScopedNamespace);
    }

    private static CodeWriters.IndentingWriter GetWriterForClass(string destFile, string classAttribute, List<string>? usingLines = null)
    {
        (string codeBlock, bool fileScopedNamespace) = GetCodeBlock(destFile, classAttribute);
        CodeWriters.IndentingWriter w = new(startingIndent: fileScopedNamespace ? 0 : 1, fileScopedNamespace);

        List<string> lines = codeBlock.Split(SA_Linebreaks, StringSplitOptions.None).ToList();

        for (int i = 0; i < lines.Count; i++)
        {
            string line = lines[i];
            string lineT = line.Trim();
            if ((usingLines != null && lineT.StartsWithOPlusWhiteSpace("using")) || lineT.StartsWithO("#nullable"))
            {
                lines.RemoveAt(i);
                i--;
            }
        }

        if (usingLines != null)
        {
            int firstUsingLine = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                string lineT = line.Trim();
                if (lineT.StartsWithOPlusWhiteSpace("namespace"))
                {
                    firstUsingLine = Math.Max(0, i - 1);
                    break;
                }
            }

            if (firstUsingLine > -1 && lines.Count > firstUsingLine)
            {
                for (int i = usingLines.Count - 1; i >= 0; i--)
                {
                    lines.Insert(firstUsingLine, usingLines[i]);
                }
            }

            if (firstUsingLine > 0 && !lines[firstUsingLine - 1].IsWhiteSpace())
            {
                lines.Insert(firstUsingLine, "");
            }
        }

        lines.Insert(0, "#nullable enable // Required for generated files");

        codeBlock = string.Join(Environment.NewLine, lines);

        w.AppendRawString(codeBlock);
        w.WL("{");
        return w;
    }

    internal static (MemberDeclarationSyntax Member, AttributeSyntax Attribute)
    GetAttrMarkedItem(SyntaxNode[] nodes, SyntaxKind syntaxKind, string attrName)
    {
        List<MemberDeclarationSyntax> attrMarkedItems = new();
        AttributeSyntax? retAttr = null;

        foreach (SyntaxNode n in nodes)
        {
            if (!n.IsKind(syntaxKind)) continue;

            MemberDeclarationSyntax item = (MemberDeclarationSyntax)n;
            foreach (AttributeListSyntax attrList in item.AttributeLists)
            {
                foreach (AttributeSyntax attr in attrList.Attributes)
                {
                    if (GetAttributeName(attr.Name.ToString(), attrName))
                    {
                        attrMarkedItems.Add(item);
                        retAttr = attr;
                    }
                }
            }
        }

        if (attrMarkedItems.Count > 1)
        {
            ThrowErrorAndTerminate("Multiple uses of attribute '" + attrName + "'.");
        }
        else if (attrMarkedItems.Count == 0)
        {
            ThrowErrorAndTerminate("No uses of attribute '" + attrName + "'.");
        }

        return (attrMarkedItems[0], retAttr!);
    }

    /// <summary>
    /// Matches an attribute, ignoring the "Attribute" suffix if it exists in either string, and also ignoring
    /// namespace/class prefixes.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="match"></param>
    /// <returns></returns>
    private static bool GetAttributeName(string name, string match)
    {
        int index;
        while ((index = name.IndexOf('.')) > -1)
        {
            name = name.Substring(index + 1);
        }
        while ((index = match.IndexOf('.')) > -1)
        {
            match = match.Substring(index + 1);
        }

        // We have to handle this quirk where you can leave off the "Attribute" suffix - Roslyn won't handle it
        // for us
        const string attributeSuffix = "Attribute";

        if (match.EndsWithO(attributeSuffix))
        {
            match = match.Substring(0, match.LastIndexOf(attributeSuffix, StringComparison.Ordinal));
        }
        if (name.EndsWithO(attributeSuffix))
        {
            name = name.Substring(0, name.LastIndexOf(attributeSuffix, StringComparison.Ordinal));
        }

        return name == match;
    }

    internal static string GetStringParamValue(AttributeSyntax attrSyntax, int argIndex)
    {
        ExpressionSyntax arg0Exp = attrSyntax.ArgumentList!.Arguments[argIndex].Expression;

        if (arg0Exp is InvocationExpressionSyntax ies &&
            ies.ChildNodes().First() is IdentifierNameSyntax ins)
        {
            if (ins.Identifier.ToString() == "nameof")
            {
                SyntaxNode? argSyntax = ies.ChildNodes().FirstOrDefault(static x => x is ArgumentListSyntax);
                if (argSyntax is ArgumentListSyntax { Arguments.Count: 1 } als)
                {
                    return als.Arguments[0].Expression.ToString();
                }
            }
        }

        return ((LiteralExpressionSyntax)arg0Exp).Token.ValueText;
    }

    internal static void WriteDestFile(string destFile, string text, string classAttribute = "", List<string>? usingLines = null)
    {
        CodeWriters.IndentingWriter w = GetWriterForClass(destFile, classAttribute, usingLines);
        w.WL(text.TrimEnd());
        w.CloseClassAndNamespace();
        File.WriteAllText(destFile, w.ToString());
    }

    #region Throw and terminate

    // Needed because on Framework, Environment.Exit() is not marked with a [DoesNotReturn] attribute itself.
#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.
    [DoesNotReturn]
    internal static void ThrowErrorAndTerminate(string message)
    {
        Trace.WriteLine("FenGen: " + message + "\r\nTerminating FenGen.");
        MessageBox.Show(message + "\r\n\r\nExiting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        Environment.Exit(-999);
    }

    [DoesNotReturn]
    internal static void ThrowErrorAndTerminate(Exception ex)
    {
        Trace.WriteLine("FenGen: " + ex + "\r\nTerminating FenGen.");
        using (ExceptionBox f = new(ex.ToString())) f.ShowDialog();
        Environment.Exit(-999);
    }
#pragma warning restore CS8763

    #endregion
}
