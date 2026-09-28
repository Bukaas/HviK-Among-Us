using MiraAPI.Utilities.Assets;

namespace HviKMod;

/// <summary>
/// Alle eingebetteten Grafiken (src/HviKMod/Resources, erzeugt von scripts/make-background.ps1).
/// </summary>
public static class HviKAssets
{
    public static LoadableResourceAsset MenuBackground { get; } = new("HviKMod.Resources.MenuBackground.png");
    public static LoadableResourceAsset MenuLogo { get; } = new("HviKMod.Resources.MenuLogo.png");

    // Button-Grafiken sind 128x128 -> bei 110 px/Einheit so gross wie die Vanilla-Buttons.
    public static LoadableResourceAsset SheriffButton { get; } = new("HviKMod.Resources.Buttons.Sheriff.png", 110);
    public static LoadableResourceAsset TransporterButton { get; } = new("HviKMod.Resources.Buttons.Transporter.png", 110);
    public static LoadableResourceAsset PuppeteerButton { get; } = new("HviKMod.Resources.Buttons.Puppeteer.png", 110);
    public static LoadableResourceAsset PenguinButton { get; } = new("HviKMod.Resources.Buttons.Penguin.png", 110);

    // Meeting-Buttons sind kleiner (Groesse wie der Abstimm-Haken) -> hoehere Pixeldichte.
    public static LoadableResourceAsset GuessButton { get; } = new("HviKMod.Resources.Buttons.Guess.png", 220);
}
