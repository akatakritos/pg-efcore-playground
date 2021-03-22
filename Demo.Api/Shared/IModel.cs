using System;

namespace Demo.Api.Shared
{
    public interface IModel
    {
        public Guid Key { get; set; }
        public int Version { get; set; }
    }

    public interface IModelKeyed
    {
        public ModelKey ModelKey { get; set; }
    }
}