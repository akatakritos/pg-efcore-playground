using System;
using Demo.Api.Shared;
using NodaTime;

namespace Demo.Api.Data
{
    public class ModelBase : IModel
    {
        private readonly int _id;
        public int Id => _id;

        private Instant _createdAt;
        public Instant CreatedAt => _createdAt;

        private Instant _updatedAt;
        public Instant UpdatedAt => _updatedAt;

        private Instant? _deletedAt;
        public Instant? DeletedAt => _deletedAt;

        private readonly Guid _key = Guid.NewGuid();

        public Guid Key => _key;

        private int _version = 1;
        public int Version => _version;

        public void IncrementVersion() => _version++;

        public void MarkUpdated()
        {
            _updatedAt = SystemClock.Instance.GetCurrentInstant();
            _version++;
        }

        public void MarkCreated()
        {
            _createdAt = SystemClock.Instance.GetCurrentInstant();
            _updatedAt = _createdAt;
            _version = 1;
        }

        public virtual void SoftDelete()
        {
            _deletedAt = SystemClock.Instance.GetCurrentInstant();
            MarkUpdated();
        }

        // DDD domain models are considered equal if they have the same id
        protected bool Equals(ModelBase other)
        {
            return _key.Equals(other._key);
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((ModelBase) obj);
        }

        public override int GetHashCode()
        {
            return _key.GetHashCode();
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
