using UnityEngine;

public class DieEvent : EntityGameEvent
{
    private string killerId;
    
    public DieEvent(EntityModel entity, float timestamp, string killerId) : base(entity, timestamp)
    {
        this.killerId = killerId;
    }

    public override void Execute()
    {
        Entity.OnDied(killerId);
    }
}
