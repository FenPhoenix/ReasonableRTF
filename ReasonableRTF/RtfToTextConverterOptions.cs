namespace ReasonableRTF;

public sealed class RtfToTextConverterOptions
{
    /// <summary>
    /// Gets or sets whether to swap the uppercase and lowercase Greek phi characters in the Symbol font to Unicode
    /// translation table.
    /// <para/>
    /// The Windows Symbol font has these two characters swapped from their nominal positions.
    /// You can disable this by setting this property to <see langword="false"/>.
    /// <para/>
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool SwapUppercaseAndLowercasePhiSymbols { get; set; } = true;

    /// <summary>
    /// Gets or sets the character at index 0xA0 (160) in the Symbol font to Unicode translation table.
    /// This character is nominally the Euro sign, but in older versions of the Symbol font it may have been a
    /// numeric space or undefined.
    /// <para/>
    /// The default value is <see cref="SymbolFontA0Char.EuroSign"/>.
    /// </summary>
    public SymbolFontA0Char SymbolFontA0Char { get; set; } = SymbolFontA0Char.EuroSign;

    /// <summary>
    /// Gets or sets the line break style for the converted plain text.
    /// <para/>
    /// The default value is <see cref="LineBreakStyle.EnvironmentDefault"/>.
    /// </summary>
    public LineBreakStyle LineBreakStyle { get; set; } = LineBreakStyle.EnvironmentDefault;

    /// <summary>
    /// Gets or sets whether to convert text that is marked as hidden. If <see langword="true"/>, this text will
    /// appear in the plain text output; otherwise it will not.
    /// <para/>
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool ConvertHiddenText { get; set; }

    internal void CopyTo(RtfToTextConverterOptions dest)
    {
        dest.SwapUppercaseAndLowercasePhiSymbols = SwapUppercaseAndLowercasePhiSymbols;
        dest.SymbolFontA0Char = SymbolFontA0Char;
        dest.LineBreakStyle = LineBreakStyle;
        dest.ConvertHiddenText = ConvertHiddenText;
    }
}
