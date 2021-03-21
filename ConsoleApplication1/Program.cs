using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;

namespace ConsoleApplication1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // await Seed();
            // return;

            var customer = new Customer()
            {
                Name = "Matt Burke",
                Orders = new List<Order>()
                {
                    new Order()
                    {
                        LineItems = new List<LineItem>()
                        {
                            new LineItem(){ Product = "Poptarts", ItemCount = 1, UnitPrice = 3.79M },
                            new LineItem(){ Product = "Beer", ItemCount = 2, UnitPrice = 12.99M },
                        }
                    }
                }
            };

            var context = new PlaygroundContext();
            // context.Customers.Add(customer);
            // await context.SaveChangesAsync();

            var c = await context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.LineItems)
                .FirstAsync(c =>
                c.Key == Guid.Parse("254b4adc-7033-48c5-800d-4a776baa1d6e"));

            foreach (var o in c.Orders)
            {
                Console.WriteLine($"{o.OrderType} Order on {o.CreatedAt}");
                foreach (var l in o.LineItems)
                {
                    Console.WriteLine($" - {l.Product}: {l.ItemCount} x ${l.UnitPrice} = ${l.ItemCount * l.UnitPrice}");
                }
            }
        }

        static async Task Seed(int n = 1000)
        {
            for (int i = 0; i < n; i++)
            {
                var context = new PlaygroundContext();
                var customer = new Customer()
                {
                    Name = Faker.Name.FullName(),
                    Orders = new List<Order>()
                };

                var orderCount = Faker.RandomNumber.Next(1, 3);
                for (int j = 0; j < orderCount; j++)
                {
                    var order = new Order()
                    {
                        OrderType = Faker.Enum.Random<OrderType>(),
                        LineItems = new List<LineItem>()
                    };
                    customer.Orders.Add(order);

                    var lineCount = Faker.RandomNumber.Next(1, 10);
                    for (int k = 0; k < lineCount; k++)
                    {
                        var line = new LineItem()
                        {
                            Product = Faker.Company.Name(),
                            ItemCount = Faker.RandomNumber.Next(1, 5),
                            UnitPrice = Faker.RandomNumber.Next(100, 100_000) / 100.00M
                        };
                        order.LineItems.Add(line);
                    }
                }

                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }
        }
    }

    public class PlaygroundContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<LineItem> LineItems { get; set; }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql("Host=localhost;Database=playground;Username=postgres;Password=LocalDev123",
                    o => o.UseNodaTime())
                .UseSnakeCaseNamingConvention()
                .EnableSensitiveDataLogging()
                .LogTo(s =>
                {
                    var previous = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(s);
                    Console.ForegroundColor = previous;
                });

            base.OnConfiguring(optionsBuilder);
        }

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

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
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
                    model.Version++;
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
    }

    public class ModelBase
    {
        public int Id { get; set; }
        public Guid Key { get; set; } = Guid.NewGuid();
        public int Version { get; set; } = 1;

        public Instant CreatedAt { get; set; }
        public Instant UpdatedAt { get; set; }
        public Instant? DeletedAt { get; set; }
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
