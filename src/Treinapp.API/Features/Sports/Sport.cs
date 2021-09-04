using MongoDB.Driver;

using System;

namespace Treinapp.API.Features.Sports
{
    public record Sport(Guid Id, string Name, string Description)
    {
        Sport Rename(string newName) => this with { Name = newName };

        Sport ChangeDescription(string newDescription) => this with { Description = newDescription };
    }

    public static class SportPersistanceExtensions
    {
        private const string SportsCollectionName = "sports";

        public class SportPersistance
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }

            public Sport ToSport() => new(Id, Name, Description);
            public static SportPersistance FromSport(Sport sport) => new()
            {
                Id = sport.Id,
                Description = sport.Description,
                Name = sport.Name
            };
        }

        public static IMongoCollection<SportPersistance> GetSportsCollection(this IMongoDatabase mongoDb) => mongoDb.GetCollection<SportPersistance>(SportsCollectionName);
    }
}
