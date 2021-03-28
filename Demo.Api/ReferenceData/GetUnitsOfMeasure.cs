using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Demo.Api.Data;
using Demo.Api.Domain;
using Demo.Api.Infrastructure;
using MediatR;

namespace Demo.Api.ReferenceData
{
    public class GetUnitsOfMeasureRequest : IRequest<List<ReferenceData>>, ICacheableRequest
    {
        public string GetCacheKey()
        {
            return "REFDATA::UNITS_OF_MEASURE";
        }
    }

    public class GetUnitsOfMeasureHandler : IRequestHandler<GetUnitsOfMeasureRequest, List<ReferenceData>>
    {
        private readonly IDatabase _db;

        public GetUnitsOfMeasureHandler(IDatabase db)
        {
            _db = db;
        }

        public async Task<List<ReferenceData>> Handle(GetUnitsOfMeasureRequest request,
                                                      CancellationToken cancellationToken)
        {
            using var connection = await _db.GetOpenConnection(cancellationToken);
            var results =
                await connection.QueryAsync<QueryResult>(@"select ""id"", ""name"" from ""unit_of_measure_lib""");
            return results.Select(r => new ReferenceData(UnitOfMeasure.FromValue(r.Id).Name, r.Name)).ToList();
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class QueryResult
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
        }
    }
}
