namespace OffroadCamping.Appointments.SharedKernel.SystemClock
{
    internal static class DateTimeExtensions
    {
        extension(DateTime)
        {
            public static DateTime StartOfDay(DateTime date) => new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, date.Kind);
        }

        extension(DateTimeOffset source)
        {
            public DateTimeOffset StartOfDay => new DateTime(source.Year, source.Month, source.Day, 0, 0, 0, source.DateTime.Kind);
        }
    }
}
