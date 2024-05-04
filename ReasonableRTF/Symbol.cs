using System.Runtime.CompilerServices;

namespace ReasonableRTF;

internal sealed class Symbol
{
    internal readonly string Keyword;
    internal readonly int DefaultParam;
    internal readonly bool UseDefaultParam;
    internal readonly Enums.KeywordType KeywordType;
    /// <summary>
    /// Index into the property table, or a regular enum member, or a character literal, depending on <see cref="KeywordType"/>.
    /// </summary>
    internal readonly ushort Index;

    internal Symbol(string keyword, int defaultParam, bool useDefaultParam, Enums.KeywordType keywordType, ushort index)
    {
        Keyword = keyword;
        DefaultParam = defaultParam;
        UseDefaultParam = useDefaultParam;
        KeywordType = keywordType;
        Index = index;
    }
}

internal sealed class SymbolDict
{
    /* ANSI-C code produced by gperf version 3.1 */
    /* Command-line: 'C:\\gperf\\tools\\gperf.exe' --output-file='C:\\_al_rtf_table_gen\\gperfOutputFile.txt' -t 'C:\\_al_rtf_table_gen\\gperfFormatFile.txt'  */
    /* Computed positions: -k'1-3,$' */

    //private const int TOTAL_KEYWORDS = 77;
    //private const int MIN_WORD_LENGTH = 1;
    private const int MAX_WORD_LENGTH = 18;
    //private const int MIN_HASH_VALUE = 1;
    private const int MAX_HASH_VALUE = 278;
    /* maximum key range = 278, duplicates = 0 */

    private readonly ushort[] asso_values =
    [
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 0, 60, 0,
        70, 0, 10, 115, 60, 0, 5, 0, 0, 100,
        25, 0, 45, 5, 35, 5, 20, 0, 15, 0,
        5, 0, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279, 279, 279, 279, 279,
        279, 279, 279, 279, 279, 279,
    ];

    // For "emspace", "enspace", "qmspace", "~"
    // Just convert these to regular spaces because we're just trying to scan for strings in readmes
    // without weird crap tripping us up
    // emspace  '\x2003'
    // enspace  '\x2002'
    // qmspace  '\x2005'
    // ~        '\xa0'

    // For "emdash", "endash", "lquote", "rquote", "ldblquote", "rdblquote"
    // NOTE: Maybe just convert these all to ASCII equivalents as well?

    // For "cs", "ds", "ts"
    // Hack to make sure we extract the \fldrslt text from Thief Trinity in that one place.

    // For "listtext", "pntext"
    // Ignore list item bullets and numeric prefixes etc. We don't need them.

    // For "v"
    // \v to make all plain text hidden (not output to the conversion stream), \v0 to make it shown again

    // For "ansi"
    // The spec calls this "ANSI (the default)" but says nothing about what codepage that actually means.
    // "ANSI" is often misused to mean one of the Windows codepages, so I'll assume it's Windows-1252.

    // For "mac"
    // The spec calls this "Apple Macintosh" but again says nothing about what codepage that is. I'll
    // assume 10000 ("Mac Roman")

    // For "fldinst"
    // We need to do stuff with this (SYMBOL instruction)

