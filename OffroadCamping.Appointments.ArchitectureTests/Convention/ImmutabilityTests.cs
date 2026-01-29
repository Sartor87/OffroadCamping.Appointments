public class ImmutabilityTests
{
    [Fact]
    public void Domain_Entities_Should_Have_Private_Setters()
    {

        var assembly = typeof(OffroadCamping.Appointments.Domain.BaseEntity).Assembly;

        var types = Types
       .InAssembly(assembly)
       .That()
       .Inherit(typeof(OffroadCamping.Appointments.Domain.BaseEntity))
       .And()
       .AreClasses()
       .GetTypes();

        var failures = new List<string>();

        foreach (var type in types)
        {
            foreach (var prop in type.GetProperties())
            {
                var setter = prop.SetMethod;

                if (setter != null && !setter.IsPrivate)
                {
                    failures.Add($"{type.Name}.{prop.Name} setter is not private");
                }
            }
        }

        Assert.True(failures.Count == 0,
            "Found non-private setters:\n" + string.Join("\n", failures));
    }
}