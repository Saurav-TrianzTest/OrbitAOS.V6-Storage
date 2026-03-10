using Xunit;
using OrbitAOS.V6.Models;

namespace OrbitAOS.V6.Models.Tests
{
    public class ErrorViewModelTests
    {
        [Fact]
        public void Constructor_ShouldCreateInstance()
        {
            // Arrange & Act
            var model = new ErrorViewModel();

            // Assert
            Assert.NotNull(model);
        }

        [Fact]
        public void RequestId_ShouldBeNullByDefault()
        {
            // Arrange & Act
            var model = new ErrorViewModel();

            // Assert
            Assert.Null(model.RequestId);
        }

        [Fact]
        public void RequestId_ShouldSetAndGetValue()
        {
            // Arrange
            var model = new ErrorViewModel();
            var testRequestId = "test-request-id-123";

            // Act
            model.RequestId = testRequestId;

            // Assert
            Assert.Equal(testRequestId, model.RequestId);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsNull()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = null };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnFalse_WhenRequestIdIsEmpty()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = string.Empty };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnTrue_WhenRequestIdHasValue()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "valid-request-id" };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ShowRequestId_ShouldReturnTrue_WhenRequestIdIsWhitespace()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "   " };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result); // string.IsNullOrEmpty doesn't check for whitespace
        }

        [Theory]
        [InlineData("request-123")]
        [InlineData("abc-def-ghi")]
        [InlineData("test")]
        [InlineData("uuid-format-string")]
        [InlineData("a")]
        public void ShowRequestId_ShouldReturnTrue_WithVariousValidRequestIds(string requestId)
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = requestId };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShowRequestId_ShouldReturnFalse_WithNullOrEmptyRequestIds(string requestId)
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = requestId };

            // Act
            var result = model.ShowRequestId;

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void RequestId_ShouldAcceptWhitespaceValue()
        {
            // Arrange
            var model = new ErrorViewModel();
            var whitespaceId = "   ";

            // Act
            model.RequestId = whitespaceId;

            // Assert
            Assert.Equal(whitespaceId, model.RequestId);
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void RequestId_CanBeReassigned()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "initial-id" };

            // Act
            model.RequestId = "new-id";

            // Assert
            Assert.Equal("new-id", model.RequestId);
        }

        [Fact]
        public void RequestId_ShouldAcceptLongStrings()
        {
            // Arrange
            var model = new ErrorViewModel();
            var longId = new string('a', 1000);

            // Act
            model.RequestId = longId;

            // Assert
            Assert.Equal(longId, model.RequestId);
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void RequestId_ShouldAcceptSpecialCharacters()
        {
            // Arrange
            var model = new ErrorViewModel();
            var specialId = "!@#$%^&*()_+-=[]{}|;:',.<>?/`~";

            // Act
            model.RequestId = specialId;

            // Assert
            Assert.Equal(specialId, model.RequestId);
            Assert.True(model.ShowRequestId);
        }

        [Fact]
        public void ShowRequestId_ShouldBeReadOnly()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "test-id" };
            var propertyInfo = typeof(ErrorViewModel).GetProperty(nameof(ErrorViewModel.ShowRequestId));

            // Act & Assert
            Assert.NotNull(propertyInfo);
            Assert.Null(propertyInfo.SetMethod);
            Assert.NotNull(propertyInfo.GetMethod);
        }

        [Fact]
        public void ErrorViewModel_PropertiesShouldBeSettableMultipleTimes()
        {
            // Arrange
            var model = new ErrorViewModel();

            // Act
            model.RequestId = "first";
            model.RequestId = "second";
            model.RequestId = "third";

            // Assert
            Assert.Equal("third", model.RequestId);
        }

        [Fact]
        public void ErrorViewModel_ShouldSupportNullAssignment()
        {
            // Arrange
            var model = new ErrorViewModel { RequestId = "initial" };

            // Act
            model.RequestId = null;

            // Assert
            Assert.Null(model.RequestId);
            Assert.False(model.ShowRequestId);
        }
    }
}
