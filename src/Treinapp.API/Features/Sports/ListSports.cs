using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Features.Sports
{
    /// <summary>
    /// A query to list all sports available in the system.
    /// </summary>
    public class ListSports : IRequest<IEnumerable<Sport>>
    {
    }

    public class ListSportHandler : IRequestHandler<ListSports, IEnumerable<Sport>>
    {
        private readonly ILogger<ListSportHandler> logger;
        private readonly IMongoDatabase database;

        public ListSportHandler(ILogger<ListSportHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.database = database ?? throw new System.ArgumentNullException(nameof(database));
        }

        public async Task<IEnumerable<Sport>> Handle(ListSports request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Listing all sports");
            var collection = database.GetSportsCollection();
            var mongoResults = await collection.Find(_ => true).ToListAsync(cancellationToken);
            return mongoResults.Select(r => r.ToSport());
        }
    }
}
