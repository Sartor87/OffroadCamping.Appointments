public class InfrastructureTests
{
    [Fact]
    public void Infrastructure_Should_Only_Depend_On_Domain()
    {
        var result = Types
            .InAssembly(typeof(OffroadCamping.Appointments.Infrastructure.Persistence.EventStoreCheckpointRepository).Assembly)
            .ShouldNot()
            .HaveDependencyOn("OffroadCamping.Appointments.API")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}