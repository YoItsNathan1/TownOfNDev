using BepInEx.Logging;
using MiraAPI.Translation;

namespace TownOfNDev.Modules;

public static class TownOfNDevLocale
{
    private static ManualLogSource Logger { get; } = BepInEx.Logging.Logger.CreateLogSource("TownOfNDevLocale");

    public static void LoadInternalLocale()
    {
        try
        {
            // MiraAPI 0.5.0 loads embedded locale files from
            // <root namespace>.Resources.Locale.<language>.xml.
            MiraLocaleManager.Register("ndevstudios.townofndev", "TownOfNDev");
            Logger.LogInfo("TownOfNDev internal locale registered with MiraAPI.");
        }
        catch (Exception ex)
        {
            Logger.LogError($"TownOfNDev locale registration failed: {ex}");
        }
    }
}
