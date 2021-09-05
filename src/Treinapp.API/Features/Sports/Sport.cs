﻿using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Treinapp.API.Features.Sports
{
    public record Sport(Guid Id, string Name, string Description);

    /// <summary>
    /// Extensions related to storing Sports (the Domain Entity) on MongoDb.
    /// </summary>
    public static class SportPersistanceExtensions
    {
        /// <summary>
        /// The collection name within Mongo.
        /// </summary>
        private const string SportsCollectionName = "sports";

        /// <summary>
        /// DAL / POCO for storage.
        /// </summary>
        public class SportPersistance
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }

            /// <summary>
            /// Creates a Sport from this POCO.
            /// </summary>
            /// <returns></returns>
            public Sport ToSport() => new(Id, Name, Description);

            /// <summary>
            /// Create a DAL / POCO from a Sport.
            /// </summary>
            /// <param name="sport"></param>
            /// <returns></returns>
            public static SportPersistance FromSport(Sport sport) => new()
            {
                Id = sport.Id,
                Description = sport.Description,
                Name = sport.Name
            };
        }

        /// <summary>
        /// Gets the collection, within Mongo, that stores Sports.
        /// </summary>
        /// <param name="mongoDb"></param>
        /// <returns></returns>
        public static IMongoCollection<SportPersistance> GetSportsCollection(this IMongoDatabase mongoDb) => mongoDb.GetCollection<SportPersistance>(SportsCollectionName);

        /// <summary>
        /// Inserts a new Sport into the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="sport"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Sport> InsertNewAsync(this IMongoCollection<SportPersistance> collection, SportPersistance sport, CancellationToken cancellationToken = default)
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
        public static async Task<IEnumerable<Sport>> GetAllAsync(this IMongoCollection<SportPersistance> collection, CancellationToken cancellationToken = default)
        {
            var mongoResults = await collection.Find(_ => true).ToListAsync(cancellationToken);
            return mongoResults.Select(r => r.ToSport());
        }
    }
}