using MudBlazor;

namespace CSHLDraft;

public class CSHLTheme
{
    public const string LogoUrl = "https://lh5.googleusercontent.com/aC0qeSXOO_Bsze5yY9s6F3x8Xf1EIOwf5DLqZEQONU14BRX1BWLJsKaAniR4Seh1AbscKmWBbZ7FCKnl0DV1zMu1ND1Q6aczNdM8qeUvdolyKdhLn5buCK_A9w=w16383";

    private readonly PaletteLight _defaultLight = new()
    {

    };

    private readonly PaletteDark _defaultDark = new()
    {

    };
    
    public MudTheme Theme { get; }

    public CSHLTheme()
    {
        Theme = new()
        {
            PaletteLight = _defaultLight,
            PaletteDark = _defaultDark,
            LayoutProperties = new(),
        };
    }
}