using Autofac;
using Microsoft.Extensions.Configuration;

namespace Demo.Api.Infrastructure.Indexing
{
    public class IndexingModule: Module
    {
        private readonly IConfiguration _configuration;

        public IndexingModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<RecipeIndexer>().AsSelf();
            builder.RegisterType<RecipeIndexSearcher>().AsSelf();
            builder.Register(ctx => new SharedLuceneWriter(_configuration["Indexing:RootPath"]))
                .SingleInstance();
        }
    }
}
