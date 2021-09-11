using Bogus;

using MediatR;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Commons.Domain;

namespace Treinapp.Spammer.Features
{
    public class SportBogusGenerator : Faker<CreateSportPayload>
    {
        public SportBogusGenerator()
        {
            RuleFor(p => p.Name, f => f.Lorem.Sentence());
            RuleFor(p => p.Description, f => f.Rant.Review());
        }
    }

    public class CreateSport : IRequest
    {
        public CreateSportPayload Payload { get; set; }
    }

    public class CreateSportHandler : IRequestHandler<CreateSport, Unit>
    {
        private readonly ILogger<CreateSportHandler> logger;
        private readonly ICollection<Sport> sports;
        private readonly ITreinappApi api;

        public CreateSportHandler(ILogger<CreateSportHandler> logger,
            ICollection<Sport> sports,
            ITreinappApi api)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.sports = sports ?? throw new ArgumentNullException(nameof(sports));
            this.api = api ?? throw new ArgumentNullException(nameof(api));
        }
        public async Task<Unit> Handle(CreateSport request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Creating a new sport");
            try
            {
                var result = await api.CreateNew(request.Payload, cancellationToken);
                if (result is not null)
                {
                    sports.Add(result);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error while hitting the API");                
            }

            return Unit.Value;
        }
    }
}
