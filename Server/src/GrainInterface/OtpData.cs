using System;

public class OtpData
{
    public Guid Otp { get; set; }
    public Guid AccountGuid { get; set; }
    public long RegionIndex { get; set; }
    public Guid PlayerGuid { get; set; }
    public string PlayerName { get; set; }
}
