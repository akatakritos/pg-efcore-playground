using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Demo.Api.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

namespace Demo.Api.Data
{
    public class PlaygroundContext : DbContext
    {
        private readonly IDomainEventDispatcher _dispatcher;
        private IDbContextTransaction? _currentTransaction;

        public PlaygroundContext(IDomainEventDispatcher dispatcher, DbContextOptions<PlaygroundContext> options) : base(options)
        {
            _dispatcher = dispatcher;
        }

        public PlaygroundContext(DbContextOptions<PlaygroundContext> options) : this(new NullDispatcher(), options)
        {
        }

        public DbSet<Recipe> Recipes => Set<Recipe>();
        public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();

        public async Task<Recipe> GetRecipeForUpdate(ModelUpdateIdentifier identifier,
                                                     CancellationToken cancellationToken = default)
        {
            return await Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.Key == identifier.Key && r.Version == identifier.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Recipe> GetRecipe(Guid key, CancellationToken cancellationToken = default)
        {
            return await Recipes
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .Where(r => r.Key == key)
                .FirstOrDefaultAsync(cancellationToken);
        }

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
                    .HasForeignKey("RecipeId");
            });

            modelBuilder.Entity<RecipeIngredient>(entity =>
            {
                ConfigureBaseModel(entity);

                entity.Property<long>("RecipeId");
                entity.HasOne(e => e.Recipe)
                    .WithMany(r => r.RecipeIngredients)
                    .HasForeignKey("RecipeId");

                entity.Property(o => o.UnitOfMeasure).IsRequired()
                    .HasConversion(
                        enm => enm.Value,
                        id => UnitOfMeasure.FromValue(id))
                    .HasColumnName("unit_of_measure_id");

                entity.Property<long>("IngredientId");
                entity.HasOne(e => e.Ingredient)
                    .WithMany()
                    .HasForeignKey("IngredientId");
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
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Version).IsConcurrencyToken();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.DeletedAt);
            entity.HasQueryFilter(e => e.DeletedAt == null);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
        {
            // expensive to do three times, do once and switch on state
            var added = ChangeTracker.Entries().Where(e => e.State == EntityState.Added);
            var modified = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
            var deleted = ChangeTracker.Entries().Where(e => e.State == EntityState.Deleted);

            foreach (var entity in added)
            {
                if (entity.Entity is ModelBase model)
                {
                    model.MarkCreated();
                }
            }

            foreach (var entity in modified)
            {
                if (entity.Entity is ModelBase model)
                {
                    model.MarkUpdated();
                }
            }

            foreach (var entity in deleted)
            {
                if (entity.Entity is ModelBase model)
                {
                    model.SoftDelete();
                }
            }

            var changes = await base.SaveChangesAsync(cancellationToken);

            foreach (var entity in ChangeTracker.Entries())
            {
                if (entity.Entity is AggregateRoot root)
                {
                    foreach (var @event in root.QueuedEvents)
                    {
                        await _dispatcher.DispatchAsync(@event);
                    }

                    root.ClearEvents();
                }
            }

            return changes;
        }


        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction =
                await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted).ConfigureAwait(false);
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
}
