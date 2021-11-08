using FluentAssertions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System;

using Xunit;

namespace Treinapp.Unit
{
    public class ApiProgramTests
    {
        private readonly IHost _subject;

        public ApiProgramTests()
        {
            //_subject = Treinapp.API.Program.CreateHostBuilder(Array.Empty<string>()).Build();
            _subject = Treinapp.API.Program.CreateHostBuilder(new string[] { "Logging:LogLevel:Default=Error" }).Build();
        }

        [Fact]
        public void Should_Have_IConfigurationInstance()
        {
            // Arrange
            var subject = _subject;

            // Act
            IConfiguration result = subject.Services.GetService<IConfiguration>();

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public void Should_Prioritize_CommandLineArgs()
        {
            // Arrange            
            IConfiguration subject = _subject.Services.GetService<IConfiguration>();

            // Act
            string result = subject.GetValue<string>("Logging:LogLevel:Default");

            // Assert
            result.Should().BeEquivalentTo("Error");
        }
    }
}
