using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using ConsoleApplication1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;

namespace ConsoleApplication1.Api
{
    internal static class AutoMapperExtensions
    {
        public static IMappingExpression<TModel, TMessage> IncludeModelKey<TModel, TMessage>(this IMappingExpression<TModel, TMessage> exp) where TModel: ModelBase where TMessage : MessageBase<TMessage>
        {
            return exp.ForMember(d => d.ModelKey,
                x => x.MapFrom(s => new ModelKey<TMessage>
                {
                    Key = s.Key,
                    Version = s.Version
                }));
        }
    }
    public class ApiService
    {
        private PlaygroundContext _context;
        private IMapper _mapper;
        private MapperConfiguration _config;

        public ApiService()
        {
            _context = new PlaygroundContext();
            _config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Customer, CustomerResponse>()
                    .IncludeModelKey();

                cfg.CreateMap<Order, OrderResponse>()
                    .IncludeModelKey();

                cfg.CreateMap<LineItem, LineItemResponse>()
                    .IncludeModelKey();
            });
            _config.AssertConfigurationIsValid();
            _config.CompileMappings();
            _mapper = _config.CreateMapper();
        }

        public async Task<CustomerResponse> GetCustomer(Guid key)
        {
            var customer = await _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.LineItems)
                .ProjectTo<CustomerResponse>(_config)
                .FirstOrDefaultAsync(c => c.ModelKey.Key == key);

            return customer;
        }
    }

    public struct ModelKey<TModel>
    {
        public Guid Key { get; init; }
        public int Version { get; init; }

        public bool Equals(ModelKey<TModel> other)
        {
            return Key.Equals(other.Key) && Version == other.Version;
        }

        public override bool Equals(object obj)
        {
            return obj is ModelKey<TModel> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Version);
        }
    }

    public class MessageBase<TModel>
    {
        public ModelKey<TModel> ModelKey { get; set; }
    }

    public class CustomerResponse : MessageBase<CustomerResponse>
    {
        public string Name { get; set; }
        public IReadOnlyList<OrderResponse> Orders { get; set; }
    }

    public class OrderResponse : MessageBase<OrderResponse>
    {
        public OrderType OrderType { get; set; }
        public IReadOnlyList<LineItemResponse> LineItems { get; set; }
    }


    public class LineItemResponse : MessageBase<LineItemResponse>
    {
        public string Product { get; set; }
        public int ItemCount { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
