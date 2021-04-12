using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Util;
using Serilog;
using StackExchange.Profiling;

namespace Demo.Api.Infrastructure.Indexing
{
    public record SearchResult(int TotalHits, IList<RecipeSearchResult> Results);

    public record RecipeSearchResult(Guid Key, string Name, string Description, string IngredientNames);

    public class RecipeIndexSearcher
    {
        private static readonly ILogger _log = Log.ForContext<RecipeIndexSearcher>();
        private readonly SharedLuceneWriter _writer;

        public RecipeIndexSearcher(SharedLuceneWriter writer)
        {
            _writer = writer;
        }

        private IEnumerable<string> TokenizeSearch(string search)
        {
            var analyser = new StandardAnalyzer(LuceneVersion.LUCENE_48);
            var tokens = analyser.GetTokenStream(null, search);
            var attr = tokens.GetAttribute<ICharTermAttribute>();
            tokens.Reset();
            while (tokens.IncrementToken())
            {
                yield return attr.ToString();
            }
        }

        public SearchResult Search(string search, int skip = 0, int take = 25)
        {
            using var reader = _writer.Writer.GetReader(true);
            var searcher = new IndexSearcher(reader);
            var phrase = new BooleanQuery();

            foreach (var token in TokenizeSearch(search))
            {
                var nameQuery = new TermQuery(new Term("name", token));
                nameQuery.Boost = 2.0f;

                var descriptionQuery = new TermQuery(new Term("description", token));
                descriptionQuery.Boost = 1.5f;

                var ingredientQuery = new TermQuery(new Term("ingredient_names", token));

                phrase.Add(nameQuery, Occur.SHOULD);
                phrase.Add(descriptionQuery, Occur.SHOULD);
                phrase.Add(ingredientQuery, Occur.SHOULD);
            }

            _log.Information("Executing index search {Query}", phrase);

            using (MiniProfiler.Current.Step("Search Lucene"))
            {
                var hits = searcher.Search(phrase, skip + take);
                _log.Debug("Found {TotalHits} matching docs", hits.TotalHits);
                return new SearchResult(
                    TotalHits: hits.TotalHits,
                    Results: hits.ScoreDocs.Skip(skip).Take(take).Select(hit =>
                    {
                        var doc = searcher.Doc(hit.Doc);
                        return new RecipeSearchResult(
                            Key: Guid.Parse(doc.Get("key")),
                            Name: doc.Get("name"),
                            Description: doc.Get("description"),
                            IngredientNames: doc.Get("ingredient_names")
                        );
                    }).ToList());
            }
        }
    }
}
