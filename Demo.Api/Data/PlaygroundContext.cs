using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;

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
            entity.Property(e => e.Key).IsRequired();
            entity.Property(e => e.Version).IsConcurrencyToken();
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
}
