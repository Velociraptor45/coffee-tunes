var builder = DistributedApplication.CreateBuilder(args);

// Add the WebApi project
var api = builder.AddProject("webapi", "../CoffeeTunes.WebApi/CoffeeTunes.WebApi.csproj");

// Add the Frontend project and reference the API
builder.AddProject("frontend", "../CoffeeTunes.Frontend/CoffeeTunes.Frontend.csproj")
    .WithReference(api);

builder.Build().Run();
