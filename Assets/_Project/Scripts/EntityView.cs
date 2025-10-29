using UnityEngine;

public class EntityView : MonoBehaviour
{
    [SerializeField] private GameObject viewRoot;
    [SerializeField] private TMPro.TextMeshProUGUI idText;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CommandColors colors;
    
    private EntityModel entity;

    public void Initialize(in EntityModel entity)
    {
        this.entity = entity;
        idText.text = entity.Id.Value;
        
        this.entity.IsAlive.OnChanged += IsAliveChanged;
        this.entity.Position.OnChanged += PositionChanged;
        this.entity.Team.OnChanged += TeamChanged;
    }

    private void TeamChanged(string value)
    {
        spriteRenderer.color = colors.Colors.Find(x => x.CommandName == value).Color;
    }

    private void PositionChanged(Vector2 value)
    {
        transform.position = value;
    }
    
    private void IsAliveChanged(bool value)
    {
        viewRoot.SetActive(value);
    }

    private void OnDisable()
    {
        if (this.entity != null)
        {
            this.entity.IsAlive.OnChanged -= IsAliveChanged;
            this.entity.Position.OnChanged -= PositionChanged;
            this.entity.Team.OnChanged -= TeamChanged;
            this.entity = null;
        }
    }
}
