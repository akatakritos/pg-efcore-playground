using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using NodaTime;

namespace Demo.Api.Data
{
    public class PlaygroundContext : DbContext
    {
        private IDbContextTransaction _currentTransaction;

        public PlaygroundContext(DbContextOptions<PlaygroundContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(entity =>
            {
                ConfigureBaseModel(entity);
                entity.Property(e => e.Name).IsRequired();
                entity.HasMany(e => e.Orders)
                    .WithOne(o => o.Customer)
                    .HasForeignKey(o => o.CustomerId);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                ConfigureBaseModel(entity);
                entity.Property(e => e.OrderType).HasConversion<int>().HasColumnName("order_type_id");
                entity.Property(o => o.CustomerId).IsRequired();
                entity.HasOne(o => o.Customer)
                    .WithMany(c => c.Orders)
                    .HasForeignKey(o => o.CustomerId);
                entity.HasMany(o => o.LineItems)
                    .WithOne(l => l.Order)
                    .HasForeignKey(l => l.OrderId);
            });

            modelBuilder.Entity<LineItem>(entity =>
            {
                ConfigureBaseModel(entity);
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.LineItems)
                    .HasForeignKey(e => e.OrderId);
                entity.Property(e => e.Product).IsRequired().HasMaxLength(128);
                entity.Property(e => e.ItemCount).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired();
            });
        }

        private void ConfigureBaseModel<T>(EntityTypeBuilder<T> entity) where T : ModelBase
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Key).IsRequired().ValueGeneratedOnAdd();
            entity.Property(e => e.Version).IsRequired().IsConcurrencyToken();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            entity.HasQueryFilter(e => e.DeletedAt == null);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            // expensive to do three times, do once and switch on state
            var added = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
            var deleted = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted);

            foreach (var entity in added)
            {
                if (entity.Entity is ModelBase model)
                {
                    model.CreatedAt = SystemClock.Instance.GetCurrentInstant();
                    model.UpdatedAt = model.CreatedAt;
                    model.Version = 1;
                    if (model.Key == Guid.Empty)
                    {
                        model.Key = Guid.NewGuid();
                    }
                }
            }

            foreach (var entity in modified)
            {
                if (entity.Entity is ModelBase model)
                {
                    model.UpdatedAt = SystemClock.Instance.GetCurrentInstant();
                    model.Version++;
                }
            }

            foreach (var entity in deleted)
            {
                if (entity.Entity is ModelBase model)
                {
                    entity.State = EntityState.Modified;
                    model.DeletedAt = SystemClock.Instance.GetCurrentInstant();
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await SaveChangesAsync().ConfigureAwait(false);

                _currentTransaction?.Commit();
            }
            catch
            {
                RollbackTransaction();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public void RollbackTransaction()
        {
            try
            {
                _currentTransaction?.Rollback();
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }
    }

    public class ModelBase : IModel
    {
        public int Id { get; set; }

        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
        public Instant? DeletedAt { get; set; }
        public Guid Key { get; set; } = Guid.NewGuid();
        public int Version { get; set; } = 1;
    }

    public class Customer : ModelBase
    {
        public string Name { get; set; }
        public virtual ICollection<Order> Orders { get; set; }
    }

    public enum OrderType
    {
        Normal = 1,
        Employee = 2
    }

    public class Order : ModelBase
    {
        public int CustomerId { get; set; }
        public OrderType OrderType { get; set; }
        public virtual Customer Customer { get; set; }
        public virtual ICollection<LineItem> LineItems { get; set; }
    }

    public class LineItem : ModelBase
    {
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public string Product { get; set; }
        public int ItemCount { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
