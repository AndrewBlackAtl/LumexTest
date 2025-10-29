using UnityEngine;

public class NDJSONParser
{
    public static void Parse<T>(string json, System.Collections.Generic.IList<T> result)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }
        
        var lines = json.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                result.Add(JsonUtility.FromJson<T>(line));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                throw;
            }
        }
    }
}
