using AuctionService.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Data;

#region 
//Note: DbContext already uses the Repository and Unit of Work patterns
#endregion
public class AuctionDbContext : DbContext
{
    public AuctionDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Auction> Auctions { get; set; }
}

/************************************************************** 
-add migrations
dotnet ef migrations add "[migration name]" -o folderpath

-update database
dotnet ef database update

-docker
docker compose up -d
**/