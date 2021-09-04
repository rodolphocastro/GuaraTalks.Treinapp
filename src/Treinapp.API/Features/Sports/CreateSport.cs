using MediatR;

using Microsoft.Extensions.Logging;

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

        public CreateSportHandler(ILogger<CreateSportHandler> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Sport> Handle(CreateSport request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Creating a new sport");
            // TODO: Actually persist
            return Task.FromResult(new Sport(Guid.NewGuid(), request.Name, request.Description));
        }
    }
}
