namespace VRCFaceTracking.PicoConnect.Models;

public class Lab
{
    public bool Quic { get; set; }
    public bool SuperResolution { get; set; }
    public int Gamma { get; set; }
    public int FaceTrackingMode { get; set; }
    public int FaceTrackingTransferProtocol { get; set; }
    public bool BodyTracking { get; set; }
    public int ControllerSensitivity { get; set; }
}
