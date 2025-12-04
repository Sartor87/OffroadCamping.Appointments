using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OffroadCamping.Appointments.Infrastructure.Data;
using OffroadCamping.Appointments.MigrationService;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.Services.AddDbContext<AppointmentsDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("AppointmentsDb")));
builder.Services.AddDbContext<UserDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("IdentityDb")));

var host = builder.Build();
host.Run();