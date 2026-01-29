using System;
using System.Linq;
using NetArchTest.Rules;
using Xunit;

namespace OffroadCamping.Appointments.ArchitectureTests.Convention
{
    public class NamingTests
    {
        /*
         Pseudocode Plan:
         1. Get the assembly that contains domain types (using BaseEntity).
         2. Select all classes in that assembly that inherit from BaseEntity.
         3. Verify those classes reside in the "OffroadCamping.Appointments.Domain.Entities" namespace.
         4. If there are violations, print each failing type to the console for easier debugging.
         5. Fail the test by asserting the rule result's success, including a descriptive message listing violations.
        */

        [Fact]
        public void Domain_Entities_Should_Reside_In_Entities_Namespace()
        {
            var assembly = typeof(OffroadCamping.Appointments.Domain.BaseEntity).Assembly;

            var result = Types
                .InAssembly(assembly)
                .That()
                .AreClasses()
                .And()
                .Inherit(typeof(OffroadCamping.Appointments.Domain.BaseEntity))
                .Should()
                .ResideInNamespace("OffroadCamping.Appointments.Domain")
                .GetResult();

            if (!result.IsSuccessful)
            {
                Console.WriteLine("Domain entity namespace violations:");
                foreach (var type in result.FailingTypes)
                {
                    Console.WriteLine(" - " + type.FullName);
                }
            }

            Assert.True(
                result.IsSuccessful,
                result.IsSuccessful ? "" :
                $"Expected all domain entity classes to reside in 'OffroadCamping.Appointments.Domain'. Violations: {string.Join(", ", result.FailingTypes.Select(t => t.FullName))}"
            );
        }
    }
}