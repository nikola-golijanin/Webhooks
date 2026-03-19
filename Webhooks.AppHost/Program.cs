var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("WebhooksDatabase");

var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();

builder.AddProject<Projects.Webhooks_API>("webhooks-api")
    .WithReference(postgres)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.AddProject<Projects.Webhooks_Processing>("webhooks-processing")
    .WithReference(postgres)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(rabbitmq);

builder.Build().Run();
