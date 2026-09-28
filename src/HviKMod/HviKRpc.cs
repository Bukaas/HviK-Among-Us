namespace HviKMod;

/// <summary>
/// IDs fuer eigene Netzwerk-Nachrichten. Reihenfolge nie aendern, nur hinten anfuegen
/// (sonst verstehen sich unterschiedliche Mod-Versionen nicht mehr).
/// </summary>
public enum HviKRpc : uint
{
    Transport = 0,
    PenguinGrab = 1,
    PenguinRelease = 2,
    AnnounceFriendCode = 3,
    BastionBombVent = 4,
    BastionRemoveBomb = 5,
    AnnounceGuess = 6,
}
