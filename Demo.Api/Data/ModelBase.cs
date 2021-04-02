using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Api.Shared;
using NodaTime;
using Serilog;

namespace Demo.Api.Data
{
    public interface IDomainEvent
    {
    }

    public class AggregateRoot : ModelBase
    {
        private List<IDomainEvent> _domainEvents = new List<IDomainEvent>();
        protected void EnqueueDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);
        internal IEnumerable<IDomainEvent> QueuedEvents => _domainEvents;
    }

    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent @event);
    }

    public class NullDispatcher: IDomainEventDispatcher
    {
        private static ILogger _log = Log.ForContext<NullDispatcher>();

        public Task DispatchAsync(IDomainEvent @event)
        {
            _log.Information("Sending domain event {@Event}", @event);
            return Task.CompletedTask;
        }
    }

    public class ModelBase : IModel
    {
        // todo -- do we even need this? Could refer to it by string name in ef setup
        public long Id { get; } = 0; // set by database

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

        public override bool Equals(object? obj)
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

        public static bool operator ==(ModelBase? left, ModelBase? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ModelBase? left, ModelBase? right)
        {
            return !Equals(left, right);
        }
    }
}
