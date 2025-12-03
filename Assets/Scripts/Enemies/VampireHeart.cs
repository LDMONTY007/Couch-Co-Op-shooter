using UnityEngine;

public class VampireHeart : MonoBehaviour, IDamageable
{
    public Vampire vampire;

    public void TakeDamage(DamageData damageData)
    {
        vampire.TakeDamage(damageData);
    }

    public ScoreData[] TakeDamageScored(DamageData damageData)
    {
        ScoreData[] scores = vampire.TakeDamageScored(damageData);

        //Say that the weakpoint on this enemy was hit
        //so the player gets the score bonus.
        foreach (ScoreData sd in scores)
        {
            sd.didHitWeakPoint = true;
        }

        //Instantly kill the vampire as it's heart was staked.
        vampire.Die();

        return scores;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        OnDamageableEnabled();
    }

    private void OnDisable()
    {
        OnDamageableDisabled();
    }

    public void OnDamageableDisabled()
    {
        GameManager.Instance.damageables.Remove(this);
    }

    public void OnDamageableEnabled()
    {
        GameManager.Instance.damageables.Add(this);
    }
}
