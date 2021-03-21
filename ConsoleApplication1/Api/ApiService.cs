using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Demo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using NodaTime;

namespace ConsoleApplication1.Api
{
    internal static class AutoMapperExtensions
    {
        public static IMappingExpression<TModel, TMessage> IncludeModelKey<TModel, TMessage>(this IMappingExpression<TModel, TMessage> exp) where TModel: ModelBase where TMessage : MessageBase<TModel>
        {
            return exp.ForMember(d => d.ModelKey,
                x => x.MapFrom(s => new ModelKey<TModel>
                {
                    Key = s.Key,
                    Version = s.Version
                }));
        }

        public static IMappingExpression<TModel, TMessage>
            IncludeNavigatedModelKey<TModel, TMessage, TNavigatedModel>(this IMappingExpression<TModel, TMessage> exp,
                Expression<Func<TMessage, ModelKey<TNavigatedModel>>> targetProperty,
                Expression<Func<TModel, TNavigatedModel>> sourceProperty)
            where TModel : ModelBase where TMessage : MessageBase<TModel> where TNavigatedModel : ModelBase
        {
            var targetModelKeyType = typeof(ModelKey<TNavigatedModel>);
            var getKeyExpression = Expression.Property(sourceProperty.Body, "Key");
            var getVersionExpression = Expression.Property(sourceProperty.Body, "Id");

            var ctor = targetModelKeyType.GetConstructors().First();
            var init = Expression.MemberInit(Expression.New(ctor, Array.Empty<Expression>()),
                Expression.Bind(targetModelKeyType.GetProperty("Key"), getKeyExpression),
                Expression.Bind(targetModelKeyType.GetProperty("Version"), getVersionExpression)
            );

            Expression<Func<TModel, ModelKey<TNavigatedModel>>> modelKey =
                Expression.Lambda<Func<TModel, ModelKey<TNavigatedModel>>>(init, sourceProperty.Parameters);

            return exp.ForMember(targetProperty, x => x.MapFrom(modelKey));

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
                    .IncludeNavigatedModelKey(d => d.CustomerModelKey, o => o.Customer)
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
                .Where(c => c.Key == key)
                .Include(c => c.Orders)
                .ThenInclude(o => o.LineItems)
                .ProjectTo<CustomerResponse>(_config)
                .FirstOrDefaultAsync();

            return customer;
        }
    }

    public class ModelKey<TModel>
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

        public override string ToString()
        {
            return $"{Key};{Version}";
        }
    }

    public class MessageBase<TModel>
    {
        public ModelKey<TModel> ModelKey { get; set; }
    }

    public class CustomerResponse : MessageBase<Customer>
    {
        public string Name { get; set; }
        public IReadOnlyList<OrderResponse> Orders { get; set; }
    }

    public class OrderResponse : MessageBase<Order>
    {
        public ModelKey<Customer> CustomerModelKey { get; set; }
        public OrderType OrderType { get; set; }
        public IReadOnlyList<LineItemResponse> LineItems { get; set; }
    }


    public class LineItemResponse : MessageBase<LineItem>
    {
        public string Product { get; set; }
        public int ItemCount { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
