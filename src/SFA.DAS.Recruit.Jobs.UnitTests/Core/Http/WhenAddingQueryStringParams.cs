using Microsoft.Extensions.Primitives;
using SFA.DAS.Recruit.Jobs.Core.Http;

namespace SFA.DAS.Recruit.Jobs.UnitTests.Core.Http;

public class WhenAddingQueryStringParams
{
    [Test]
    public void Then_The_Parameters_Are_Added()
    {
        // act
        var result = "api/controller/action".WithQueryParams(("foo", "foo_value"), ("foo2", "foo2_value"));

        // assert
        result.Should().Be("api/controller/action?foo=foo_value&foo2=foo2_value");
    }
    
    [Test]
    public void Then_The_Parameters_Are_Encoded()
    {
        // act
        var result = "api/controller/action".WithQueryParams(("foo", "value with /"));

        // assert
        result.Should().Be("api/controller/action?foo=value%20with%20%2F");
    }
    
    [Test]
    public void Then_Array_Values_Are_Added()
    {
        // act
        var result = "api/controller/action".WithQueryParams(("foo", new StringValues(["value1", "value2"])));

        // assert
        result.Should().Be("api/controller/action?foo=value1&foo=value2");
    }
    
    [Test]
    public void Then_Calls_Can_Be_Chained()
    {
        // act
        var result = "api/controller/action"
            .WithQueryParams(("foo", new StringValues(["value1", "value2"])))
            .WithQueryParams(("foo2", "foo2_value"));

        // assert
        result.Should().Be("api/controller/action?foo=value1&foo=value2&foo2=foo2_value");
    }
}