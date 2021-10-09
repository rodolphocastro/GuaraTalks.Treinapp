using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common.Domain;
using Treinapp.Commons.Domain;

namespace Treinapp.Reports.Worker.Features.Reports
{
    /// <summary>
    /// Command to append a created sport into a report.
    /// </summary>
    public class AppendCreatedSport : IRequest<Report>
    {
        public Report AppendTo { get; init; }
        public Sport Append { get; init; }
    }

    public class AppendCreatedSportHandler : IRequestHandler<AppendCreatedSport, Report>
    {
        private readonly ILogger<AppendCreatedSportHandler> logger;
        private readonly IMongoDatabase database;

        public AppendCreatedSportHandler(ILogger<AppendCreatedSportHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public async Task<Report> Handle(AppendCreatedSport request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Appending a new sport to report {reportId}", request.AppendTo.Id);
            var report = request.AppendTo.WithCreatedSport(request.Append);
            report = await database
                .GetReportsCollection()
                .UpdateAsync(report, cancellationToken);
            return report;
        }
    }
}
