using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OrbitAOS.V6.Controllers;
using OrbitAOS.V6.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace OrbitAOS.V6.Controllers.Tests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_mockLogger.Object);
        }

        [Fact]
        public void Constructor_ShouldCreateInstance_WithValidLogger()
        {
            // Arrange
            var logger = new Mock<ILogger<HomeController>>().Object;

            // Act
            var controller = new HomeController(logger);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Constructor_ShouldAcceptNullLogger()
        {
            // Arrange, Act
            var controller = new HomeController(null!);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void Index_ShouldReturnViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Index_ShouldReturnViewWithNoModel()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Model);
        }

        [Fact]
        public void Index_ViewNameShouldBeNull()
        {
            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName);
        }

        [Fact]
        public void Privacy_ShouldReturnViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Privacy_ShouldReturnViewWithNoModel()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Model);
        }

        [Fact]
        public void Privacy_ViewNameShouldBeNull()
        {
            // Act
            var result = _controller.Privacy() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName);
        }

        [Fact]
        public void Error_ShouldReturnViewResult()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error();

            // Assert
            Assert.NotNull(result);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ShouldReturnViewWithErrorViewModel()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-trace-id";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Model);
            Assert.IsType<ErrorViewModel>(result.Model);
        }

        [Fact]
        public void Error_ShouldSetRequestIdFromTraceIdentifier_WhenActivityCurrentIsNull()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            var expectedTraceId = "test-trace-identifier-123";
            httpContext.TraceIdentifier = expectedTraceId;
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.Equal(expectedTraceId, model.RequestId);
        }

        [Fact]
        public void Error_ShouldSetRequestIdFromActivity_WhenActivityExists()
        {
            // Arrange
            var activity = new Activity("TestActivity");
            activity.Start();
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;
            activity.Stop();

            // Assert
            Assert.NotNull(model);
            Assert.NotNull(model.RequestId);
        }

        [Fact]
        public void Error_ViewModelShouldHaveRequestId()
        {
            // Arrange
            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "trace-123";
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = _controller.Error() as ViewResult;
            var model = result?.Model as ErrorViewModel;

            // Assert
            Assert.NotNull(model);
            Assert.NotNull(model.RequestId);
            Assert.NotEmpty(model.RequestId);
        }

        [Fact]
        public void Error_ShouldHaveResponseCacheAttribute()
        {
            // Arrange
            var method = typeof(HomeController).GetMethod(nameof(HomeController.Error));

            // Act
            var attributes = method?.GetCustomAttributes(typeof(ResponseCacheAttribute), false);

            // Assert
            Assert.NotNull(attributes);
            Assert.Single(attributes);
            var responseCacheAttr = attributes[0] as ResponseCacheAttribute;
            Assert.NotNull(responseCacheAttr);
            Assert.Equal(0, responseCacheAttr.Duration);
            Assert.Equal(ResponseCacheLocation.None, responseCacheAttr.Location);
            Assert.True(responseCacheAttr.NoStore);
        }

        [Fact]
        public void Index_CanBeCalledMultipleTimes()
        {
            // Act
            var result1 = _controller.Index();
            var result2 = _controller.Index();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<ViewResult>(result1);
            Assert.IsType<ViewResult>(result2);
        }

        [Fact]
        public void Privacy_CanBeCalledMultipleTimes()
        {
            // Act
            var result1 = _controller.Privacy();
            var result2 = _controller.Privacy();

            // Assert
            Assert.NotNull(result1);
            Assert.NotNull(result2);
            Assert.IsType<ViewResult>(result1);
            Assert.IsType<ViewResult>(result2);
        }

        [Fact]
        public void HomeController_ShouldInheritFromController()
        {
            // Assert
            Assert.IsAssignableFrom<Controller>(_controller);
        }

        [Fact]
        public void AllActionMethods_ShouldReturnIActionResult()
        {
            // Act
            var indexResult = _controller.Index();
            var privacyResult = _controller.Privacy();

            var httpContext = new DefaultHttpContext();
            httpContext.TraceIdentifier = "test-id";
            _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
            var errorResult = _controller.Error();

            // Assert
            Assert.IsAssignableFrom<IActionResult>(indexResult);
            Assert.IsAssignableFrom<IActionResult>(privacyResult);
            Assert.IsAssignableFrom<IActionResult>(errorResult);
        }

        [Fact]
        public void Index_ShouldNotThrowException()
        {
            // Act
            var exception = Record.Exception(() => _controller.Index());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Privacy_ShouldNotThrowException()
        {
            // Act
            var exception = Record.Exception(() => _controller.Privacy());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Error_ShouldNotThrowException_WithValidContext()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var exception = Record.Exception(() => _controller.Error());

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void HomeController_ShouldHavePublicMethods()
        {
            // Arrange
            var type = typeof(HomeController);

            // Act
            var indexMethod = type.GetMethod(nameof(HomeController.Index));
            var privacyMethod = type.GetMethod(nameof(HomeController.Privacy));
            var errorMethod = type.GetMethod(nameof(HomeController.Error));

            // Assert
            Assert.NotNull(indexMethod);
            Assert.NotNull(privacyMethod);
            Assert.NotNull(errorMethod);
            Assert.True(indexMethod.IsPublic);
            Assert.True(privacyMethod.IsPublic);
            Assert.True(errorMethod.IsPublic);
        }

        [Fact]
        public void HomeController_ShouldBePublicClass()
        {
            // Arrange
            var type = typeof(HomeController);

            // Assert
            Assert.True(type.IsPublic);
            Assert.True(type.IsClass);
        }

        [Fact]
        public void Error_ViewNameShouldBeNull()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.Error() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName);
        }

        [Fact]
        public void Logger_ShouldBeInjectedViaConstructor()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<HomeController>>();

            // Act
            var controller = new HomeController(mockLogger.Object);

            // Assert
            Assert.NotNull(controller);
        }

        [Fact]
        public void HomeController_ConstructorParameter_ShouldBeNamed_logger()
        {
            // Arrange
            var constructorInfo = typeof(HomeController).GetConstructors()[0];
            var parameters = constructorInfo.GetParameters();

            // Assert
            Assert.Single(parameters);
            Assert.Equal("logger", parameters[0].Name);
        }
    }
}
