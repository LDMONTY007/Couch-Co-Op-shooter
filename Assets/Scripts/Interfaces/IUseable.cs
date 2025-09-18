using UnityEngine;

public interface IUseable
{
    public void Use(float useSpeed = 1f);

    public void CancelUse();

    public void ReleaseUse();
}
