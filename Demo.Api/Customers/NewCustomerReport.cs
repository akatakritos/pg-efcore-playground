using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Demo.Api.Data;
using MediatR;
using Microsoft.FSharp.Control;
using NodaTime;

namespace Demo.Api.Customers
{
    public class NewCustomerReportRequest: IRequest<NewCustomerReportResult>
    {
    }

    public class NewCustomerReportResult
    {
        public IList<CustomerReport> Reports { get; set; }
    }

    public class CustomerReport
    {
        public string CustomerName { get; set; }
        public Instant SignedUpAt { get; set; }
        public decimal OrderCount { get; set; }
        public decimal TotalSpend { get; set; }
    }

    public class NewCustomerReportHandler : IRequestHandler<NewCustomerReportRequest, NewCustomerReportResult>
    {
        private readonly IDatabase _db;

        public NewCustomerReportHandler(IDatabase db)
        {
            _db = db;
        }

        public async Task<NewCustomerReportResult> Handle(NewCustomerReportRequest request, CancellationToken cancellationToken)
        {
            var sql = $@"select
            c.name as CustomerName,
            COUNT(distinct o.id) as OrderCount,
            SUM(li.item_count * li.unit_price) as TotalSpend
            from
                customers as c
                join orders o on c.id = o.customer_id
            join line_items li on o.id = li.order_id
            where c.created_at > @start
            group by c.id";

            using var conn = await _db.GetOpenConnection(cancellationToken);
            var start = SystemClock.Instance.GetCurrentInstant().Minus(Duration.FromDays(30));
            var report = await conn.QueryAsync<CustomerReport>(sql, new { start });
            return new NewCustomerReportResult
            {
                Reports = report.ToList()
            };
        }
    }
}
