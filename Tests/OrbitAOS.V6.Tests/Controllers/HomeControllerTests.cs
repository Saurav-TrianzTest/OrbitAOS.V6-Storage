using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using System.Diagnostics;
using Xunit;

namespace OrbitAOS.V6.Tests.Controllers
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object);

            // Provide a minimal HttpContext so HttpContext.TraceIdentifier is accessible
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        // ---------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidLogger_CreatesInstance()
        {
            // Arrange & Act
            var controller = new HomeController(_loggerMock.Object);

            // Assert
            Assert.NotNull(controller);
        }

        // ---------------------------------------------------------------
        // Index()
        // ---------------------------------------------------------------

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
            Assert.Null(result.Model);
        }

        [Fact]
        public void Index_ViewResult_ViewNameIsNullOrIndex()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            // When ViewName is null the framework resolves it to the action name ("Index")
            Assert.True(result.ViewName == null || result.ViewName == "Index");
        }

        // ---------------------------------------------------------------
        // Privacy()
        // ---------------------------------------------------------------

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
            Assert.Null(result.Model);
        }

        [Fact]
        public void Privacy_ViewResult_ViewNameIsNullOrPrivacy()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.True(result.ViewName == null || result.ViewName == "Privacy");
        }

        // ---------------------------------------------------------------
        // Error()
        // ---------------------------------------------------------------

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
            Assert.IsType<ErrorViewModel>(result.Model);
        }

        [Fact]
        public void Error_ErrorViewModel_RequestIdIsNotNull()
        {
            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            // TraceIdentifier is set by DefaultHttpContext; it should not be null
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void Error_ErrorViewModel_ShowRequestIdReflectsRequestId()
        {
            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            // ShowRequestId must be consistent with RequestId
            bool expected = !string.IsNullOrEmpty(model.RequestId);
            Assert.Equal(expected, model.ShowRequestId);
        }

        [Fact]
        public void Error_WhenActivityCurrentIsNull_UsesTraceIdentifier()
        {
            // Arrange – ensure no ambient Activity
            var previousActivity = Activity.Current;
            Activity.Current = null;

            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "trace-id-from-context";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

            try
            {
                // Act
                var result = _controller.Error() as ViewResult;
                var model = result?.Model as ErrorViewModel;

                // Assert
                Assert.NotNull(model);
                Assert.Equal("trace-id-from-context", model.RequestId);
            }
            finally
            {
                Activity.Current = previousActivity;
            }
        }

        [Fact]
        public void Error_WhenActivityCurrentIsSet_UsesActivityId()
        {
            // Arrange
            using var activity = new Activity("TestOperation");
            activity.Start();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            // Activity.Current.Id is used when available
            Assert.Equal(Activity.Current?.Id, model.RequestId);
        }

        // ---------------------------------------------------------------
        // Multiple calls – idempotency
        // ---------------------------------------------------------------

        [Fact]
        public void Index_CalledMultipleTimes_AlwaysReturnsViewResult()
        {
            for (int i = 0; i < 3; i++)
            {
                var result = _controller.Index();
                Assert.IsType<ViewResult>(result);
            }
        }

        [Fact]
        public void Privacy_CalledMultipleTimes_AlwaysReturnsViewResult()
        {
            for (int i = 0; i < 3; i++)
            {
                var result = _controller.Privacy();
                Assert.IsType<ViewResult>(result);
            }
        }

        [Fact]
        public void Error_CalledMultipleTimes_AlwaysReturnsViewResult()
        {
            for (int i = 0; i < 3; i++)
            {
                var result = _controller.Error();
                Assert.IsType<ViewResult>(result);
            }
        }

        // ---------------------------------------------------------------
        // IActionResult interface compliance
        // ---------------------------------------------------------------

        [Fact]
        public void Index_ReturnsIActionResult()
        {
            IActionResult result = _controller.Index();
            Assert.NotNull(result);
        }

        [Fact]
        public void Privacy_ReturnsIActionResult()
        {
            IActionResult result = _controller.Privacy();
            Assert.NotNull(result);
        }

        [Fact]
        public void Error_ReturnsIActionResult()
        {
            IActionResult result = _controller.Error();
            Assert.NotNull(result);
        }
    }
}
