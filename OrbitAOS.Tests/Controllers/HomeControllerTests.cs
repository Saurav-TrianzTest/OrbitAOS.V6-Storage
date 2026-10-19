// ============================================================
// HomeControllerTests.cs – Unit Test Project
// Tests for HomeController (Web layer).
//
// Pattern: Arrange / Act / Assert
// Framework: xUnit + Moq + FluentAssertions
// ============================================================

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using Xunit;

namespace OrbitAOS.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object);

            // Set up a minimal HttpContext so HttpContext.TraceIdentifier is available
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void Error_ReturnsViewResult_WithErrorViewModel()
        {
            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeOfType<ErrorViewModel>();
        }

        [Fact]
        public void Error_ErrorViewModel_HasRequestId()
        {
            // Arrange – set a known TraceIdentifier
            _controller.ControllerContext.HttpContext.TraceIdentifier = "test-trace-id";

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            model.Should().NotBeNull();
            model!.ShowRequestId.Should().BeTrue();
            model.RequestId.Should().Be("test-trace-id");
        }
    }
}
