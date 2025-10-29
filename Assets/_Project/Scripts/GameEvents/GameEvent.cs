using UnityEngine;

public interface IGameEvent
{
    public float GetTimestamp { get; }
    public void Execute();
}

public abstract class EntityGameEvent : IGameEvent
{
    protected EntityModel Entity;

    protected EntityGameEvent(EntityModel entity, float timestamp)
    {
        Entity = entity;
        GetTimestamp = timestamp;
    }
    
    public float GetTimestamp { get; }

    public abstract void Execute();
}

public static class GameEventConverter
{
    public static IGameEvent ConvertEvent(this World world, in GameEventDTO dto)
    {
        if (world == null)
        {
            return null;
        }
        
        IGameEvent result = null;
        switch (dto.type)
        {
            case GameEventDTO.EntityMovedEvent:
                result = new MoveEvent(world.GetOrCreateEntity(dto.id), dto.ts,
                    new Vector2(dto.data.x, dto.data.y));
                break;
            case GameEventDTO.EntitySpawnedEvent:
                result = new SpawnEvent(world.GetOrCreateEntity(dto.id), dto.ts, dto.data.name, dto.data.team,
                    new Vector2(dto.data.x, dto.data.y), dto.data.max_hp);
                break;
            case GameEventDTO.EntityAttackedEvent:
                result = new AttackEvent(world.GetOrCreateEntity(dto.id), dto.ts, dto.data.target_id);
                break;
            case GameEventDTO.EntityDamagedEvent:
                result = new TakeDamageEvent(world.GetOrCreateEntity(dto.id), dto.ts, dto.data.amount, dto.data.hp);
                break;
            case GameEventDTO.EntityDiedEvent:
                result = new DieEvent(world.GetOrCreateEntity(dto.id), dto.ts, dto.data.killer_id);
                break;
            default:
                Debug.LogError($"Event with type {dto.type} is  not supported");
                break;
        }

        return result;
    }
}
