using System;
using System.Collections.Generic;
using System.Linq;

using Treinapp.Commons.Domain;

namespace Treinapp.Common.Domain
{
    public record ReportedSport : Sport
    {
        public ReportedSport(Sport original, DateTimeOffset reportedAt) : base(original)
        {
            ReportedAt = reportedAt;
        }

        public DateTimeOffset ReportedAt { get; init; }
    }

    public record Report
    {
        public Report(Guid id,
            IEnumerable<ReportedSport> createdSports = null,
            IEnumerable<Workout> bookedWorkouts = null,
            IEnumerable<Workout> startedWorkouts = null,
            IEnumerable<Workout> finishedWorkouts = null,
            DateTimeOffset? ForDay = null)
        {
            Id = id;
            CreatedSports = createdSports ?? Enumerable.Empty<ReportedSport>();
            BookedWorkouts = bookedWorkouts ?? Enumerable.Empty<Workout>();
            StartedWorkouts = startedWorkouts ?? Enumerable.Empty<Workout>();
            FinishedWorkouts = finishedWorkouts ?? Enumerable.Empty<Workout>();
            this.ForDay = ForDay?.Date ?? DateTimeOffset.UtcNow.Date;
        }

        public Guid Id { get; }
        public IEnumerable<ReportedSport> CreatedSports { get; init; }
        public IEnumerable<Workout> BookedWorkouts { get; init; }
        public IEnumerable<Workout> StartedWorkouts { get; init; }
        public IEnumerable<Workout> FinishedWorkouts { get; init; }
        public DateTimeOffset ForDay { get; }

        public Report WithCreatedSport(Sport sport)
        {
            if (sport is null)
            {
                throw new ArgumentNullException(nameof(sport));
            }

            return this with
            {
                CreatedSports = new HashSet<ReportedSport>
                    (CreatedSports
                        .Where(s => !s.Id.Equals(sport.Id))
                    .Append(new ReportedSport(sport, DateTimeOffset.UtcNow)))
            };
        }

        public Report WithBookedWorkout(Workout workout)
        {
            if (workout is null)
            {
                throw new ArgumentNullException(nameof(workout));
            }

            return this with
            {
                BookedWorkouts = BookedWorkouts
                    .Where(w => !w.Id.Equals(workout.Id))
                    .Append(workout)
            };
        }
    }
}
