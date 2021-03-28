using System;
using System.Threading.Tasks;
using Demo.Api.Domain;
using Demo.Api.ReferenceData;
using FluentAssertions;
using Xunit;

namespace Demo.Api.IntegrationTests.ReferenceData
{
    public class GetUnitsOfMeasureTests: BaseIntegrationTest
    {
        [Fact]
        public async Task ItMapsItAsStringToString()
        {
            var result = await AppFixture.SendAsync(new GetUnitsOfMeasureRequest());
            foreach (var refData in result)
            {
                var isMemberOfEnum = Enum.TryParse(refData.Code, out UnitOfMeasure _);
                isMemberOfEnum.Should()
                    .BeTrue(because: $"'{refData.Code}' should be a member of the UnitOfMeasure enum");
            }
        }
    }
}
