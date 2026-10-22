using Xunit;
using OrbitAOS.V6.Models;

namespace OrbitAOS.V6.Tests.Models
{
    public class ErrorViewModelTests
    {
        // ---------------------------------------------------------------
        // Constructor / default state
        // ---------------------------------------------------------------

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

        // ---------------------------------------------------------------
        // RequestId property
        // ---------------------------------------------------------------

        [Fact]
        public void RequestId_SetToNonEmptyString_ReturnsSetValue()
        {
            // Arrange
            var model = new ErrorViewModel();

            // Act
            model.RequestId = "REQ-12345";

            // Assert
            Assert.Equal("REQ-12345", model.RequestId);
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
        public void RequestId_SetToNull_ReturnsNull()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "some-id" };

            // Act
            model.RequestId = null;

            // Assert
            Assert.Null(model.RequestId);
        }

        // ---------------------------------------------------------------
        // ShowRequestId computed property
        // ---------------------------------------------------------------

        [Fact]
        public void ShowRequestId_WhenRequestIdIsNull_ReturnsFalse()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = null };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsEmpty_ReturnsFalse()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = string.Empty };

            // Act & Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdIsWhitespace_ReturnsTrue()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "   " };

            // Act & Assert – ShowRequestId uses IsNullOrEmpty, not IsNullOrWhiteSpace
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_WhenRequestIdHasValue_ReturnsTrue()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "0HN1234:00000001" };

            // Act & Assert
            Assert.True(model.ShowRequestId);
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("00000000-0000-0000-0000-000000000001")]
        [InlineData("Activity:1234")]
        public void ShowRequestId_WithVariousNonEmptyValues_ReturnsTrue(string requestId)
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = requestId };

            // Act & Assert
            Assert.True(model.ShowRequestId);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("  ", true)]
        public void ShowRequestId_NullOrEmptyReturnsFalse_WhitespaceReturnsTrue(string? requestId, bool expected)
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = requestId };

            // Act & Assert
            Assert.Equal(expected, model.ShowRequestId);
        }

        // ---------------------------------------------------------------
        // Object initializer
        // ---------------------------------------------------------------

        [Fact]
        public void ErrorViewModel_ObjectInitializer_SetsRequestId()
        {
            // Arrange & Act
            var model = new ErrorViewModel { RequestId = "INIT-001" };

            // Assert
            Assert.Equal("INIT-001", model.RequestId);
            Assert.True(model.ShowRequestId);
        }

        // ---------------------------------------------------------------
        // Mutation: toggle RequestId
        // ---------------------------------------------------------------

        [Fact]
        public void ShowRequestId_AfterClearingRequestId_ReturnsFalse()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "some-id" };
            Assert.True(model.ShowRequestId);

            // Act
            model.RequestId = null;

            // Assert
            Assert.False(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_AfterSettingRequestId_ReturnsTrue()
        {
            // Arrange
            var model = new ErrorViewModel();
            Assert.False(model.ShowRequestId);

            // Act
            model.RequestId = "new-id";

            // Assert
            Assert.True(model.ShowRequestId);
        }
    }
}
