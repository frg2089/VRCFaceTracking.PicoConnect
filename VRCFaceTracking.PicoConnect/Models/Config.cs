namespace VRCFaceTracking.PicoConnect.Models;

public class Config
{
    public required Video Video { get; set; }
    public required Audio Audio { get; set; }
    public required General General { get; set; }
    public required Lab Lab { get; set; }
}
