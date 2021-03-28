using System;
using System.Linq;
using System.Linq.Expressions;
using System.Xml;
using AutoMapper;
using Demo.Api.Data;
using Demo.Api.Recipes;

namespace Demo.Api.Shared
{

    internal static class AutoMapperExtensions
    {
        public static IMappingExpression<TModel, TMessage> IncludeModelId<TModel, TMessage>(
            this IMappingExpression<TModel, TMessage> exp) where TModel :
            ModelBase
            where TMessage : ModelResponseBase
        {
            return exp.ForMember(d => d.ModelKey, x => x.MapFrom(s => new ModelUpdateIdentifier(s)));
        }

        // public static IMappingExpression<TModel, TMessage>
        //     IncludeNavigatedModelId<TModel, TMessage, TNavigatedModel>(this IMappingExpression<TModel, TMessage> exp,
        //         Expression<Func<TMessage, ModelKey>> targetProperty,
        //         Expression<Func<TModel, TNavigatedModel>> sourceProperty)
        //     where TModel : ModelBase where TMessage : IModelIded where TNavigatedModel : ModelBase
        // {
        //     var targetModelIdType = typeof(ModelKey);
        //     var getKeyExpression = Expression.Property(sourceProperty.Body, "Key");
        //     var getVersionExpression = Expression.Property(sourceProperty.Body, "Id");
        //
        //     var ctor = targetModelIdType.GetConstructors().First();
        //     var init = Expression.MemberInit(Expression.New(ctor, Array.Empty<Expression>()),
        //         Expression.Bind(targetModelIdType.GetProperty("Key"), getKeyExpression),
        //         Expression.Bind(targetModelIdType.GetProperty("Version"), getVersionExpression)
        //     );
        //
        //     var ModelId =
        //         Expression.Lambda<Func<TModel, ModelKey>>(init, sourceProperty.Parameters);
        //
        //     return exp.ForMember(targetProperty, x => x.MapFrom(ModelId));
        // }

        public static IMappingExpression<TCmd, TModel> IgnoreUneditableModelFields<TCmd, TModel>(
            this IMappingExpression<TCmd, TModel> exp) where TModel : ModelBase
        {
            return exp
                .ForMember(d => d.Id, x => x.Ignore())
                .ForMember(d => d.CreatedAt, x => x.Ignore())
                .ForMember(d => d.UpdatedAt, x => x.Ignore())
                .ForMember(d => d.DeletedAt, x => x.Ignore())
                .ForMember(d => d.Key, x => x.Ignore())
                .ForMember(d => d.Version, x => x.Ignore());
        }
    }
}
