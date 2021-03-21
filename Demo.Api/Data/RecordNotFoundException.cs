using System;

namespace Demo.Api.Data
{
    public class RecordNotFoundException<TModel> : Exception
    {
        public RecordNotFoundException(Guid key) : base($"Failed to find ${typeof(TModel).Name} (Key={key})")
        {
        }
    }
}
