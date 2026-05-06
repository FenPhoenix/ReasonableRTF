using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static FenGen.Misc;

namespace FenGen;

internal static class ParseKeywordGen
{
    internal static void Generate(string sourceFile, List<string> destFiles)
    {
        SyntaxNode[] nodes = GetNodes(sourceFile);

        (MemberDeclarationSyntax member, AttributeSyntax attr) = GetAttrMarkedItem(
            nodes,
            SyntaxKind.MethodDeclaration,
            GenAttributes.FenGen_ParseKeywordAttribute);

        const int reqArgCount = 3;

        if (attr.ArgumentList is not { Arguments.Count: reqArgCount })
        {
            ThrowErrorAndTerminate(nameof(GenAttributes.FenGen_ParseKeywordAttribute) + " had wrong number of args (should be " + reqArgCount + ")");
            return;
        }

        string getByteFunctionName = GetStringParamValue(attr, 0);
        string bufferName = GetStringParamValue(attr, 1);
        string incrementFunctionName = GetStringParamValue(attr, 2);

        if (member is not MethodDeclarationSyntax method)
        {
            ThrowErrorAndTerminate("Attribute " + GenAttributes.FenGen_ParseKeywordAttribute + " was not on a function");
            return;
        }

        TextLineCollection methodLines = method.GetText().Lines;
        List<string> sourceLines = new();
        bool inSourceLinesSection = false;
        for (int i = 0; i < methodLines.Count; i++)
        {
            TextLine line = methodLines[i];
            string lineStr = line.ToString();

            if (inSourceLinesSection)
            {
                if (IsFenGenNotationLine(lineStr, "[FenGen:ScalarKeywordParseSection:Source:End]"))
                {
                    break;
                }
                else
                {
                    sourceLines.Add(lineStr);
                }
            }
            else if (IsFenGenNotationLine(lineStr, "[FenGen:ScalarKeywordParseSection:Source:Begin]"))
            {
                inSourceLinesSection = true;
            }
        }

        foreach (string destFile in destFiles)
        {
            List<string> destLines = File.ReadAllLines(destFile).ToList();
            for (int i = 0; i < destLines.Count; i++)
            {
                string line = destLines[i];
                if (IsFenGenNotationLine(line, "[FenGen:ScalarKeywordParseSection:Slow:Dest:Begin]"))
                {
                    CopyLines(sourceLines, destLines, destFile, i, "Slow", getByteFunctionName, incrementFunctionName, bufferName);
                    break;
                }
                else if (IsFenGenNotationLine(line, "[FenGen:ScalarKeywordParseSection:Fast:Dest:Begin]"))
                {
                    CopyLines(sourceLines, destLines, destFile, i, "Fast", getByteFunctionName, incrementFunctionName, bufferName);
                    break;
                }
            }
        }
    }

    private static void CopyLines(
        List<string> sourceLines,
        List<string> destLines,
        string destFile,
        int i,
        string version,
        string getByteFunctionName,
        string incrementFunctionName,
        string bufferName)
    {
        for (int subI = i + 1; subI < destLines.Count; subI++)
        {
            string subLine = destLines[subI];
            if (IsFenGenNotationLine(subLine, "[FenGen:ScalarKeywordParseSection:" + version + ":Dest:End]"))
            {
                for (int copyI = sourceLines.Count - 1; copyI >= 0; copyI--)
                {
                    string sourceLine = sourceLines[copyI];
                    if (version == "Fast")
                    {
                        sourceLine = sourceLine.Replace(
                            getByteFunctionName + "(" + incrementFunctionName + "())",
                            bufferName + "[" + incrementFunctionName + "()]"
                        );
                    }
                    destLines.Insert(subI, sourceLine);
                }

                File.WriteAllLines(destFile, destLines);

                return;
            }
            else
            {
                destLines.RemoveAt(subI);
                subI--;
            }
        }
    }

    private static bool IsFenGenNotationLine(string line, string value)
    {
        string lineT = line.Trim();
        return lineT.StartsWithO("//") && lineT.TrimStart('/').TrimStart(' ') == value;
    }
}
