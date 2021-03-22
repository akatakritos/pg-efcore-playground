using System;
using Demo.Api.Shared;

namespace Demo.Api.Data
{
    public class RecordNotFoundException : Exception
    {
        public RecordNotFoundException(string modelName, Guid key) : base($"Failed to find {modelName} (Key={key})")
        {
        }

        public RecordNotFoundException(string modelName, ModelKey key) : base(
            $"Failed to find {modelName} (Key={key.Key} Version={key.Version})")
        {
        }
    }
}