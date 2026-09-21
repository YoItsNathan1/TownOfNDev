namespace TownOfNDev.Networking;

public enum TownOfNDevRpc : uint
{
    FungiInfectRequest = 6100,
    FungiInfectResult = 6101,
    FungiSpreadRequest = 6102,
    // Reserved legacy id. Infection state now synchronizes through MiraAPI RpcAddModifier.
    FungiApplyInfection = 6103,
    FungiSetObjectiveAchieved = 6104,

    TracerDustRequest = 6200,
    TracerDustResult = 6201,
    TracerInteractionRequest = 6202,

    CondemnerDeathNoteRequest = 6300,
    CondemnerDeathNoteResult = 6301,

    SuiProtectRequest = 6400,
    SuiProtectResult = 6401,
    SuiProtectionTriggerRequest = 6402,
    SuiHuntActivated = 6403
}
