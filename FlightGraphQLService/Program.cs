using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Postgres connection
builder.Services.AddDbContext<FlightDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("FlightDb")));

// GraphQL setup
builder.Services
       .AddGraphQLServer()
       .AddQueryType<Query>();

var app = builder.Build();

app.Urls.Add("http://*:80");
app.MapGraphQL();
app.Run();
