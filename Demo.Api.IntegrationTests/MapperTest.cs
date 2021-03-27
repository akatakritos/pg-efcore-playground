using Demo.Api.Infrastructure;
using Xunit;

namespace Demo.Api.IntegrationTests
{
    public class MapperTest
    {
        [Fact]
        public void MapProfiles_AreValid()
        {
            var cfg = AutoMapperModule.ScanForMapProfiles(new[] { typeof(Startup).Assembly });
            cfg.AssertConfigurationIsValid();
        }

    }
}
