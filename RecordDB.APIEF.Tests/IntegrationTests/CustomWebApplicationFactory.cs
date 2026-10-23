using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RecordDB.APIEF.Repositories;

namespace RecordDB.APIEF.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IArtistRepository> ArtistRepoMock { get; } = new();
        public Mock<IRecordRepository> RecordRepoMock { get; } = new();
        public Mock<IDiscRepository> DiscRepoMock { get; } = new();
        public Mock<ITrackRepository> TrackRepoMock { get; } = new();

        public CustomWebApplicationFactory()
        {
            ResetMocks();
        }

        public void ResetMocks()
        {
            ArtistRepoMock.Reset();
            RecordRepoMock.Reset();
            DiscRepoMock.Reset();
            TrackRepoMock.Reset();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureServices(services =>
            {
                // Remove existing scoped repository registrations
                var toRemove = services.Where(d =>
                    d.ServiceType == typeof(IArtistRepository) ||
                    d.ServiceType == typeof(IRecordRepository) ||
                    d.ServiceType == typeof(IDiscRepository) ||
                    d.ServiceType == typeof(ITrackRepository)
                ).ToList();

                foreach (var descriptor in toRemove)
                {
                    services.Remove(descriptor);
                }

                // Register mock singletons/delegates so tests can inject behavior
                services.AddScoped(_ => ArtistRepoMock.Object);
                services.AddScoped(_ => RecordRepoMock.Object);
                services.AddScoped(_ => DiscRepoMock.Object);
                services.AddScoped(_ => TrackRepoMock.Object);
            });
        }
    }
}
