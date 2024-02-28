namespace VRCFaceTracking.PicoConnect.Models;

public class Video
{
    public required string Resolution { get; set; }
    public required Bitrate Bitrate { get; set; }
    public bool AutoBitrate { get; set; }
    public bool RefreshRate90Hz { get; set; }
    public bool FrameBuffer { get; set; }
    public required string Codec { get; set; }
    public bool Asw { get; set; }
    public int SharpenRate { get; set; }
}
