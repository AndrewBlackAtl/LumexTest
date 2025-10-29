using UnityEngine;

public class AttackEvent : EntityGameEvent
{
    private string targetId;
    
    public AttackEvent(EntityModel entity, float timestamp, string targetId) : base(entity, timestamp)
    {
        this.targetId = targetId;
    }

    public override void Execute()
    {
        Entity.OnAttacked(targetId);
    }
}
