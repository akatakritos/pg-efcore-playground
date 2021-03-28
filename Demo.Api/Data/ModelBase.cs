using System;
using Demo.Api.Shared;
using NodaTime;

namespace Demo.Api.Data
{
    public class ModelBase : IModel
    {
        public int Id { get; }

        public Instant CreatedAt { get; private set; }

        public Instant UpdatedAt { get; private set; }

        public Instant? DeletedAt { get; private set; }

        public Guid Key { get; } = Guid.NewGuid();

        public int Version { get; private set; } = 1;

        public void IncrementVersion()
        {
            Version++;
        }

        public void MarkUpdated()
        {
            UpdatedAt = SystemClock.Instance.GetCurrentInstant();
            IncrementVersion();
        }

        public void MarkCreated()
        {
            CreatedAt = SystemClock.Instance.GetCurrentInstant();
            UpdatedAt = CreatedAt;
            Version = 1;
        }

        public virtual void SoftDelete()
        {
            DeletedAt = SystemClock.Instance.GetCurrentInstant();
            MarkUpdated();
        }

        // DDD domain models are considered equal if they have the same identifier.
        // in our design, the key is a better domain identifier than the database PK, since we never
        // expose the PK
        protected bool Equals(ModelBase other)
        {
            return Key.Equals(other.Key);
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

            return Equals((ModelBase) obj);
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public static bool operator ==(ModelBase left, ModelBase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelBase left, ModelBase right)
        {
            return !Equals(left, right);
        }
    }
}