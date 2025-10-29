using UnityEngine;

public class SpawnEvent : EntityGameEvent
{
    private string name;
    private string team;
    private Vector2 pos;
    private int maxHP;
        
    public SpawnEvent(EntityModel entity, float timestamp, string name, string team, Vector2 pos, int maxHP) : base(entity, timestamp)
    {
        this.name = name;
        this.team = team;
        this.pos = pos;
        this.maxHP = maxHP;
    }

    public override void Execute()
    {
        Entity.OnSpawned(name, team, pos, maxHP);
    }
}
