using Refit;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Commons.Domain;

namespace Treinapp.Spammer
{
    public class CreateSportPayload
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class BookWorkoutPayload
    {
        public Guid SportId { get; set; }

        public DateTimeOffset BookAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public interface ITreinappApi
    {
        [Post("/sports")]
        Task<Sport> CreateNew([Body] CreateSportPayload command, CancellationToken cancellationToken = default);

        [Post("/workouts")]
        Task<Workout> BookNew([Body] BookWorkoutPayload command, CancellationToken cancellationToken = default);
    }
}
