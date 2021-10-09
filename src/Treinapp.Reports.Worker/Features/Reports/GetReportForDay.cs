using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common.Domain;

namespace Treinapp.Reports.Worker.Features.Reports
{
    /// <summary>
    /// Query to fetch a report for a given day.
    /// </summary>
    public class GetReportForDay : IRequest<Report>
    {
        public DateTimeOffset ForDay { get; init; } = DateTimeOffset.UtcNow;
    }

    public class GetReportForDayHandler : IRequestHandler<GetReportForDay, Report>
    {
        private readonly ILogger<GetReportForDayHandler> logger;
        private readonly IMongoDatabase database;

        public GetReportForDayHandler(ILogger<GetReportForDayHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Task<Report> Handle(GetReportForDay request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Fetching a report for {givenDay}", request.ForDay);
            return database
                    .GetReportsCollection()
                    .FetchAsync(request.ForDay, cancellationToken);
        }
    }
}
