using MediatR;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Features.Sports
{
    /// <summary>
    /// Command to create a new Sport in the system.
    /// </summary>
    public class CreateSport : IRequest<Sport>
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; init; }

        [Required(AllowEmptyStrings = false)]
        public string Description { get; init; }
    }

    /// <summary>
    /// Handler for the CreateSport command.
    /// </summary>
    public class CreateSportHandler : IRequestHandler<CreateSport, Sport>
    {
        private readonly ILogger<CreateSportHandler> logger;
        private readonly IMongoDatabase database;

        public CreateSportHandler(ILogger<CreateSportHandler> logger, IMongoDatabase database)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public async Task<Sport> Handle(CreateSport request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Creating a new sport");
            var sportPoco = new SportPersistenceExtensions.SportPersistence
            {
                Description = request.Description,
                Name = request.Name
            };
            await database
                .GetSportsCollection()
                .InsertNewAsync(sportPoco, cancellationToken);
            return sportPoco.ToSport();
        }
    }
}
