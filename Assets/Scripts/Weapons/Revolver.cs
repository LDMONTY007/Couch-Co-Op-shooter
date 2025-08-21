using UnityEngine;

public class Revolver : Weapon
{
    
    int mask;

    private void Awake()
    {
        //Get everything except player and ignore raycast.
        mask = ~LayerMask.GetMask("Player", "IgnoreRaycast");
    }

    public override void Attack(Camera c)
    {
        if (Physics.Raycast(c.transform.position, c.transform.forward, out var hitInfo, 5f, mask))
        {
            Debug.Log("HIT!!");
        }
    }
}
