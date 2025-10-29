using UnityEngine;

public class TakeDamageEvent : EntityGameEvent
{
    private int dmgAmount;
    private int newHP;
        
    public TakeDamageEvent(EntityModel entity, float timestamp, int dmgAmount, int newHP) : base(entity, timestamp)
    {
        this.dmgAmount = dmgAmount;
        this.newHP = newHP;
    }

    public override void Execute()
    {
        Entity.OnDamaged(dmgAmount, newHP);
    }
}
