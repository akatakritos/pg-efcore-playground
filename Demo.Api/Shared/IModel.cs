using System;
using AutoMapper;
using Demo.Api.Data;

namespace Demo.Api.Shared
{
    public interface IModel
    {
        public Guid Key { get; }
        public int Version { get; }
    }

    public record ModelUpdateIdentifier(Guid Key, int Version)
    {
        public bool Matches(IModel model) => model.Key == Key && model.Version == Version;

        public ModelUpdateIdentifier(IModel from): this(from.Key, from.Version)
        {
        }
    }

}
