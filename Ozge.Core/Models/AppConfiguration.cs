namespace Ozge.Core.Models;

public sealed record AppConfiguration(
    string Language,
    bool HighContrast,
    bool ReducedMotion,
    int BonusDurationSeconds,
    string PinCode,
    string ProjectorDisplayDeviceId)
{
    public static AppConfiguration Default => new(
        Language: "en",
        HighContrast: false,
        ReducedMotion: false,
        BonusDurationSeconds: 30,
        PinCode: "1234",
        ProjectorDisplayDeviceId: string.Empty);
}
