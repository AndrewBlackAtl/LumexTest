using System.Linq;
using UnityEngine;

public class World : MonoBehaviour
{
    public EntityView EntityViewPrefab;
    
    private UnityEngine.Pool.IObjectPool<EntityView> entityViewPool;
    private UnityEngine.Pool.IObjectPool<EntityModel> entityPool;
    
    private System.Collections.Generic.Dictionary<string, EntityModel> spawnedEntities = UnityEngine.Pool.DictionaryPool<string, EntityModel>.Get();
    private System.Collections.Generic.Dictionary<string, EntityView> spawnedViews = UnityEngine.Pool.DictionaryPool<string, EntityView>.Get();
    
    private System.Collections.Generic.Queue<IGameEvent> eventsQueue = new();
    private float time;
    private Coroutine simulationProcess;

    public EntityModel GetOrCreateEntity(string id)
    {
        if (spawnedEntities.TryGetValue(id, out var entity))
        {
            return entity;
        }
        
        entity = entityPool.Get();
        entity.Initialize(id);
        
        var entityView = entityViewPool.Get();
        entityView.Initialize(entity);
        spawnedViews[id] = entityView;
            
        spawnedEntities[id] = entity;
        return entity;
    }

    private void RemoveEntity(string id)
    {
        entityViewPool.Release(spawnedViews[id]);
        entityPool.Release(spawnedEntities[id]);
        spawnedEntities.Remove(id);
        spawnedViews.Remove(id);
    }

    private void RemoveDeadEntities()
    {
        var entitiesToRemove = UnityEngine.Pool.ListPool<string>.Get();
        foreach (var kvp in spawnedEntities)
        {
            if (!kvp.Value.IsAlive.Value)
            {
                entitiesToRemove.Add(kvp.Key);
            }
        }
        foreach (var id in entitiesToRemove)
        {
            RemoveEntity(id);
        }
        UnityEngine.Pool.ListPool<string>.Release(entitiesToRemove);
    }

    public void Initialize(int preloadViewsAmount = 10)
    {
        entityViewPool = new UnityEngine.Pool.ObjectPool<EntityView>(
            createFunc:() => Instantiate(EntityViewPrefab), 
            actionOnGet: view => view.gameObject.SetActive(true),
            actionOnRelease: view => view.gameObject.SetActive(false), 
            actionOnDestroy: view => Destroy(view),
            defaultCapacity: preloadViewsAmount);
        
        entityViewPool.Preload(preloadViewsAmount);

        entityPool = new UnityEngine.Pool.ObjectPool<EntityModel>(
            createFunc: () => new EntityModel(),
            defaultCapacity: preloadViewsAmount);
        
        entityPool.Preload(preloadViewsAmount);
    }

    public void RunSimulation()
    {
        if (simulationProcess != null)
        {
            Debug.LogWarning("Simulation is already running");
            return;
        }

        simulationProcess = StartCoroutine(SimulationProcess());
    }

    public void StopSimulation()
    {
        if (simulationProcess == null)
        {
            return;
        }
        StopCoroutine(simulationProcess);
        simulationProcess = null;
        
        foreach (var view in spawnedViews.Values)
        {
            entityViewPool.Release(view);
        }
        foreach (var entity in spawnedEntities.Values)
        {
            entityPool.Release(entity);
        }
        
        spawnedEntities.Clear();
        spawnedViews.Clear();
        eventsQueue.Clear();
        
        time = 0f;
    }

    public void AddEvent(IGameEvent gameEvent)
    {
        if (gameEvent != null)
        {
            eventsQueue.Enqueue(gameEvent);
        }
    }

    private System.Collections.IEnumerator SimulationProcess()
    {
        while (true)
        {
            while (eventsQueue.Count > 0 && time >= eventsQueue.Peek().GetTimestamp)
            {
                eventsQueue.Dequeue().Execute();
            }

            RemoveDeadEntities();

            time += Time.deltaTime * 1000f;
            yield return null;
            
        }
    }
}
