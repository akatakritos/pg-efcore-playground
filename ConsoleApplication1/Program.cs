using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using ConsoleApplication1.Api;
using Demo.Api.Data;
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

            var context = new PlaygroundContext();
            // context.Customers.Add(customer);
            // await context.SaveChangesAsync();

            var service = new ApiService();
            var c = await service.GetCustomer(Guid.Parse("254b4adc-7033-48c5-800d-4a776baa1d6e"));


            foreach (var o in c.Orders)
            {
                Console.WriteLine($"{o.OrderType} Order (for {o.CustomerModelKey}");
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

}
