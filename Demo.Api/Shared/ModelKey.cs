using System;

namespace Demo.Api.Shared
{
    public class ModelKey : IEquatable<ModelKey>
    {
        public Guid Key { get; init; }
        public int Version { get; init; }

        public bool Equals(ModelKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Key.Equals(other.Key) && Version == other.Version;
        }

        public static bool operator ==(ModelKey left, ModelKey right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelKey left, ModelKey right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((ModelKey) obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Version);
        }

        public override string ToString()
        {
            return $"{Key};{Version}";
        }
    }
}