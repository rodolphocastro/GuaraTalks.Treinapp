using FluentAssertions;

using Microsoft.Extensions.Configuration;

using System.Collections.Generic;

using Xunit;

namespace Treinapp.Unit
{
    public class IConfigurationTests
    {
        private readonly ConfigurationBuilder _builder;
        private IConfiguration Subject => _builder.Build();

        public IConfigurationTests()
        {
            _builder = new ConfigurationBuilder();
        }

        [Fact]
        public void Builder_EnvironmentVariables_GetsPath()
        {
            // Arrange
            _builder.AddEnvironmentVariables();

            // Act
            string result = Subject.GetValue<string>("PATH");

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

        [Theory]
        [InlineData("ConnectionStrings__MongoDb", "abcdefg")]
        [InlineData("Logging__Console__Cobaia", "cobaia.123")]
        public void Builder_InMemory_ReturnsWithValue(string expectedPath, string expectedString)
        {
            // Arrange
            _builder
                .AddEnvironmentVariables()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { expectedPath, expectedString }
                });

            var subject = Subject;

            // Act
            string result = subject.GetValue<string>(expectedPath);

            // Assert
            result.Should().BeEquivalentTo(expectedString);
        }
    }
}
