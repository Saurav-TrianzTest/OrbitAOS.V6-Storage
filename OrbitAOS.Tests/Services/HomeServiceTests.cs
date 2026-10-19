// ============================================================
// HomeServiceTests.cs – Unit Test Project
// Tests for HomeService (Infrastructure layer).
//
// Pattern: Arrange / Act / Assert
// Framework: xUnit + Moq + FluentAssertions
// ============================================================

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.Infrastructure.Services;
using Xunit;

namespace OrbitAOS.Tests.Services
{
    public class HomeServiceTests
    {
        private readonly Mock<ILogger<HomeService>> _loggerMock;
        private readonly HomeService _service;

        public HomeServiceTests()
        {
            _loggerMock = new Mock<ILogger<HomeService>>();
            _service = new HomeService(_loggerMock.Object);
        }

        [Fact]
        public async Task GetDashboardDataAsync_ReturnsDto_WithApplicationName()
        {
            // Act
            var result = await _service.GetDashboardDataAsync();

            // Assert
            result.Should().NotBeNull();
            result.ApplicationName.Should().Be("OrbitAOS");
        }

        [Fact]
        public async Task GetDashboardDataAsync_ReturnsDto_WithRecentLastRefreshed()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var result = await _service.GetDashboardDataAsync();

            // Assert
            result.LastRefreshed.Should().BeOnOrAfter(before);
            result.LastRefreshed.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public async Task GetDashboardDataAsync_ReturnsDto_WithWelcomeMessage()
        {
            // Act
            var result = await _service.GetDashboardDataAsync();

            // Assert
            result.WelcomeMessage.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetDashboardDataAsync_SupportsCancellation()
        {
            // Arrange
            using var cts = new CancellationTokenSource();

            // Act & Assert – should not throw
            var act = async () => await _service.GetDashboardDataAsync(cts.Token);
            await act.Should().NotThrowAsync();
        }
    }
}
