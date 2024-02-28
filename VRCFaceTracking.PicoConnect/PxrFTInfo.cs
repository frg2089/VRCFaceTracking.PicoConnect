using System.Runtime.InteropServices;

namespace VRCFaceTracking.PicoConnect;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct PxrFTInfo
{
    public const int BLEND_SHAPE_NUMS = 72;
    public const int Size = sizeof(long) + BLEND_SHAPE_NUMS * sizeof(float) + 10 * sizeof(float) + sizeof(float) + 10 * sizeof(float) + 128 * sizeof(float);

    public long Timestamp;
    public fixed float BlendShapeWeight[BLEND_SHAPE_NUMS];
    public fixed float VideoInputValid[10];
    public float LaughingProb;
    public fixed float EmotionProb[10];
    public fixed float Reserved[128];
};