using UnityEngine;
using UnityEngine.Rendering;

public class ScreenOffsetVolumeComponent : VolumeComponent
{
    public FloatParameter horizontalOffset = new FloatParameter(0f);
    public FloatParameter verticalOffset = new FloatParameter(0f);
    public IntParameter playerIndex = new IntParameter(-1);
}
