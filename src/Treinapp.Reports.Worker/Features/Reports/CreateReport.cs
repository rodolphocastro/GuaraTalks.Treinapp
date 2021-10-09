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
    /// Command to create a new Report for a given day.
    /// </summary>
    public class CreateReport : IRequest<Report>
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTimeOffset ForDay { get; init; } = DateTimeOffset.Now;
    }

    public class CreateReportHandler : IRequestHandler<CreateReport, Report>
    {
        private readonly ILogger<CreateReportHandler> logger;
        private readonly IMongoDatabase database;

        public CreateReportHandler(ILogger<CreateReportHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }
        public async Task<Report> Handle(CreateReport request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Creating a new report for {givenDay}", request.ForDay);
            var result = await database
                    .GetReportsCollection()
                    .InsertNewAsync(new Report(Guid.NewGuid()), cancellationToken);

            return result;
        }
    }
}
