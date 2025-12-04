using System.Text.Json;
using KurrentDB.Client;
using MassTransit;
using Microsoft.Extensions.Logging;
using OffroadCamping.Appointments.Application.Helpers;
using OffroadCamping.Appointments.Domain.Appointments.Events;
using OffroadCamping.Messaging.Contracts.MessageContracts;

namespace OffroadCamping.Appointments.Application.Consumers;

public class AppointmentEmailSentConsumer : IConsumer<AppointmentEmailSentContract>
{
    private readonly ILogger<AppointmentEmailSentConsumer> _logger;
    private readonly KurrentDBClient _eventStoreClient;

    public AppointmentEmailSentConsumer(ILogger<AppointmentEmailSentConsumer> logger, KurrentDBClient eventStoreClient)
    {
        _logger = logger;
        _eventStoreClient = eventStoreClient;
    }
    
    public async Task Consume(ConsumeContext<AppointmentEmailSentContract> context)
    {
        using var activity = ActivitySourceHelper.ActivitySource.StartActivity();
        activity.AddTag("AppointmentId", context.Message.AppointmentId);
        _logger.LogInformation($"Email confirmed sent for appointment: {context.Message.AppointmentId}");
        
        // This isn't used at all, just cool.
        var eventData = new EventData(
            Uuid.NewUuid(),
            nameof(AppointmentEmailSentEvent),
            JsonSerializer.SerializeToUtf8Bytes(new AppointmentEmailSentEvent(context.Message.AppointmentId))
        );
            
        await _eventStoreClient.AppendToStreamAsync(
            "emails",
            StreamState.Any,
            new[] { eventData },
            cancellationToken: new CancellationToken()
        );
    }
}