using MediatR;

using Microsoft.Extensions.Logging;

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

        public ListSportHandler(ILogger<ListSportHandler> logger)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public Task<IEnumerable<Sport>> Handle(ListSports request, CancellationToken cancellationToken)
        {
            logger.LogTrace("Listing all sports");
            // TODO: Actually fetch from Storage
            return Task.FromResult(Enumerable.Empty<Sport>());
        }
    }
}
