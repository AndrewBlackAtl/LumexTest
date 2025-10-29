using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private string fileName = "events.jsonl";
    [SerializeField] private World world;
    [SerializeField] private int preloadViewsAmount = 10;
    
    private void Start()
    {
        world.Initialize(preloadViewsAmount);
    }

    private void SetupAndRun()
    {
        var filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogError($"File with name {filePath} is not exists");
            return;
        }
        var content = System.IO.File.ReadAllText(filePath);
        
        var events = UnityEngine.Pool.ListPool<GameEventDTO>.Get();
        NDJSONParser.Parse(content, events);
        if (events.Count == 0)
        {
            Debug.LogWarning($"There is no game events in file {filePath}");
            return;
        }
        
        events.Sort((x, y) => x.ts.CompareTo(y.ts));
            
        foreach (var gameEvent in events)
        {
            world.AddEvent(world.ConvertEvent(gameEvent));
        }
        
        UnityEngine.Pool.ListPool<GameEventDTO>.Release(events);
        
        world.RunSimulation();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 30), "Run simulation"))
        {
            SetupAndRun();
        }
        
        if (GUI.Button(new Rect(10, 50, 100, 30), "Stop Simulation"))
        {
            world.StopSimulation();
        }
    }
}
