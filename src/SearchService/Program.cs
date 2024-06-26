// using MongoDB.Driver;
// using MongoDB.Entities;
using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
// using SearchService.Entities;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
//add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
//Use Microsoft.Extensions.Http.Polly to enable transient fault tolerance for the http request to keep trying until its successful

builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());

//add masstransit
builder.Services.AddMassTransit(x =>
{
    //register our consumers with Mass transit
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionUpdatedConsumer>();
    x.AddConsumersFromNamespaceContaining<AuctionDeletedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            e.UseMessageRetry(r => r.Interval(5, 5));

            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
            e.ConfigureConsumer<AuctionUpdatedConsumer>(context);
            e.ConfigureConsumer<AuctionDeletedConsumer>(context);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();


app.UseAuthorization();

app.MapControllers();


//This is to allow our search service app to accept request event when the Auction service is not running.
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        //NOTE: when we hit this line wihtout the Auction service running, the httpclient request keeps firing every 3 seconds 
        //This means that the execution gets stuck here and the app.Run() that follows never gets called.
        //In this case, although the search service is running, endpoints have not been instantiated yet.
        //This can be fixed with app.Lifetime events
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
});



app.Run();


static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
    .HandleTransientHttpError() //handles transient http error. Transient failures are failure which may at some point in the future may no more be failures.
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));  //keeps trying every 3 seconds until it succeeds