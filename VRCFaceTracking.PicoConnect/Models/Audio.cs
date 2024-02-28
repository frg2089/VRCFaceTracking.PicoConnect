namespace VRCFaceTracking.PicoConnect.Models;

public class Audio
{
    public bool Mic { get; set; }
    public int Volume { get; set; }
    public required string Output { get; set; }
    public int Latency { get; set; }
}
