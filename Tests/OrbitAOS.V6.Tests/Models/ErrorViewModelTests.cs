using Xunit;
using System;
using OrbitAOS.V6.Models;

namespace OrbitAOS.V6.Tests.Models
{
    /// <summary>
    /// Unit tests for the ErrorViewModel class.
    /// Covers all properties and computed members.
    /// </summary>
    public class ErrorViewModelTests
    {
        // -----------------------------------------------------------------------
        // Constructor / default state
        // -----------------------------------------------------------------------

        [Fact]
        public void ErrorViewModel_DefaultConstructor_RequestIdIsNull()
        {
            // Arrange & Act
            var model = new ErrorViewModel();

            // Assert
            Assert.Null(model.RequestId);
        }

        [Fact]
        public void ErrorViewModel_DefaultConstructor_ShowRequestIdIsFalse()
        {
            // Arrange & Act
            var model = new ErrorViewModel();

            // Assert
            Assert.False(model.ShowRequestId);
        }

        // -----------------------------------------------------------------------
        // RequestId property
        // -----------------------------------------------------------------------

        [Fact]
        public void RequestId_SetToNonEmptyString_ReturnsSameValue()
        {
            // Arrange
            var model = new ErrorViewModel();
            const string expectedId = "abc-123-xyz";

            // Act
            model.RequestId = expectedId;

            // Assert
            Assert.Equal(expectedId, model.RequestId);
        }

        [Fact]
        public void RequestId_SetToNull_ReturnsNull()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "some-id" };

            // Act
            model.RequestId = null;

            // Assert
            Assert.Null(model.RequestId);
        }

        [Fact]
        public void RequestId_SetToEmptyString_ReturnsEmptyString()
        {
            // Arrange
            var model = new ErrorViewModel();

            // Act
            model.RequestId = string.Empty;

            // Assert
            Assert.Equal(string.Empty, model.RequestId);
        }

        [Fact]
        public void RequestId_SetToWhitespace_ReturnsWhitespace()
        {
            // Arrange
            var model = new ErrorViewModel();

            // Act
            model.RequestId = "   ";

            // Assert
            Assert.Equal("   ", model.RequestId);
        }

        // -----------------------------------------------------------------------
        // ShowRequestId computed property
        // -----------------------------------------------------------------------

        [Fact]
        public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = null };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsEmptyString_ReturnsFalse()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = string.Empty };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsNonEmptyString_ReturnsTrue()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "00-abc123-def456-00" };

            // Act & Assert
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsWhitespaceOnly_ReturnsTrue()
        {
            // Arrange – whitespace is NOT empty, so ShowRequestId should be true
            var model = new ErrorViewModel { RequestId = "   " };

            // Act & Assert
            Assert.True(model.ShowRequestId);
        }

        [Theory]
        [InlineData("trace-id-001", true)]
        [InlineData("0HN1234567890", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void ShowRequestId_Theory_MatchesExpected(string? requestId, bool expected)
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = requestId };

            // Act & Assert
            Assert.Equal(expected, model.ShowRequestId);
        }

        // -----------------------------------------------------------------------
        // Object initializer pattern (common usage in HomeController)
        // -----------------------------------------------------------------------

        [Fact]
        public void ErrorViewModel_ObjectInitializer_SetsRequestIdCorrectly()
        {
            // Arrange & Act
            const string traceId = "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01";
            var model = new ErrorViewModel { RequestId = traceId };

            // Assert
            Assert.Equal(traceId, model.RequestId);
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ErrorViewModel_MultipleAssignments_LastValueWins()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "first-id" };

            // Act
            model.RequestId = "second-id";

            // Assert
            Assert.Equal("second-id", model.RequestId);
            Assert.True(model.ShowRequestId);
        }
    }
}
