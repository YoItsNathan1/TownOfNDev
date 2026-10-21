using System;
using HarmonyLib;

namespace TownOfNDev.Patches;

// This is a display-only patch. It runs after Reactor has written its credits
// and ToU-Mira has added the Region line; no game or role state is touched.
[HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
[HarmonyAfter("gg.reactor.api", "auavengers.tou.mira")]
internal static class TownOfNDevPingTrackerPatch
{
    private const string CreditText = "TownOfNDev 0.1.0-beta.2.0.0 (ToU 1.7.9)";
    private const string BlueCredit = ", <color=#4DA6FF>" + CreditText + "</color>";

    [HarmonyPostfix]
    // Both upstream postfixes specify Priority.Last (0). Run strictly after
    // both, including if Harmony ignores their relative registration order.
    [HarmonyPriority(-100)]
    public static void Postfix(PingTracker __instance)
    {
        if (__instance == null || __instance.text == null)
        {
            return;
        }

        var footer = __instance.text.text;
        if (string.IsNullOrEmpty(footer) ||
            footer.Contains(CreditText, StringComparison.Ordinal))
        {
            return;
        }

        // ToU-Mira adds the region as a separate line. Find the credits
        // immediately before that line, without depending on case-sensitive
        // mod names or a particular version format.
        var regionStart = footer.LastIndexOf("\n<align=center><size=60%>", StringComparison.Ordinal);
        if (regionStart < 0)
        {
            return;
        }

        var touIndex = footer.LastIndexOf("Town Of Us: Mira", regionStart,
            StringComparison.OrdinalIgnoreCase);
        if (touIndex < 0)
        {
            return;
        }

        // Reactor wraps each mod credit in <color> ... </color> with the
        // mod name/version inside <noparse>. Append after ToU's closing color
        // tag, keeping the added NDev credit on the same formatted line.
        var insertionAt = footer.IndexOf("</color>", touIndex, StringComparison.Ordinal);
        if (insertionAt < 0 || insertionAt >= regionStart)
        {
            return;
        }

        insertionAt += "</color>".Length;
        footer = footer.Insert(insertionAt, BlueCredit);

        // The credits line is longer now; shrink only this line, leaving
        // ToU's Region line and the actual ping display unchanged.
        var sizeAt = footer.LastIndexOf("<size=50%>", touIndex, StringComparison.Ordinal);
        if (sizeAt >= 0)
        {
            footer = footer.Remove(sizeAt, "<size=50%>".Length)
                .Insert(sizeAt, "<size=38%>");
        }

        __instance.text.text = footer;
    }
}
