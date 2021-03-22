using System;

namespace Demo.Api.Data
{
    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException(string modelName, Guid key) : base($"Failed to find {modelName} (Key={key})")
        {
        }
    }
}
