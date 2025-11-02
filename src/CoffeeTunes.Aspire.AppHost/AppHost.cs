var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithVolume("postgres-data", "/var/lib/postgresql/data")
    .AddDatabase("postgresdb");

var api = builder.AddProject<Projects.CoffeeTunes_WebApi>("api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Local")
    .WithEnvironment("CT_CORS_ORIGIN__0", "https://localhost:5000")
    .WithReference(postgres, "Postgres")
    .WaitFor(postgres);

// builder.AddProject<Projects.CoffeeTunes_Frontend>("frontend")
//     .WithReference(api)
//     .WaitFor(api);

builder.Build().Run();