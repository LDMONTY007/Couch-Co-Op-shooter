
public enum ModifiedStat
{ 
    UseSpeed,
    MoveSpeed,
    Damage,
}


public class StatModifier
{
    public ModifiedStat type;   // Speed, Damage
    public float multiplier; // e.g. 1.25 = +25%
    public float additive;       // optional flat add
    public float divisor; // optional divisor
    public float subtractor; // optional subtractor
    public float duration;   // if timed, if set to 0 then it is not timed.
    public object source { get; set; } // Track what added the modifier
}
