// using MongoDB.Driver;
// using MongoDB.Entities;
using System.Net;
using Polly;
using Polly.Extensions.Http;
using SearchService.Data;
// using SearchService.Entities;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

//Use Microsoft.Extensions.Http.Polly to enable transient fault tolerance for the http request to keep trying until its successful

builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());


var app = builder.Build();


app.UseAuthorization();

app.MapControllers();

// await DB.InitAsync("SearchDb", MongoClientSettings
// .FromConnectionString(builder.Configuration.GetConnectionString("MongoDbConnection")));

// await DB.Index<Item>()
//     .Key(x => x.Make, KeyType.Text)
//     .Key(x => x.Model, KeyType.Text)
//     .Key(x => x.Color, KeyType.Text)
//     .CreateAsync();

try
{
    await DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}


app.Run();


static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
    .HandleTransientHttpError() //handles transient http error. Transient failures are failure which may at some point in the future may no more be failures.
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));  //keeps trying every 3 seconds until it succeeds