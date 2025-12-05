using KurrentDB.Client;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OffroadCamping.Appointments.Application.Consumers;
using OffroadCamping.Appointments.Application.Repositories;
using OffroadCamping.Appointments.Application.Services.Contracts;
using OffroadCamping.Appointments.Infrastructure.Data;
using OffroadCamping.Appointments.Infrastructure.Persistence;
using OffroadCamping.Appointments.Infrastructure.Services;
using OpenTelemetry.Trace;

namespace OffroadCamping.Appointments.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMasstransit(this IServiceCollection services, string connection)
        {
            services.AddMassTransit(x =>
            {
                x.AddConsumer<AppointmentEmailSentConsumer>();

                x.UsingRabbitMq((context, factoryConfigurator) =>
                {
                    factoryConfigurator.Host(new Uri(connection));

                    factoryConfigurator.ConfigureEndpoints(context);

                });
            });

            return services;
        }

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddTransient<ICalendarService, GoogleCalendarService>();
            services.AddScoped<IAuthService, AuthService>();
            //services.AddTelemetry();
            return services;
        }

        public static IServiceCollection AddEventStore(this IServiceCollection serviceCollection, string connectionString)
        {
            var settings = KurrentDBClientSettings.Create(connectionString);
            var eventStoreClient = new KurrentDBClient(settings);
            serviceCollection.AddSingleton(eventStoreClient);
            return serviceCollection;
        }

        public static IServiceCollection AddAppointmentsDbContextForEventStore(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppointmentsDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            }, ServiceLifetime.Singleton);

            services.AddSingleton<IAppointmentsRepository, AppointmentsRepository>();
            services.AddSingleton<IEventStoreCheckpointRepository, EventStoreCheckpointRepository>();
            return services;
        }

        public static IServiceCollection AddAppointmentsDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AppointmentsDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            }, ServiceLifetime.Singleton); // TODO: - Figure out these lifetimes

            services.AddSingleton<IAppointmentsRepository, AppointmentsRepository>();
            services.AddSingleton<IEventStoreCheckpointRepository, EventStoreCheckpointRepository>();
            return services;
        }

        public static IServiceCollection AddUsersDbContext(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<UserDbContext>(options =>
            {
                options.UseSqlServer(connectionString);
            }, ServiceLifetime.Singleton); // TODO: - Figure out these lifetimes

            services.AddSingleton<IUserRepository, UserRepository>();
            return services;
        }

        //private static IServiceCollection AddTelemetry(this IServiceCollection services)
        //{
        //    services
        //        .AddOpenTelemetry()
        //        .WithTracing(builder =>
        //        {
        //            builder
        //                .AddOtlpExporter(x =>
        //                {
        //                    x.Endpoint = new Uri("http://localhost:4317");
        //                })
        //                .AddJaegerExporter()
        //                .AddCommonResourceBuilder()
        //                .AddHttpClientInstrumentation()
        //                .AddAspNetCoreInstrumentation()
        //                .AddSqlClientInstrumentation()
        //                .AddSource(ActivitySourceHelper.ActivitySource.Name);
        //        });

        //    return services;
        //}
    }
}
