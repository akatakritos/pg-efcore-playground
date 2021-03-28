using System;

namespace Demo.Api.Shared
{
    public interface IModel
    {
        public Guid Key { get; }
        public int Version { get; }
    }

    public record ModelUpdateIdentifier(Guid Key, int Version)
    {
        public ModelUpdateIdentifier(IModel from) : this(from.Key, from.Version)
        {
        }

        public bool Matches(IModel model)
        {
            return model.Key == Key && model.Version == Version;
        }
    }
}