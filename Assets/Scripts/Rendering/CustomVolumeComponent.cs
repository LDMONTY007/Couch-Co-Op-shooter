using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class CustomVolumeComponent : VolumeComponent
{
    public BoolParameter isActive = new BoolParameter(true);
    public Vector2Parameter pixelSize = new Vector2Parameter(new Vector2(320, 240));
}
