using System;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Usable;

[Serializable]
public class ScoreData
{
    //used for determining rules
    //about scoring.
    public enum EnemyType
    {
        None,
        Undead,
        Vampire,
        Ghost,
        Aquatic,
        Mummy,
        Werewolf
    }

    //If the player shot an explosive,
    //the entire score will double.
    public bool wasIndirectDamage = false;
    //Used if a weapon will give bonuses for targeting
    //a specific weakpoint, for example the stake
    //will reveal vampire hearts and will give 150% bonus
    //for hitting them in the heart.
    public bool didHitWeakPoint = false;
    public EnemyType enemyType = EnemyType.None;
    public DamageType damageType = DamageType.None; 
    public float baseScore = 0f;

    public Vector3 spawnPos = Vector3.zero;

    //Everytime an enemy is damaged,
    //we'll add a scoredata to the player
    //from said damage and calculate values
    //using it.
    public float calcBonus()
    {
        //Logic for determing score modifiers for
        //enemy type and weapon type.
        switch (enemyType)
        {
            case EnemyType.None:
                //return the default score value
                return baseScore;
            case EnemyType.Undead:
                if (damageType == DamageType.Healing)
                {
                    //50% bonus for using a type advantage.
                    return baseScore * 0.5f;
                }
                break;
            case EnemyType.Vampire:
                if (damageType == DamageType.Stake && didHitWeakPoint)
                {
                    //150% bonus for getting a stake in the heart kill.
                    return baseScore * 1.5f;
                }
                if (damageType == DamageType.Garlic)
                {
                    //50% bonus for using a type advantage.
                    return baseScore * 0.5f;
                }
                if (damageType == DamageType.Light)
                {
                    //It's harder to hit
                    //the vampire with the camera because
                    //using the camera they are invisible.
                    //75% bonus for using a type advantage.
                    return baseScore * 0.75f;
                }
                break;
            case EnemyType.Ghost:
                //Sub type advantage.
                if (damageType == DamageType.Healing)
                {
                    //25% bonus for using a type advantage.
                    return baseScore * 0.25f;
                }
                //Main type advantage
                if (damageType == DamageType.Light)
                {
                    //50% bonus for using a type advantage.
                    return baseScore * 0.50f;
                }
                break;
            case EnemyType.Aquatic:
                //Sub type advantage
                if (damageType == DamageType.Light)
                {
                    //25% bonus for using a type advantage.
                    return baseScore * 0.25f;
                }
                break;
            case EnemyType.Mummy:
                //Sub type advantage
                if (damageType == DamageType.Healing)
                {
                    //25% bonus for using a type advantage.
                    return baseScore * 0.25f;
                }
                //Sub type advantage
                if (damageType == DamageType.Silver)
                {
                    //25% bonus for using a type advantage.
                    return baseScore * 0.25f;
                }
                break;
            case EnemyType.Werewolf:
                //Main Type advantage
                if (damageType == DamageType.Silver)
                {
                    //50% bonus for using a type advantage.
                    return baseScore * 0.50f;
                }
                break;

        }

        return 0f;
    }

    public float GetTotalScore()
    {
        float score = baseScore + calcBonus();
        //lastly we should double the overall score if it was indirect damage.
        score = wasIndirectDamage ? score * 2 : score;

        return score;
    }

    //Constructor.
    public ScoreData(float score, DamageType damageType, EnemyType enemyType, Vector3 spawnPos)
    {
        this.enemyType = enemyType;
        this.damageType = damageType;
        baseScore = score;
        this.spawnPos = spawnPos;
    }

    public ScoreData(ScoreData sd)
    {
        this.enemyType = sd.enemyType;
        this.damageType = sd.damageType;
        baseScore = sd.baseScore;
        this.spawnPos = sd.spawnPos;
    }
}
