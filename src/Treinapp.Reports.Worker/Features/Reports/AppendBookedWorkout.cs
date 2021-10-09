
using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common.Domain;
using Treinapp.Commons.Domain;

namespace Treinapp.Reports.Worker.Features.Reports
{
    /// <summary>
    /// Command to append a booked workout to a report.
    /// </summary>
    public class AppendBookedWorkout : IRequest<Report>
    {
        public Report AppendTo { get; init; }
        public Workout Append { get; init; }
    }

    public class AppendBookedWorkoutHandler : IRequestHandler<AppendBookedWorkout, Report>
    {
        private readonly ILogger<AppendBookedWorkoutHandler> logger;
        private readonly IMongoDatabase database;

        public AppendBookedWorkoutHandler(ILogger<AppendBookedWorkoutHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<Report> Handle(AppendBookedWorkout request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Appending a Workout to report {reportId}", request.AppendTo.Id);
            var report = request.AppendTo.WithBookedWorkout(request.Append);
            report = await database
                    .GetReportsCollection()
                    .UpdateAsync(report, cancellationToken);
            return report;
        }
    }
}