    // NOTE: This is generated. Values can be modified, but not keys (keys are the first string params).
    // Also no reordering. Adding, removing, reordering, or modifying keys requires generating a new version.
    // See RTF_SymbolListGenSource.cs for how to generate a new version (it also contains the original
    // Symbol list which must be used as the source to generate this one).
    private readonly Symbol?[] _symbolTable =
    [
        null,
// Entry 12
        new Symbol("u", 0, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.UnicodeChar),
// Entry 11
        new Symbol("uc", 1, false, Enums.KeywordType.Property, (ushort)Enums.Property.UnicodeCharSkipCount),
        null,
// Entry 76
        new Symbol("cell", 0, false, Enums.KeywordType.Character, ' '),
        null, null,
// Entry 65
        new Symbol("xe", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 37
        new Symbol("colortbl", 0, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.ColorTable),
        null, null,
// Entry 24
        new Symbol("lquote", 0, false, Enums.KeywordType.Character, '\x2018'),
// Entry 30
        new Symbol("cs", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.CanBeDestOrNotDest),
// Entry 54
        new Symbol("keywords", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null,
// Entry 43
        new Symbol("footerl", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 45
        new Symbol("footnote", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null,
// Entry 7
        new Symbol("f", 0, false, Enums.KeywordType.Property, (ushort)Enums.Property.FontNum),
// Entry 62
        new Symbol("tc", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 16
        new Symbol("softline", 0, false, Enums.KeywordType.Character, '\n'),
        null, null, null,
// Entry 42
        new Symbol("footerf", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 64
        new Symbol("txe", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 15
        new Symbol("line", 0, false, Enums.KeywordType.Character, '\n'),
        null,
// Entry 13
        new Symbol("v", 1, false, Enums.KeywordType.Property, (ushort)Enums.Property.Hidden),
// Entry 32
        new Symbol("ts", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.CanBeDestOrNotDest),
// Entry 33
        new Symbol("listtext", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 0
        new Symbol("ansi", 1252, true, Enums.KeywordType.Special, (ushort)Enums.SpecialType.HeaderCodePage),
        null, null,
// Entry 20
        new Symbol("enspace", 0, false, Enums.KeywordType.Character, ' '),
// Entry 75
        new Symbol("row", 0, false, Enums.KeywordType.Character, '\n'),
// Entry 53
        new Symbol("info", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null,
// Entry 6
        new Symbol("fonttbl", 0, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.FontTable),
// Entry 59
        new Symbol("rxe", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null,
// Entry 63
        new Symbol("title", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 25
        new Symbol("rquote", 0, false, Enums.KeywordType.Character, '\x2019'),
// Entry 1
        new Symbol("pc", 437, true, Enums.KeywordType.Special, (ushort)Enums.SpecialType.HeaderCodePage),
// Entry 3
        new Symbol("pca", 850, true, Enums.KeywordType.Special, (ushort)Enums.SpecialType.HeaderCodePage),
        null, null,
// Entry 41
        new Symbol("footer", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 44
        new Symbol("footerr", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null,
// Entry 60
        new Symbol("stylesheet", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null, null,
// Entry 35
        new Symbol("author", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 48
        new Symbol("ftnsepc", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null,
// Entry 51
        new Symbol("headerl", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null,
// Entry 66
        new Symbol("pict", 1, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.SkippableHex),
        null, null,
// Entry 72
        new Symbol("objdata", 1, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.SkippableHex),
        null, null, null,
// Entry 74
        new Symbol("panose", 20, true, Enums.KeywordType.Special, (ushort)Enums.SpecialType.SkipNumberOfBytes),
// Entry 50
        new Symbol("headerf", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null,
// Entry 31
        new Symbol("ds", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.CanBeDestOrNotDest),
        null, null,
// Entry 46
        new Symbol("ftncn", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 18
        new Symbol("bullet", 0, false, Enums.KeywordType.Character, '\x2022'),
// Entry 57
        new Symbol("private", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 55
        new Symbol("operator", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 67
        new Symbol("themedata", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.SkippableHex),
        null, null,
// Entry 61
        new Symbol("subject", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null,
// Entry 5
        new Symbol("deff", 0, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.DefaultFont),
        null, null, null,
// Entry 8
        new Symbol("fcharset", -1, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.Charset),
// Entry 70
        new Symbol("datastore", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.SkippableHex),
        null,
// Entry 49
        new Symbol("header", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 52
        new Symbol("headerr", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 2
        new Symbol("mac", 10000, true, Enums.KeywordType.Special, (ushort)Enums.SpecialType.HeaderCodePage),
        null, null,
// Entry 47
        new Symbol("ftnsep", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 29
        new Symbol("fldinst", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.FieldInstruction),
        null, null, null, null,
// Entry 19
        new Symbol("emspace", 0, false, Enums.KeywordType.Character, ' '),
// Entry 28
        new Symbol("bin", 0, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.SkipNumberOfBytes),
        null, null,
// Entry 34
        new Symbol("pntext", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 21
        new Symbol("qmspace", 0, false, Enums.KeywordType.Character, ' '),
// Entry 14
        new Symbol("par", 0, false, Enums.KeywordType.Character, '\n'),
        null, null, null,
// Entry 69
        new Symbol("passwordhash", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.SkippableHex),
        null, null, null, null,
// Entry 38
        new Symbol("comment", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null, null,
// Entry 68
        new Symbol("colorschememapping", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.SkippableHex),
        null, null, null,
// Entry 73
        new Symbol("blipuid", 32, true, Enums.KeywordType.Special, (ushort)Enums.SpecialType.SkipNumberOfBytes),
        null,
// Entry 26
        new Symbol("ldblquote", 0, false, Enums.KeywordType.Character, '\x201C'),
        null, null,
// Entry 39
        new Symbol("creatim", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
// Entry 17
        new Symbol("tab", 0, false, Enums.KeywordType.Character, '\t'),
// Entry 10
        new Symbol("lang", 0, false, Enums.KeywordType.Property, (ushort)Enums.Property.Lang),
        null, null, null, null, null, null, null,
// Entry 4
        new Symbol("ansicpg", 1252, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.HeaderCodePage),
        null, null, null,
// Entry 58
        new Symbol("revtim", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null,
// Entry 23
        new Symbol("endash", 0, false, Enums.KeywordType.Character, '\x2013'),
        null, null, null, null, null, null, null,
// Entry 71
        new Symbol("datafield", 0, false, Enums.KeywordType.Destination,
            (ushort)Enums.DestinationType.SkippableHex),
        null, null, null, null,
// Entry 27
        new Symbol("rdblquote", 0, false, Enums.KeywordType.Character, '\x201D'),
        null, null,
// Entry 40
        new Symbol("doccomm", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
// Entry 56
        new Symbol("printim", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null,
// Entry 36
        new Symbol("buptim", 0, false, Enums.KeywordType.Destination, (ushort)Enums.DestinationType.Skip),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null,
// Entry 22
        new Symbol("emdash", 0, false, Enums.KeywordType.Character, '\x2014'),
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null, null, null, null, null,
        null, null, null, null, null,
// Entry 9
        new Symbol("cpg", -1, false, Enums.KeywordType.Special, (ushort)Enums.SpecialType.CodePage),
    ];

    private static Symbol?[] InitControlSymbolArray()
    {
        Symbol?[] ret = new Symbol?[256];
        ret['\''] = new Symbol("'", 0, false, Enums.KeywordType.Special, (int)Enums.SpecialType.HexEncodedChar);
        /*
        @RTF(KeywordType.Character and symbol fonts):
        \, {, and } are the only KeywordType.Character chars that can be in a symbol font. Everything else is
        either below 0x20 or more than one byte, which in either case means they can't be symbol font chars.
        ~ is nominally a non-breaking space, and in RichEdit is displayed as such (or at least whitespace of
        some kind), but in LibreOffice is displayed as a square dot when set to Wingdings (as expected).
        Since RichEdit doesn't treat it as a symbol font character we should in theory match its behavior,
        but we convert it to an ASCII space anyway so the whole thing is moot currently. But just in case we
        decide to change it, there's the info.

        We could maybe figure out a way to not have to do the symbol font check/conversion in the common case
        where we don't need to, is the point of this whole soliloquy.
        */
        ret['\\'] = new Symbol("\\", 0, false, Enums.KeywordType.Character, '\\');
        ret['{'] = new Symbol("{", 0, false, Enums.KeywordType.Character, '{');
        ret['}'] = new Symbol("}", 0, false, Enums.KeywordType.Character, '}');
        // Nominally Non-Breaking Space (0xA0)
        ret['~'] = new Symbol("~", 0, false, Enums.KeywordType.Character, ' ');
        // Nominally Non-Breaking Hyphen (0x2011)
        ret['_'] = new Symbol("_", 0, false, Enums.KeywordType.Character, '-');
        // There's also \- which is Optional Hyphen (the scanner is only producing single-line values, so no
        // need for this), and \: which "specifies a subentry in an index entry" (it's not clear even from
        // the spec what exactly an "index entry" is).
        return ret;
    }

    private readonly Symbol?[] ControlSymbols = InitControlSymbolArray();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Symbol? LookUpControlSymbol(char ch) => ControlSymbols[ch];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal Symbol? LookUpControlWord(char[] keyword, int len)
    {
        // Min word length is 1, and we're guaranteed to always be at least 1, so no need to check for >= min
        if (len <= MAX_WORD_LENGTH)
        {
            int key = len;

            // Original C code does a stupid thing where it puts default at the top and falls through and junk,
            // but we can't do that in C#, so have something clearer/clunkier
            switch (len)
            {
                // Most common case first - we get a measurable speedup from this
                case > 2:
                    key += asso_values[keyword[2]];
                    key += asso_values[keyword[1]];
                    key += asso_values[keyword[0]];
                    break;
                case 1:
                    key += asso_values[keyword[0]];
                    break;
                case 2:
                    key += asso_values[keyword[1]];
                    key += asso_values[keyword[0]];
                    break;
            }
            key += asso_values[keyword[len - 1]];

            if (key <= MAX_HASH_VALUE)
            {
                Symbol? symbol = _symbolTable[key];
                if (symbol == null)
                {
                    return null;
                }

                string seq2 = symbol.Keyword;
                if (len != seq2.Length)
                {
                    return null;
                }

                for (int ci = 0; ci < len; ci++)
                {
                    if (keyword[ci] != seq2[ci])
                    {
                        return null;
                    }
                }

                return symbol;
            }
        }

        return null;
    }
}