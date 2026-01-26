using OffroadCamping.Appointments.API.Endpoints;
using OffroadCamping.Appointments.Application;
using OffroadCamping.Appointments.Infrastructure;
using OffroadCamping.Appointments.SharedKernel.Core.SystemClock;
using OffroadCamping.Appointments.SharedKernel.ErrorHandling;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(AssemblyMarker).Assembly));

builder.Services.AddOutputCache();

// Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Patient", policy =>
    {
        policy.RequireRole("patient");
    });
    options.AddPolicy("Doctor", policy =>
    {
        policy.RequireRole("doctor");
    });
});

// JWT Authentication
builder.Services
    .AddJwtAuthentication(
        builder.Configuration["AppSettings:Issuer"]!,
        builder.Configuration["AppSettings:Audience"]!,
        Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
    .AddApplicationServices()
    .AddEventStore(builder.Configuration.GetConnectionString("kurrentdb")!);

// Infrastructure Services
builder.Services
    .AddMasstransit(builder.Configuration.GetConnectionString("rabbitmq")!)
    .AddInfrastructureServices();
// Caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("cache")!;
});

// DbContexts
builder.Services.AddAppointmentsDbContext(builder.Configuration.GetConnectionString("AppointmentsDb")!);
builder.Services.AddUsersDbContext(builder.Configuration.GetConnectionString("IdentityDb")!);

builder.Services.AddExceptionHandling();

builder.Services.AddSystemClock();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseErrorHandling();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

app.MapAppointmentsEndpoints();
app.MapAuthEndpoints();

app.Run();
