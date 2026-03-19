var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", "postgres");

var postgres = builder
    .AddPostgres("postgres", postgresPassword)
    .WithHostPort(5432)
    .AddDatabase("WebhooksDatabase");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

var webhooksApi = builder.AddProject<Projects.Webhooks_API>("webhooks-api")
    .WithReference(postgres)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

var testListener = builder.AddProject<Projects.Webhooks_TestListener>("webhooks-test-listener")
    .WithReference(webhooksApi)
    .WaitFor(webhooksApi);

var _ = builder.AddProject<Projects.Webhooks_Processing>("webhooks-processing")
    .WithReference(postgres)
    .WithReference(rabbitmq)
    .WithReference(testListener)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.Build().Run();
