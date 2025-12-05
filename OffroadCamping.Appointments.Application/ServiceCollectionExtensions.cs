using Microsoft.Extensions.DependencyInjection;
using OffroadCamping.Appointments.Application.Services.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace OffroadCamping.Appointments.Application
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string validIssuer, string audience, byte[] token)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = validIssuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(token),
                        ValidateIssuerSigningKey = true,
                    };
                });

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add any HttpClients here
            // Examples:
            
            // Example for Azure OpenAI ChatClient with Function Invocation and OpenTelemetry
            //services.AddChatClient(services =>
            //    new ChatClientBuilder(
            //        new AzureOpenAIClient(
            //            new Uri(azureEndpoint),
            //            new AzureKeyCredential(azureKey)
            //        ).GetChatClient(azureDeploymentName).AsIChatClient()
            //    )
            //    .UseFunctionInvocation()
            //    .UseOpenTelemetry(loggerFactory: loggerFactory, configure: o => o.EnableSensitiveData = true);

            //Example for resiliant and cicuit breaker HttpClient
            // Configure HttpClientFactory for gnews.io API
            //services.AddHttpClient<IGNewsApiClient, GNewsApiClient>(client =>
            //{
            //    client.BaseAddress = new Uri("https://gnews.io");
            //    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("stock-exchange-tool", "1.0"));
            //})
            //// Retry on transient errors (HTTP 5xx, 408, etc.)
            //.AddPolicyHandler(HttpPolicyExtensions
            //    .HandleTransientHttpError()
            //    .WaitAndRetryAsync(
            //        retryCount: 3,
            //        sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)) // 2s, 4s, 8s
            //    ))
            //// Circuit breaker: after 5 consecutive failures, break for 30s
            //.AddPolicyHandler(HttpPolicyExtensions
            //    .HandleTransientHttpError()
            //    .CircuitBreakerAsync(
            //        handledEventsAllowedBeforeBreaking: 5,
            //        durationOfBreak: TimeSpan.FromSeconds(30)
            //    ))
            //// Timeout: cancel requests taking longer than 10s
            //.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));

            return services;
        }
    }
}
