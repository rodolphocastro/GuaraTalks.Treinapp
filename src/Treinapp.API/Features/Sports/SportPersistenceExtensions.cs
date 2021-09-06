using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.API.Features.Workouts;

namespace Treinapp.API.Features.Sports
{
    /// <summary>
    /// DAL / POCO for storage.
    /// </summary>
    public class SportPersistence
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<WorkoutPersistence> Workouts { get; set; } = new HashSet<WorkoutPersistence>();

        /// <summary>
        /// Creates a Sport from this POCO.
        /// </summary>
        /// <returns></returns>
        public Sport ToSport() => new(Id, Name, Description, Workouts.Select(w => w.ToWorkout()));
    }

    /// <summary>
    /// Extensions related to storing Sports (the Domain Entity) on MongoDb.
    /// </summary>
    public static class SportPersistenceExtensions
    {
        /// <summary>
        /// The collection name within Mongo.
        /// </summary>
        private const string SportsCollectionName = "sports";

        /// <summary>
        /// Creates a DAL / POCO from a Sport.
        /// </summary>
        /// <param name="sport"></param>
        /// <returns></returns>
        public static SportPersistence ToPersistence(this Sport sport)
        {
            return new()
            {
                Id = sport.Id,
                Description = sport.Description,
                Name = sport.Name,
                Workouts = sport.Workouts.Select(w => w.ToPersistence())
            };
        }

        /// <summary>
        /// Gets the collection, within Mongo, that stores Sports.
        /// </summary>
        /// <param name="mongoDb"></param>
        /// <returns></returns>
        public static IMongoCollection<SportPersistence> GetSportsCollection(this IMongoDatabase mongoDb) => mongoDb.GetCollection<SportPersistence>(SportsCollectionName);

        /// <summary>
        /// Inserts a new Sport into the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="sport"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Sport> InsertNewAsync(this IMongoCollection<SportPersistence> collection, SportPersistence sport, CancellationToken cancellationToken = default)
        {
            await collection.InsertOneAsync(sport, null, cancellationToken);
            return sport.ToSport();
        }

        /// <summary>
        /// Retrieves all Sports from the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<Sport>> GetAllAsync(this IMongoCollection<SportPersistence> collection, CancellationToken cancellationToken = default)
        {
            var mongoResults = await collection.Find(_ => true).ToListAsync(cancellationToken);
            return mongoResults.Select(r => r.ToSport());
        }
    }
}
