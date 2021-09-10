using HotChocolate;
using HotChocolate.Data;

using MongoDB.Driver;

using Treinapp.Reports.Worker.Features.Reports;

namespace Treinapp.Reports.API.Features.Reports
{
    public class ReportsQuery
    {
        [UseProjection]
        [UseSorting]
        [UseFiltering]
        public IExecutable<ReportPersistence> GetReport([Service] IMongoDatabase database)
        {
            var collection = database.GetReportsCollection();
            return collection.AsExecutable();
        }
    }
}
