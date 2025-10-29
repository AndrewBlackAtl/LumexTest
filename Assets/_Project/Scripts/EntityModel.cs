using UnityEngine;


public class EntityModel
{
    public readonly ReactiveProperty<string> Id = new();
    public readonly ReactiveProperty<string> Name = new();
    public readonly ReactiveProperty<string> Team = new();
    public readonly ReactiveProperty<Vector2> Position = new();
    public readonly ReactiveProperty<int> MaxHP = new();
    public readonly ReactiveProperty<int> CurrentHP = new();
    public readonly ReactiveProperty<bool> IsAlive = new();
    
    public void Initialize(string id)
    {
        Id.Value = id;
    }

    public void OnSpawned(string name, string team, Vector2 position, int maxHP)
    {
        Name.Value = name;
        Team.Value = team;
        Position.Value = position;
        MaxHP.Value = maxHP;
        CurrentHP.Value = maxHP;
        IsAlive.Value = true;
    }
    
    public void OnDied(string killerId)
    {
        IsAlive.Value = false;
    }

    public void OnMoved(Vector2 newPos)
    {
        Position.Value = newPos;
    }

    public void OnAttacked(string targetId)
    {
        
    }

    public void OnDamaged(int dmgAmount, int newHP)
    {
        CurrentHP.Value = newHP;
    }
}
