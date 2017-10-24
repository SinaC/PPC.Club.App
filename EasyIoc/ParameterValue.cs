namespace EasyIoc
{
    public sealed class ParameterValue : IParameterValue
    {
        public string Name { get; }
        public object Value { get; }

        public ParameterValue(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
