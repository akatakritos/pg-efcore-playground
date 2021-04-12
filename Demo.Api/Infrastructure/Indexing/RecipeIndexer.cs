using System.Collections.Generic;
using System.Linq;
using Demo.Api.Domain;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Serilog;
using Serilog.Core;

namespace Demo.Api.Infrastructure.Indexing
{
    public class RecipeIndexer
    {
        private static readonly ILogger _log = Log.ForContext<RecipeIndexer>();
        private readonly SharedLuceneWriter _writer;

        public RecipeIndexer(SharedLuceneWriter writer)
        {
            _writer = writer;
        }

        public int IndexRecipes(IEnumerable<Recipe> recipes)
        {
            int count = 0;
            foreach (var r in recipes)
            {
                var doc = CreateDocument(r);
                _writer.Writer.UpdateDocument(new Term("key", r.Key.ToString("N")), doc);
                count++;
            }

            _writer.Writer.Flush(true, true);
            _writer.Writer.Commit();
            return count;
        }

        public void IndexRecipe(Recipe r)
        {
            var doc = CreateDocument(r);
            _writer.Writer.UpdateDocument(new Term("key", r.Key.ToString("N")), doc);
            _writer.Writer.Flush(true, true);
            _writer.Writer.Commit();
            _log.Information("Indexed Recipe {Key}", r.Key);
        }

        private Document CreateDocument(Recipe r)
        {
            return new Document
            {
                new StringField("key", r.Key.ToString("N"), Field.Store.YES),
                new TextField("name", r.Name, Field.Store.YES),
                new TextField("description", r.Description, Field.Store.YES),
                new Int64Field("created_at", r.CreatedAt.ToUnixTimeSeconds(), Field.Store.YES),
                new Int64Field("cook_time", (long) r.CookTime.TotalSeconds, Field.Store.YES),
                new Int64Field("prep_time", (long) r.PrepTime.TotalSeconds, Field.Store.YES),
                new TextField("ingredient_names",
                    string.Join("|", r.RecipeIngredients.Select(ri => ri.Ingredient.Name)), Field.Store.YES)
            };
        }
    }
}
