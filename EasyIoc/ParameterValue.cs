namespace EasyIoc
{
    public class ParameterValue : IParameterValue
    {
        public string Name { get; private set; }
        public object Value { get; private set; }

        public ParameterValue(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
