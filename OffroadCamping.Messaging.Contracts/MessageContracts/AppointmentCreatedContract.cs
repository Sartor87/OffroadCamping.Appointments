namespace OffroadCamping.Messaging.Contracts.MessageContracts
{
    public record AppointmentCreatedContract(Guid AppointmentId, string Subject, string HtmlContent);
}
