using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//add AutoMapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

//add masstransit
builder.Services.AddMassTransit(x =>
{

    //add the ability to send messages to message outbox in case the service bus is down
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        //if the service bus is available, the message would be delivered immediately, else it will go into the outbox and 
        //a will retry every 10 secs, because of the configuration below
        o.QueryDelay = TimeSpan.FromSeconds(10);

        //tell the outbox which database to use
        //NOTE: at the time of coding, masstransit has only for postgres, mongodb and sqlite
        o.UsePostgres();
        //specify option to use bus outbox. In this case messages are delivered to the outbox before sending to the message broker
        o.UseBusOutbox();

    });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

//seeding the database
try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{

    Console.WriteLine(ex);
}

app.Run();
