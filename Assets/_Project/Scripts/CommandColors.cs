using UnityEngine;

[CreateAssetMenu(fileName = "CommandColors", menuName = "Scriptable Objects/CommandColors")]
public class CommandColors : ScriptableObject
{
    [System.SerializableAttribute]
    public struct CommandColor
    {
        public string CommandName;
        public Color Color;
    }

    public System.Collections.Generic.List<CommandColor> Colors;
}
