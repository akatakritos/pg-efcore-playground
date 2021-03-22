using System.Collections.Generic;
using System.Threading.Tasks;
using Demo.Api.Data;
using Faker;
using Microsoft.EntityFrameworkCore;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // await Seed();
            // return;

            // context.Customers.Add(customer);
            // await context.SaveChangesAsync();
            await Task.Delay(1000);
        }

        private static async Task Seed(int n = 1000)
        {
            for (var i = 0; i < n; i++)
            {
                var context = new PlaygroundContext(new DbContextOptionsBuilder<PlaygroundContext>()
                    .UseNpgsql("Host=localhost;Database=playground;Username=postgres;Password=LocalDev123",
                        o => o.UseNodaTime())
                    .UseSnakeCaseNamingConvention()
                    .EnableSensitiveDataLogging().Options);
                var customer = new Customer
                {
                    Name = Name.FullName(),
                    Orders = new List<Order>()
                };

                var orderCount = RandomNumber.Next(1, 3);
                for (var j = 0; j < orderCount; j++)
                {
                    var order = new Order
                    {
                        OrderType = Enum.Random<OrderType>(),
                        LineItems = new List<LineItem>()
                    };
                    customer.Orders.Add(order);

                    var lineCount = RandomNumber.Next(1, 10);
                    for (var k = 0; k < lineCount; k++)
                    {
                        var line = new LineItem
                        {
                            Product = Company.Name(),
                            ItemCount = RandomNumber.Next(1, 5),
                            UnitPrice = RandomNumber.Next(100, 100_000) / 100.00M
                        };
                        order.LineItems.Add(line);
                    }
                }

                context.Customers.Add(customer);
                await context.SaveChangesAsync();
            }
        }
    }
}