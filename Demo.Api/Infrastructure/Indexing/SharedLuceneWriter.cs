using System;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Demo.Api.Infrastructure.Indexing
{
    public class SharedLuceneWriter
    {
        public string RootPath { get; }

        public SharedLuceneWriter(string rootPath)
        {
            RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
            _writer = new Lazy<IndexWriter>(CreateWriter);
        }

        private readonly Lazy<IndexWriter> _writer;
        public IndexWriter Writer => _writer.Value;

        private IndexWriter CreateWriter()
        {
            var dir = FSDirectory.Open(RootPath);
            var version = LuceneVersion.LUCENE_48;
            var config = new IndexWriterConfig(version, new StandardAnalyzer(version));
            return new IndexWriter(dir, config);
        }
    }
}
