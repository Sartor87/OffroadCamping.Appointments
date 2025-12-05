using OffroadCamping.Appointments.SharedKernel.SystemClock;

namespace OffroadCamping.Appointments.SharedKernel.Core.SystemClock;

internal sealed class SystemClock : ISystemClock
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow.StartOfDay;
}