namespace Demo.Api.ReferenceData
{
    public class ReferenceData
    {
        public string Code { get; }
        public string Description { get; }

        public ReferenceData(string code, string description)
        {
            Code = code;
            Description = description;
        }
    }
}
