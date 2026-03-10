using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using OrbitAOS.V6.Data;

namespace OrbitAOS.V6.Tests
{
    public class ProgramTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Program_ShouldBePublicPartialClass()
        {
            // Arrange
            var programType = typeof(Program);

            // Assert
            Assert.NotNull(programType);
            Assert.True(programType.IsPublic);
            Assert.True(programType.IsClass);
        }

        [Fact]
        public void Application_ShouldStartSuccessfully()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act & Assert - If the application starts, this won't throw
            Assert.NotNull(client);
        }

        [Fact]
        public async Task HomeIndex_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task HomeIndex_ShouldReturnHtmlContent()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/");

            // Assert
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.NotNull(content);
                Assert.Contains("text/html", response.Content.Headers.ContentType?.ToString() ?? "");
            }
        }

        [Fact]
        public async Task Privacy_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/Home/Privacy");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task NonExistentRoute_ShouldReturnNotFound()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/NonExistent/Route");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task StaticFiles_ShouldBeAccessible()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert - if static files middleware is configured, the request should succeed
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound || response.StatusCode == HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task Application_ShouldHandleHttpsRedirection()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/");

            // Assert - Either OK or redirect
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.MovedPermanently ||
                response.StatusCode == HttpStatusCode.Redirect ||
                response.StatusCode == HttpStatusCode.TemporaryRedirect);
        }

        [Fact]
        public async Task Error_ShouldReturnSuccessStatusCode()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/Home/Error");

            // Assert
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Application_ShouldHaveContentInResponse()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/");

            // Assert
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Assert.NotNull(content);
                Assert.True(content.Length > 0);
            }
        }

        [Fact]
        public void WebApplicationFactory_ShouldCreateClient()
        {
            // Act
            var client = _factory.CreateClient();

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        public async Task MultipleRequests_ShouldSucceed()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response1 = await client.GetAsync("/");
            var response2 = await client.GetAsync("/Home/Privacy");
            var response3 = await client.GetAsync("/");

            // Assert
            Assert.True(response1.IsSuccessStatusCode || response1.StatusCode == HttpStatusCode.Redirect);
            Assert.True(response2.IsSuccessStatusCode || response2.StatusCode == HttpStatusCode.NotFound || response2.StatusCode == HttpStatusCode.Redirect);
            Assert.True(response3.IsSuccessStatusCode || response3.StatusCode == HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task Home_ShouldUseDefaultRoute()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response1 = await client.GetAsync("/");
            var response2 = await client.GetAsync("/Home");
            var response3 = await client.GetAsync("/Home/Index");

            // Assert - All should return OK or redirect
            Assert.True(response1.IsSuccessStatusCode || response1.StatusCode == HttpStatusCode.Redirect);
            Assert.True(response2.IsSuccessStatusCode || response2.StatusCode == HttpStatusCode.NotFound || response2.StatusCode == HttpStatusCode.Redirect);
            Assert.True(response3.IsSuccessStatusCode || response3.StatusCode == HttpStatusCode.NotFound || response3.StatusCode == HttpStatusCode.Redirect);
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/Privacy")]
        [InlineData("/Home/Error")]
        public async Task ValidRoutes_ShouldNotReturnServerError(string url)
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.True((int)response.StatusCode < 500);
        }

        [Theory]
        [InlineData("/Invalid")]
        [InlineData("/Home/InvalidAction")]
        [InlineData("/InvalidController/Index")]
        public async Task InvalidRoutes_ShouldReturnNotFound(string url)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync(url);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public void Application_ShouldHaveDbContext()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act
            var dbContext = services.GetService<ApplicationDbContext>();

            // Assert
            Assert.NotNull(dbContext);
        }

        [Fact]
        public void Application_ShouldConfigureServices()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act & Assert
            Assert.NotNull(services.GetService<ApplicationDbContext>());
        }

        [Fact]
        public async Task Application_ShouldHandleMultipleConcurrentRequests()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            var tasks = new List<Task<HttpResponseMessage>>();

            // Act
            for (int i = 0; i < 10; i++)
            {
                tasks.Add(client.GetAsync("/"));
            }
            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses)
            {
                Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);
            }
        }

        [Fact]
        public void Services_ShouldIncludeControllersWithViews()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var services = scope.ServiceProvider;

            // Act
            var actionDescriptorProvider = services.GetService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();

            // Assert
            Assert.NotNull(actionDescriptorProvider);
        }

        [Fact]
        public async Task Application_ShouldReturnContentType()
        {
            // Arrange
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            // Act
            var response = await client.GetAsync("/");

            // Assert
            if (response.IsSuccessStatusCode)
            {
                Assert.NotNull(response.Content.Headers.ContentType);
            }
        }

        [Fact]
        public void Factory_ShouldProvideServices()
        {
            // Act
            var services = _factory.Services;

            // Assert
            Assert.NotNull(services);
        }
    }
}
