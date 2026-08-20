namespace SAIN.Preset.Shared.Enums;

/// <summary>
/// SAIN mirror of EFT's ETagStatus
/// </summary>
[System.Flags]
public enum ESainTagStatus
{
    Unaware = 1,
    Aware = 2,
    Combat = 4,
    Solo = 8,
    Coop = 16,
    Bear = 32,
    Usec = 64,
    Scav = 128,
    TargetSolo = 256,
    TargetMultiple = 512,
    Healthy = 1024,
    Injured = 2048,
    BadlyInjured = 4096,
    Dying = 8192,
    Birdeye = 16384,
    Knight = 32768,
    BigPipe = 65536,
}
