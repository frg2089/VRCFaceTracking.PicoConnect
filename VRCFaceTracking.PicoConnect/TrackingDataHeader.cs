using System.Runtime.InteropServices;

namespace VRCFaceTracking.PicoConnect;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct TrackingDataHeader
{
    public const int Size = 6 * sizeof(byte) + sizeof(ushort) + sizeof(ulong);

    public byte StartCode1;
    public byte StartCode2;
    public byte TrackingType;
    public byte SubType;
    public byte MultiPacket;
    public byte CurrentPacketIndex;
    public ushort Version;
    public ulong Timestamp;
};
