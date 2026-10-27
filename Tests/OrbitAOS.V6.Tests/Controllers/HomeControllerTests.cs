using Xunit;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;

namespace OrbitAOS.V6.Tests.Controllers
{
    /// <summary>
    /// Unit tests for HomeController.
    /// Covers Index, Privacy, and Error actions including edge cases.
    /// </summary>
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object);

            // Set up a default HttpContext so HttpContext.TraceIdentifier is available
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidLogger_CreatesInstance()
        {
            // Arrange & Act
            var logger = new Mock<ILogger<HomeController>>();
            var controller = new HomeController(logger.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_WithNullLogger_CreatesInstanceWithoutGuard()
        {
            // Arrange & Act – HomeController does not null-guard the logger parameter;
            // construction succeeds and the null reference is stored.
            ILogger<HomeController> nullLogger = null!;
            var controller = new HomeController(nullLogger);

            // Assert – instance is created (null guard is not enforced by the controller)
            Assert.NotNull(controller);
        }

        // -----------------------------------------------------------------------
        // Index action
        // -----------------------------------------------------------------------

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ViewResult_HasNullModel()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.Model);
        }

        [Fact]
        public void Index_ViewResult_ViewNameIsNullOrIndex()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert – when ViewName is null the framework resolves "Index" by convention
            Assert.NotNull(result);
            Assert.True(result!.ViewName == null || result.ViewName == "Index");
        }

        [Fact]
        public void Index_CalledMultipleTimes_AlwaysReturnsViewResult()
        {
            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                var result = _controller.Index();
                Assert.IsType<ViewResult>(result);
            }
        }

        // -----------------------------------------------------------------------
        // Privacy action
        // -----------------------------------------------------------------------

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ViewResult_HasNullModel()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result!.Model);
        }

        [Fact]
        public void Privacy_ViewResult_ViewNameIsNullOrPrivacy()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.ViewName == null || result.ViewName == "Privacy");
        }

        [Fact]
        public void Privacy_CalledMultipleTimes_AlwaysReturnsViewResult()
        {
            // Act & Assert
            for (int i = 0; i < 5; i++)
            {
                var result = _controller.Privacy();
                Assert.IsType<ViewResult>(result);
            }
        }

        // -----------------------------------------------------------------------
        // Error action
        // -----------------------------------------------------------------------

        [Fact]
        public void Error_ReturnsViewResult()
        {
            // Act
            var result = _controller.Error();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ViewResult_ModelIsErrorViewModel()
        {
            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ErrorViewModel>(result!.Model);
        }

        [Fact]
        public void Error_WhenNoActivityCurrent_UsesHttpContextTraceIdentifier()
        {
            // Arrange – ensure Activity.Current is null
            Activity.Current = null;

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.Equal("test-trace-id", model!.RequestId);
        }

        [Fact]
        public void Error_WhenActivityCurrentExists_UsesActivityId()
        {
            // Arrange
            using var activity = new Activity("TestOperation");
            activity.Start();
            Activity.Current = activity;

            try
            {
                // Act
                var result = _controller.Error() as ViewResult;
                var model = result?.Model as ErrorViewModel;

                // Assert
                Assert.NotNull(model);
                // Activity.Current?.Id is preferred over HttpContext.TraceIdentifier
                Assert.Equal(Activity.Current?.Id, model!.RequestId);
            }
            finally
            {
                activity.Stop();
                Activity.Current = null;
            }
        }

        [Fact]
        public void Error_RequestId_IsNotNullOrEmpty()
        {
            // Arrange
            Activity.Current = null;

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.False(string.IsNullOrEmpty(model!.RequestId));
        }

        [Fact]
        public void Error_ShowRequestId_IsTrueWhenTraceIdentifierSet()
        {
            // Arrange
            Activity.Current = null;

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.True(model!.ShowRequestId);
        }

        [Fact]
        public void Error_ViewResult_ViewNameIsNullOrError()
        {
            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result!.ViewName == null || result.ViewName == "Error");
        }

        [Fact]
        public void Error_CalledMultipleTimes_AlwaysReturnsViewResult()
        {
            // Arrange
            Activity.Current = null;

            // Act & Assert
            for (int i = 0; i < 3; i++)
            {
                var result = _controller.Error();
                Assert.IsType<ViewResult>(result);
            }
        }

        // -----------------------------------------------------------------------
        // IActionResult return type checks
        // -----------------------------------------------------------------------

        [Fact]
        public void Index_ReturnsIActionResult()
        {
            // Act
            IActionResult result = _controller.Index();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Privacy_ReturnsIActionResult()
        {
            // Act
            IActionResult result = _controller.Privacy();

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public void Error_ReturnsIActionResult()
        {
            // Act
            IActionResult result = _controller.Error();

            // Assert
            Assert.NotNull(result);
        }
    }
}
