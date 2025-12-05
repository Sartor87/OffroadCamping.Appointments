var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache")
                    .WithRedisInsight();

var kurrentDB = builder.AddKurrentDB("kurrentdb", 2113);

var rabbitMqUsername = builder.AddParameter("RabbitMqUsername", secret: true);
var rabbitMqPassword = builder.AddParameter("RabbitMqPassword", secret: true);

var rabbit = builder.AddRabbitMQ("rabbitmq", rabbitMqUsername, rabbitMqPassword)
    .WithManagementPlugin();            // optional but super useful

var saPassword = builder.Configuration["Sql:SaPassword"]
    ?? throw new InvalidOperationException("Missing SQL SA password in user secrets.");

var issuer = builder.Configuration["AppSettings:Issuer"]
        ?? throw new InvalidOperationException("Missing JWT Issuer in user secrets.");
var audience = builder.Configuration["AppSettings:Audience"]
        ?? throw new InvalidOperationException("Missing JWT Audience in user secrets.");
var token = builder.Configuration["AppSettings:Token"]
    ?? throw new InvalidOperationException("Missing JWT Token in user secrets.");

var sql = builder
    .AddSqlServer("offroadcamping-appointments-sqlserver")
    .WithImage("mssql/server:2022-latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    // persist data to a volume to survive container restarts
    .WithVolume("sql_data", "/var/opt/mssql")
    .WithEnvironment("SA_PASSWORD", saPassword)
    .WithHostPort(1433);

var appointmentsDb = sql.AddDatabase("AppointmentsDb");
var identityDb = sql.AddDatabase("IdentityDb");

var migrations = builder.AddProject<Projects.OffroadCamping_Appointments_MigrationService>("migrations")
    .WithReference(sql)
    .WithReference(appointmentsDb)
    .WithReference(identityDb)
    .WaitForStart(appointmentsDb)
    .WaitForStart(identityDb)
    .WaitFor(sql);

builder.AddProject<Projects.OffroadCamping_Appointments_API>("offroadcamping-appointments-api")
    .WithReference(appointmentsDb)
    .WithReference(identityDb)
    .WithReference(kurrentDB)
    .WithReference(migrations)
    .WithReference(cache)
    .WithReference(rabbit)
    .WithEnvironment("AppSettings:Issuer", issuer)
    .WithEnvironment("AppSettings:Audience", audience)
    .WithEnvironment("AppSettings:Token", token)
    .WaitForCompletion(migrations);

builder.Build().Run();