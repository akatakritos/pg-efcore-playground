using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Demo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NodaTime;
using NodaTime.TimeZones;

namespace ConsoleApplication1
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // await Seed();
            // return;

            // context.Customers.Add(customer);
            // await context.SaveChangesAsync();
            await Task.Delay(1000);
        }

        static async Task Seed(int n = 1000)
        {
            for (int i = 0; i < n; i++)
            {
                var context = new PlaygroundContext(new DbContextOptionsBuilder<PlaygroundContext>()
                    .UseNpgsql("Host=localhost;Database=playground;Username=postgres;Password=LocalDev123",
                        o => o.UseNodaTime())
                    .UseSnakeCaseNamingConvention()
                    .EnableSensitiveDataLogging().Options);
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

}
