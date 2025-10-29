using UnityEngine;

[System.Serializable]
public struct GameEventDTO
{
    public const string EntityMovedEvent = "entity_moved";
    public const string EntitySpawnedEvent = "entity_spawned";
    public const string EntityAttackedEvent = "entity_attacked";
    public const string EntityDamagedEvent = "entity_damaged";
    public const string EntityDiedEvent = "entity_died";
    
    public float ts;
    public string type;
    public string id;
    public EventDataDTO data;
}

[System.Serializable]
public struct EventDataDTO
{
    public string name;
    public string team;
    public float x;
    public float y;
    public int hp;
    public int max_hp;
    public string target_id;
    public string killer_id;
    public int amount;
}
