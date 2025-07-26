using MudBlazor;

namespace CSHLDraft;

public class CSHLTheme
{
    public const string LogoUrl = "https://ffcghkodqzdpgfmzrmci.supabase.co/storage/v1/object/public/brand//cshl-logo.png";
    public const string EmptyProfileUrl = "https://ffcghkodqzdpgfmzrmci.supabase.co/storage/v1/object/public/headshots//default.jpg";

    private readonly PaletteLight _defaultLight = new()
    {
        Primary = "#bc1b20",
        AppbarBackground = "#bc1b20",
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