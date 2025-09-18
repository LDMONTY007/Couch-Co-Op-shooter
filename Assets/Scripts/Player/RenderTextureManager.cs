using UnityEngine;

public class RenderTextureManager : MonoBehaviour
{
    public Camera cam;
    public int playerIndex = -1;
    public RenderTexture[] textures = new RenderTexture[4];

    public void InitRenderTexture(int index)
    { 
        playerIndex = index;

        //Set the camera's render texture to match the player index.
        cam.targetTexture = textures[playerIndex];
    }

}
