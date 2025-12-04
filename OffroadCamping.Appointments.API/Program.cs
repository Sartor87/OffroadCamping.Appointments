using Microsoft.Extensions.Configuration;
using OffroadCamping.Appointments.Application;
using OffroadCamping.Appointments.Infrastructure;
using Scalar.AspNetCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.Load("OffroadCamping.Appointments.Application")));

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

builder.Services
    .AddApplicationServices()
    .AddEventStore(builder.Configuration.GetConnectionString("kurrentdb")!);

builder.Services.AddInfrastructureServices();
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("cache")!;
});

builder.Services.AddAppointmentsDbContext(builder.Configuration.GetConnectionString("AppointmentsDb")!);
builder.Services.AddUsersDbContext(builder.Configuration.GetConnectionString("IdentityDb")!);

builder.Services.AddOpenApi();

var app = builder.Build();

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

app.MapControllers();

app.Run();
