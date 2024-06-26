using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Entities;

namespace SearchService.Consumers
{
    public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
    {
        private readonly IMapper _mapper;

        //inject automapper
        public AuctionUpdatedConsumer(IMapper mapper)
        {
            _mapper = mapper;
        }
        public async Task Consume(ConsumeContext<AuctionUpdated> context)
        {
            Console.WriteLine("--> Consuming auctions updated: " + context.Message.Id);

            var item = _mapper.Map<Item>(context.Message);

            var result = await DB.Update<Item>()
                    .MatchID(item.ID)
                    .ModifyOnly(x => new { x.Color, x.Make, x.Model, x.Mileage }, item)
                    .ExecuteAsync();

            if (!result.IsAcknowledged)
                throw new MessageException(typeof(AuctionUpdated), "Problem updating mongodb");
        }
    }
}