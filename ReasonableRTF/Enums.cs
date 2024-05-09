﻿namespace ReasonableRTF;

internal static class Enums
{
    #region Enums

    internal enum SpecialType : byte
    {
        HeaderCodePage,
        DefaultFont,
        FontTable,
        Charset,
        CodePage,
        UnicodeChar,
        HexEncodedChar,
        SkipNumberOfBytes,
        ColorTable,
        CellRowEnd,
    }

    internal enum KeywordType : byte
    {
        Character,
        Property,
        Destination,
        Special,
    }

    internal enum DestinationType : byte
    {
        FieldInstruction,
        /// <summary>
        /// This is for \csN, \dsN, and \tsN.
        /// <para/>
        /// These are weird hybrids that can either be written as destinations (eg. "\*\cs15") or not (eg. "\cs15").
        /// <para/>
        /// The spec explains:<br/>
        /// "\csN:<br/>
        /// Designates character style with a style handle N. Like \sN, \csN is not a destination control word.<br/>
        /// However, it is important to treat it like one inside the style sheet; that is, \csN must be prefixed<br/>
        /// with \* and must appear as the first item inside a group. Doing so ensures that readers that do not<br/>
        /// understand character styles will skip the character style information correctly. When used in body<br/>
        /// text to indicate that a character style was applied, do not include the \* prefix."
        /// <para/>
        /// Despite the fact that the \* prefixed versions are only supposed to appear at the start of a group,<br/>
        /// there's one readme (Thief Trinity) where \*\csN is written in the middle of a group. If we treated<br/>
        /// the \* prefixed version as being a skip-group trigger, then we would skip the rest of the group if it<br/>
        /// was in the middle of one, missing whatever text was after it.
        /// <para/>
        /// However, we actually don't have to treat any version of the word as a skip-group trigger, because the<br/>
        /// only time we want that is when they're in the \stylesheet group, which is already being skipped. So<br/>
        /// ignoring the word is a no-op in \stylesheet, and also a no-op in a regular group, which is what we want<br/>
        /// in both cases.
        /// </summary>
        CanBeDestOrNotDest,
        Skip,
        SkippableHex,
    }

    internal const int PropertiesLen = 4;
    internal enum Property : byte
    {
        Hidden,
        UnicodeCharSkipCount,
        FontNum,
        Lang,
    }

    // TODO: Un-hardcode the symbol fonts here and make them all into arbitrary-style ones, just with a few of them built-in already
    internal enum SymbolFont : byte
    {
        // Non-font values at the start, to avoid having to check the top bounds
        None,
        Unset,
        Symbol,
        Wingdings,
        Wingdings2,
        Wingdings3,
        Webdings,
        ZapfDingbats,
    }

    #endregion
}
