using System.Reflection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OffroadCamping.Appointments.Infrastructure
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddCommonResourceBuilder(this TracerProviderBuilder builder)
        {
            var serviceName = Assembly.GetEntryAssembly()?.GetName().Name;

            return builder
                .SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService(serviceName ?? "OffroadCamping.Appointments")
                    .AddTelemetrySdk()
                    .AddEnvironmentVariableDetector());
        }
    }
}
