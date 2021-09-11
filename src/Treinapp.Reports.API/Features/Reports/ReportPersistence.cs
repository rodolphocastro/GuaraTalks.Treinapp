
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common.Domain;
using Treinapp.Commons.Domain;

namespace Treinapp.Reports.Worker.Features.Reports
{
    public class ReportPersistence
    {
        public ReportPersistence() { }

        public ReportPersistence(Report report)
        {
            Id = report.Id;
            ForDay = report.ForDay;
            CreatedSports = report.CreatedSports;
            BookedWorkouts = report.BookedWorkouts;
            StartedWorkouts = report.StartedWorkouts;
            FinishedWorkouts = report.FinishedWorkouts;
        }

        public Guid Id { get; set; }
        public DateTimeOffset ForDay { get; set; }
        public IEnumerable<ReportedSport> CreatedSports { get; set; } = new HashSet<ReportedSport>();
        public IEnumerable<Workout> BookedWorkouts { get; set; } = new HashSet<Workout>();
        public IEnumerable<Workout> StartedWorkouts { get; set; } = new HashSet<Workout>();
        public IEnumerable<Workout> FinishedWorkouts { get; set; } = new HashSet<Workout>();

        public Report ToReport() => new(
            Id, CreatedSports, BookedWorkouts,
            StartedWorkouts, FinishedWorkouts, ForDay);
    }

    public static class ReportPersistenceExtensions
    {
        /// <summary>
        /// The collection name within Mongo.
        /// </summary>
        private const string SportsCollectionName = "reports";

        /// <summary>
        /// Gets the collection, within Mongo, that stores Reports.
        /// </summary>
        /// <param name="mongoDb"></param>
        /// <returns></returns>
        public static IMongoCollection<ReportPersistence> GetReportsCollection(this IMongoDatabase mongoDb) => mongoDb.GetCollection<ReportPersistence>(SportsCollectionName);

        /// <summary>
        /// Inserts a new Report into the collection.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Report> InsertNewAsync(this IMongoCollection<ReportPersistence> collection, Report report, CancellationToken cancellationToken = default)
        {
            var persistence = new ReportPersistence(report);
            await collection.InsertOneAsync(persistence, null, cancellationToken);
            return persistence.ToReport();
        }

        /// <summary>
        /// Fetches a report by its day.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="sportId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Report> FetchAsync(
            this IMongoCollection<ReportPersistence> collection,
            DateTimeOffset forDay,
            CancellationToken cancellationToken = default)
        {
            ReportPersistence mongoResult = await collection
                .Find(s => s.ForDay.Equals(forDay.Date))
                .SingleOrDefaultAsync(cancellationToken);
            return mongoResult?.ToReport() ?? null;
        }

        /// <summary>
        /// Replaces a Sport and all its elements.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="report"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<Report> UpdateAsync(
            this IMongoCollection<ReportPersistence> collection,
            Report report,
            CancellationToken cancellationToken = default)
        {
            var persistence = new ReportPersistence(report);
            _ = await collection
                .ReplaceOneAsync(s => s.Id.Equals(persistence.Id), persistence, new ReplaceOptions(), cancellationToken);
            return persistence.ToReport();
        }
    }
}
