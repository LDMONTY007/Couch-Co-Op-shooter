using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

//LD didn't come up with this,
//here's the source: https://www.youtube.com/watch?v=XJJl19N2KFM
public class CutoutMaskUI : Image
{
   public override Material materialForRendering
    {
        get
        {
            Material material = new Material(base.materialForRendering);
            material.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
            return material;
        }
    }
}
