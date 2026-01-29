public class DomainTests
{
    [Fact]
    public void Domain_Should_Not_Depend_On_Infrastructure_Or_Api_Or_Web()
    {
        var result = Types
            .InAssembly(typeof(OffroadCamping.Appointments.Domain.BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOn("OffroadCamping.Appointments.Infrastructure")
            .And()
            .NotHaveDependencyOn("OffroadCamping.Appointments.API")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}