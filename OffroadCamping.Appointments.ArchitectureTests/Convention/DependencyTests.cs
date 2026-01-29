public class DependencyTests
{
    [Fact]
    public void Only_Infrastructure_Should_Reference_EF_Core_SQL()
    {
        var result = Types
            .InAssembly(typeof(OffroadCamping.Appointments.Infrastructure.Data.CommonDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore.SqlServer")
            .GetResult();

        if (!result.IsSuccessful)
        {
            Console.WriteLine("Violations:");
            foreach (var type in result.FailingTypes)
            {
                Console.WriteLine(" - " + type.FullName);
            }
        }

        Assert.True(result.IsSuccessful);
    }
}