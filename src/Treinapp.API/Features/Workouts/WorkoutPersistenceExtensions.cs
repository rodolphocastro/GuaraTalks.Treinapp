using MongoDB.Bson.Serialization.Attributes;

using System;

namespace Treinapp.API.Features.Workouts
{
    public class WorkoutPersistence
    {
        public Guid Id { get; set; } = Guid.NewGuid();        
        public DateTimeOffset BookedAt { get; set; }
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }

        public Workout ToWorkout()
        {
            return new(Id, BookedAt, StartedAt, FinishedAt);
        }
    }

    public static class WorkoutPersistenceExtensions
    {
        public static WorkoutPersistence ToPersistence(this Workout workout)
        {
            return new()
            {
                Id = workout.Id,
                BookedAt = workout.BookedAt,
                FinishedAt = workout.FinishedAt,
                StartedAt = workout.StartedAt
            };
        }
    }
}
