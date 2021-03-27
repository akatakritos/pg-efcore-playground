using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Domain;
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

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Recipe>(entity =>
            {
                ConfigureBaseModel(entity);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Description);

                entity.HasMany(e => e.RecipeIngredients)
                    .WithOne(o => o.Recipe)
                    .HasForeignKey(o => o.RecipeId);
            });

            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                ConfigureBaseModel(entity);

                entity.Property(e => e.RecipeId);
                entity.HasOne(e => e.Recipe)
                    .WithMany(r => r.RecipeIngredients)
                    .HasForeignKey(r => r.RecipeId);

                entity.Property(o => o.UnitOfMeasure).IsRequired()
                    .HasConversion<int>()
                    .HasColumnName("unit_of_measure_id");

                entity.Property(e => e.IngredientId);
                entity.HasOne(e => e.Ingredient)
                    .WithMany()
                    .HasForeignKey(e => e.IngredientId);

                entity.Property(e => e.Quantity);

            });

            modelBuilder.Entity<Ingredient>(entity =>
            {
                ConfigureBaseModel(entity);

                entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
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

    public record ModelId(Guid Key, int Version)
    {

    }

    public class ModelBase : IModel
    {

        public int Id { get; set; }

        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
        public Instant? DeletedAt { get; set; }
        public Guid Key { get; set; } = Guid.NewGuid();
        public int Version { get; set; } = 1;

        // DDD domain models are considered equal if they have the same id
        protected bool Equals(ModelBase other)
        {
            return Id == other.Id;
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
            return Id;
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
