using MediatR;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.API.Features.Sports;

namespace Treinapp.API.Features.Workouts
{
    /// <summary>
    /// Query to list all Workouts of a sport.
    /// </summary>
    public class ListWorkouts : IRequest<IEnumerable<Workout>>
    {        
        public Guid SportId { get; set; }
    }

    public class ListWorkoutsHander : IRequestHandler<ListWorkouts, IEnumerable<Workout>>
    {
        private readonly ILogger<ListWorkoutsHander> logger;
        private readonly IMongoDatabase database;

        public ListWorkoutsHander(ILogger<ListWorkoutsHander> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<IEnumerable<Workout>> Handle(ListWorkouts request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Listing all workouts for a sport");
            var sport = await database
                .GetSportsCollection()
                .FetchAsync(request.SportId, cancellationToken);
            return sport.Workouts;
        }
    }
}
