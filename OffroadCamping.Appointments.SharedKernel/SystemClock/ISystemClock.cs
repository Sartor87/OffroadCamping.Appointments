namespace OffroadCamping.Appointments.SharedKernel.Core.SystemClock;

public interface ISystemClock
{
    DateTimeOffset Now { get; }
}