namespace Treinapp.Common
{
    /// <summary>
    /// Constants applicable to the API as a whole.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Default route for controllers.
        /// </summary>
        public const string DefaultRoute = @"[controller]";

        /// <summary>
        /// Default key for the MongoDB Connection String.
        /// </summary>
        public const string MongoConnectionKey = @"MongoDb";

        /// <summary>
        /// Default database for this API's entities.
        /// </summary>
        public const string MongoDatabase = @"TreinaApp-entities";

        /// <summary>
        /// Default key for the Kafka Bootstrap Connection String.
        /// </summary>
        public const string KafkaBootstrapKey = @"KafkaBootstrap";

        public static class CloudEvents
        {
            /// <summary>
            /// Kafka topic for Sports that have been created.
            /// </summary>
            public const string SportCreatedTopic = "sport.created";

            /// <summary>
            /// Cloud Event type for Sports that have been created.
            /// </summary>
            public const string SportCreatedType = "treinapp.sports.v1.created";

            /// <summary>
            /// Kafka topic for Workouts that have been booked (created).
            /// </summary>
            public const string WorkoutBookedTopic = "workout.booked";

            /// <summary>
            /// Cloud Event type for Workouts that have been booked (created).
            /// </summary>
            public const string WorkoutBookedType = "treinapp.workouts.v1.booked";

            /// <summary>
            /// Kafka topic for Workouts that have been started.
            /// </summary>
            public const string WorkoutStartedTopic = "workout.started";

            /// <summary>
            /// Cloud Event type for Workouts that have been started.
            /// </summary>
            public const string WorkoutStartedType = "treinapp.workouts.v1.started";

            /// <summary>
            /// Kafka topic for Workouts that have been finished.
            /// </summary>
            public const string WorkoutFinishedTopic = "workout.finished";

            /// <summary>
            /// Cloud Event type for Workouts that have been finished.
            /// </summary>
            public const string WorkoutFinishedType = "treinapp.workouts.v1.finished";
        }
    }
}
