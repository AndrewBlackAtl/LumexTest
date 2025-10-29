public class ReactiveProperty<T>
{
    private T value;
    public event System.Action<T> OnChanged;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            OnChanged?.Invoke(value);
        }
    }

    public ReactiveProperty(T initValue = default)
    {
        value = initValue;
    }
}