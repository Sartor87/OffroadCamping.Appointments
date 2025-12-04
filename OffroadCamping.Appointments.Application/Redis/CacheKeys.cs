namespace OffroadCamping.Appointments.Application.Redis;

public static class CacheKeys
{
    public static string AppointmentsByDoctorAndFacility(string facilityName, Guid doctorId) => $"appointments:{facilityName}:{doctorId}";
    public static string UserById(Guid id) => $"user:{id}";
}