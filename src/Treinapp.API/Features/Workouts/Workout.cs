using System;

namespace Treinapp.API.Features.Workouts
{
    /// <summary>
    /// A Workout controlled by this API,
    /// "Creating" a Workout is "Booking" it as well.
    /// A workout exists within a Sport, never on its own.
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="BookedAt"></param>
    /// <param name="StartedAt"></param>
    /// <param name="FinishedAt"></param>
    public record Workout(Guid Id, DateTimeOffset BookedAt, DateTimeOffset? StartedAt = null, DateTimeOffset? FinishedAt = null)
    {
        /// <summary>
        /// Whatever a Workout has been started.
        /// </summary>
        public bool IsStarted => StartedAt.HasValue;

        /// <summary>
        /// Whatever a Workout has been finished.
        /// </summary>
        public bool IsFinished => FinishedAt.HasValue;

        /// <summary>
        /// Starts a Workout session.
        /// </summary>
        /// <returns></returns>
        public Workout Start(DateTimeOffset? startedAt) => this with { StartedAt = startedAt ?? DateTimeOffset.UtcNow };

        /// <summary>
        /// Finishes a Workout session.
        /// </summary>
        /// <returns></returns>
        public Workout Finish(DateTimeOffset? finishedAt) => this with { FinishedAt = finishedAt ?? DateTimeOffset.UtcNow };
    }
}
