using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Demo.Api.Data;

namespace Demo.Api.Shared
{
    internal static class AutoMapperExtensions
    {
        public static IMappingExpression<TModel, TMessage> IncludeModelKey<TModel, TMessage>(this IMappingExpression<TModel, TMessage> exp) where TModel:

        ModelBase where TMessage : IModelKeyed
        {
            return exp.ForMember(d => d.ModelKey,
                x => x.MapFrom(s => new ModelKey
                {
                    Key = s.Key,
                    Version = s.Version
                }));
        }

        public static IMappingExpression<TModel, TMessage>
            IncludeNavigatedModelKey<TModel, TMessage, TNavigatedModel>(this IMappingExpression<TModel, TMessage> exp,
                Expression<Func<TMessage, ModelKey>> targetProperty,
                Expression<Func<TModel, TNavigatedModel>> sourceProperty)
            where TModel : ModelBase where TMessage : IModelKeyed where TNavigatedModel : ModelBase
        {
            var targetModelKeyType = typeof(ModelKey);
            var getKeyExpression = Expression.Property(sourceProperty.Body, "Key");
            var getVersionExpression = Expression.Property(sourceProperty.Body, "Id");

            var ctor = targetModelKeyType.GetConstructors().First();
            var init = Expression.MemberInit(Expression.New(ctor, Array.Empty<Expression>()),
                Expression.Bind(targetModelKeyType.GetProperty("Key"), getKeyExpression),
                Expression.Bind(targetModelKeyType.GetProperty("Version"), getVersionExpression)
            );

            Expression<Func<TModel, ModelKey>> modelKey =
                Expression.Lambda<Func<TModel, ModelKey>>(init, sourceProperty.Parameters);

            return exp.ForMember(targetProperty, x => x.MapFrom(modelKey));

        }
    }
}
