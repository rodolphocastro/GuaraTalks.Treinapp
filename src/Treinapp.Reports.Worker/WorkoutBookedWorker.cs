﻿using CloudNative.CloudEvents.Kafka;
using CloudNative.CloudEvents.SystemTextJson;

using Confluent.Kafka;

using Microsoft.Extensions.Logging;

using MongoDB.Driver;

using System;
using System.Threading;
using System.Threading.Tasks;

using Treinapp.Common;
using Treinapp.Common.Domain;
using Treinapp.Commons.Domain;
using Treinapp.Reports.Worker.Features.Reports;

namespace Treinapp.Reports.Worker
{
    public class WorkoutBookedWorker : KafkaConsumerWorker
    {
        private readonly IMongoDatabase database;

        public WorkoutBookedWorker(ILogger<WorkoutBookedWorker> logger,
            IServiceProvider serviceProvider,
            IMongoDatabase database) : base(logger, serviceProvider, new JsonEventFormatter<Workout>())
        {
            this.database = database ?? throw new ArgumentNullException(nameof(database));
        }

        protected override async Task DoScoped(CancellationToken cancellationToken)
        {
            if (KafkaConsumer is null)
            {
                throw new ArgumentException("For some reason the Consumer is null, this shouldn't happen.");
            }

            KafkaConsumer.Subscribe(Constants.CloudEvents.WorkoutBookedTopic);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var result = KafkaConsumer.Consume(cancellationToken);
                    var cloudEvent = result.Message.ToCloudEvent(cloudEventFormatter);
                    if (cloudEvent.Data is Workout bookedWorkout)
                    {
                        _logger.LogTrace("Attempting to update a report with the new booked workout");
                        var report = await database
                            .GetReportsCollection()
                            .FetchAsync(DateTimeOffset.UtcNow, cancellationToken);
                        if (report is null)
                        {
                            report = await database
                                .GetReportsCollection()
                                .InsertNewAsync(new Report(Guid.NewGuid()), cancellationToken);
                        }
                        report = report.WithBookedWorkout(bookedWorkout);
                        await database
                            .GetReportsCollection()
                            .UpdateAsync(report, cancellationToken);
                    }
                }                
                // Consumer errors should generally be ignored (or logged) unless fatal.
                catch (ConsumeException e) when (e.Error.IsFatal)
                {
                    _logger.LogError(e, "A fatal consumer error happened");
                    throw;
                }
                catch (ConsumeException) { /* Ignored, see above */ }
                catch (Exception e)
                {
                    _logger.LogError(e, "An exception happened, oh no");
                }
                finally
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }

            }
            KafkaConsumer.Unsubscribe();
        }
    }
}