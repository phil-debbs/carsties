using System.Text.Json;
using MongoDB.Driver;
using MongoDB.Entities;
using SearchService.Entities;
using SearchService.Services;

namespace SearchService.Data
{
    public class DbInitializer
    {
        public static async Task InitDb(WebApplication app)
        {
            //Initialize mongoDB entities
            //Note that all of the functionalities provided by mongodb entities are effectively
            //static classes so there's no need to create new instance of mongodb
            await DB.InitAsync("SearchDb",
            MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

            //create an index for the Item
            await DB.Index<Item>()
                .Key(x => x.Make, KeyType.Text)
                .Key(x => x.Model, KeyType.Text)
                .Key(x => x.Color, KeyType.Text)
                .CreateAsync();

            // //check if database already has data
            // var count = await DB.CountAsync<Item>();

            // if (count == 0)
            // {
            //     Console.WriteLine("No data - will attempt to seed.");

            //     var itemData = await File.ReadAllTextAsync("Data/auctions.json");

            //     var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            //     var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

            //     await DB.SaveAsync(items);

            //     Console.WriteLine("Seeded successfully.");
            // }

            using var scope = app.Services.CreateScope();
            //get the aution httpclient to get the data from Auction service
            var httpClient = scope.ServiceProvider.GetRequiredService<AuctionSvcHttpClient>();

            var items = await httpClient.GetItemsForSearchDb();

            Console.WriteLine(items.Count + " returned from the auction service");

            if (items.Count > 0) await DB.SaveAsync(items);
        }
    }
}