using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Treinapp.Commons.Domain
{
    /// <summary>
    /// A Sport controller by this API.
    /// </summary>
    /// <param name="Id">The sport's unique ID</param>
    /// <param name="Name">The sport's Name</param>
    /// <param name="Description">A short description of the sport</param>
    public record Sport(Guid Id, string Name, string Description)
    {
        [JsonConstructor]
        public Sport(Guid id, string name, string description, IEnumerable<Workout> workouts) : this(id, name, description)
        {
            Workouts = new HashSet<Workout>(workouts) ?? new HashSet<Workout>();
        }

        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
        public IEnumerable<Workout> Workouts { get; private set; }

        public Sport BookWorkout(Workout workout)
        {
            if (workout is null)
            {
                return this;
            }

            return this with
            {
                Workouts = new HashSet<Workout>(Workouts) { workout }
            };
        }

        public Sport UnbookWorkout(Workout workout)
        {
            if (workout is null)
            {
                return this;
            }

            return this with
            {
                Workouts = new HashSet<Workout>(Workouts.Where(w => w.Equals(workout)))
            };
        }

        public Sport UpdateWorkout(Guid workoutId, Func<Workout, Workout> act)
        {
            var existingWorkout = Workouts.SingleOrDefault(w => w.Id.Equals(workoutId));

            if (existingWorkout == null)
                return this;

            var updatedWorkout = act(existingWorkout);
            return this with
            {
                Workouts = new HashSet<Workout>(
                    Workouts
                        .Where(w => !w.Equals(existingWorkout))
                        .Append(updatedWorkout))
            };
        }
    }
}
