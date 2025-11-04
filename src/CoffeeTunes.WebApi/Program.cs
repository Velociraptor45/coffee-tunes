using System.IdentityModel.Tokens.Jwt;
using CoffeeTunes.WebApi.Auth;
using CoffeeTunes.WebApi.Contexts;
using CoffeeTunes.WebApi.Endpoints;
using CoffeeTunes.WebApi.Services;
using CoffeeTunes.WebApi.Services.Youtube;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var authOptions = new AuthOptions();
builder.Configuration.Bind(authOptions);
builder.Services.AddSingleton(authOptions);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddCors();
builder.Services.AddHttpContextAccessor();

builder.Services.AddHostedService<DatabaseMigrationBackgroundService>();
builder.Services.AddScoped<FranchiseAccessService>();
builder.Services.AddScoped<BarService>();
builder.Services.AddScoped<YouTubeMetadataProvider>();
builder.Services.AddHttpClient("YouTubeApi", client =>
{
    client.BaseAddress = new Uri("https://www.googleapis.com/youtube/v3/");
});

var ytOptions = new YouTubeOptions();
builder.Configuration.Bind(ytOptions);
builder.Services.AddSingleton(ytOptions);


SetupSecurity();

builder.Services.AddOpenApi("v1", opt =>
{
    opt.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddDbContext<CoffeeTunesDbContext>(opt =>
{
    var connectionString = builder.Configuration.GetConnectionString("Postgres");
    if (connectionString is null)
        throw new InvalidOperationException("Connection string not found");
    
    opt.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt => opt.Title = "CoffeeTunes API");
}

app.UseCors(policyBuilder =>
{
    var corsConfig = new CorsConfig();
    app.Configuration.Bind(corsConfig);

    policyBuilder
        .WithOrigins(corsConfig.AllowedOrigins)
        .WithMethods("GET", "PUT", "POST", "DELETE")
        .WithHeaders("Content-Type", "authorization");
});

app.UseRouting();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.RegisterFranchiseEndpoints();
app.RegisterBarEndpoints();
app.RegisterIngredientEndpoints();

await app.RunAsync();


void SetupSecurity()
{
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

    builder.Services.AddTransient<JwtSecurityTokenHandler>();
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opt =>
        {
            opt.Authority = authOptions.Authority;
            opt.Audience = authOptions.Audience;
            opt.TokenValidationParameters = new()
            {
                ValidTypes = authOptions.ValidTypes,
                NameClaimType = authOptions.NameClaimType,
                RoleClaimType = authOptions.RoleClaimType,
            };
        });
    builder.Services
        .AddAuthorizationBuilder()
        .AddPolicy("User", policy => policy.RequireRole(authOptions.UserRoleName));
}