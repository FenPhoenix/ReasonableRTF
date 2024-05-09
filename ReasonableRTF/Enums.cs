namespace ReasonableRTF;

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
        /// We don't really have a way to handle this table-side, so just call it a destination and give it a<br/>
        /// special-cased type where it just skips the control word always, even if it was a destination and<br/>
        /// would normally have caused its entire group to be skipped.
        /// <para/>
        /// You might think we could just elide these from the table entirely and accomplish the same thing,<br/>
        /// but there's one readme (Thief Trinity) where \*\csN is written in the middle of a group, rather<br/>
        /// than the start of a group as destinations are supposed to be written, causing us to skip its group<br/>
        /// and miss some text.
        /// <para/>
        /// The correct way to handle this would be to track whether we're at the start of a group when we hit<br/>
        /// one of these, but that's a bunch of crufty garbage so whatever, let's just stick with this, as it<br/>
        /// seems to work fine...
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

    internal enum SymbolFont : byte
    {
        // Non-font values at the start, to avoid having to check the top bounds
        None,
        Unset,
        Symbol,
        Wingdings,
        Webdings,
    }

    #endregion
}
