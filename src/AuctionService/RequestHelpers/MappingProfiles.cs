using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;

namespace AuctionService.RequestHelpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            //create mapping from Auction to AuctionDto and include mapping for Item also, which is a property of Auctions
            CreateMap<Auction, AuctionDto>().IncludeMembers(x => x.Item);
            CreateMap<Item, AuctionDto>();

            //dest = destination, src=source, opt = options
            //The destination Auction class contains a property Item and we want to populate that with values from the source CreateAuctionDto
            //So that's what the "ForMember" does.
            CreateMap<CreateAuctionDto, Auction>().ForMember(dest => dest.Item, opt => opt.MapFrom(src => src));
            CreateMap<CreateAuctionDto, Item>();
            CreateMap<AuctionDto, AuctionCreated>();
        }
    }
}