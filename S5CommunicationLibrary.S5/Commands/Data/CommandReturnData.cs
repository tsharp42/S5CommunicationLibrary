namespace S5CommunicationLibrary.S5.Commands.Data;

public class CommandReturnData
{

    public decimal? FirmwareVersion = null;

    public string? Name = null;

    public int? MuteLevel = null;

    public decimal? Frequency = null;

    public float? RFA = null;
    public float? RFB = null;

    public float? AudioLevel = null;

    public float? BatteryLevel = null;

    public bool? IsMuted = null;
    public bool? IsPCMuted = null;

    public Receiver.Antenna? Antenna = null;

}
