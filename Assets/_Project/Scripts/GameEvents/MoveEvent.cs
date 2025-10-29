using UnityEngine;

public class MoveEvent : EntityGameEvent
{
    private Vector2 newPos;
    
    public MoveEvent(EntityModel entity, float timestamp, Vector2 newPos) : base(entity, timestamp)
    {
        this.newPos = newPos;
    }

    public override void Execute()
    {
        Entity.OnMoved(newPos);
    }
}
