namespace CosmosDbExplorer.Core.Models
{
    public class Optional<T>
    {
        public Optional(T value)
        {
            Value = value;
            IsSome = true;
        }

        public Optional()
        {
            Value = default;
            IsSome = false;
        }

        public T? Value { get; }
        
        public bool IsSome { get; }
    }

}
