using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.Requests;

namespace WoWMarketWatcher.API.Core
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            this.CreateUserMaps();
            this.CreateWoWItemMaps();
            this.CreateRealmMaps();
            this.CreateWatchListMaps();
            this.CreateAuctionTimeSeriesMaps();
        }

        private void CreateUserMaps()
        {
            this.CreateMap<User, UserDto>()
                .ForMember(dto => dto.Roles, opt =>
                    opt.MapFrom(u => u.UserRoles.Select(ur => ur.Role.Name)));
            this.CreateMap<RegisterUserRequest, User>();
            this.CreateMap<UserPreference, UserPreferenceDto>();
            this.CreateMap<Role, RoleDto>();
            this.CreateMap<LinkedAccount, LinkedAccountDto>();
            this.CreateMap<JsonPatchDocument<UpdateUserRequest>, JsonPatchDocument<User>>();
            this.CreateMap<Operation<UpdateUserRequest>, Operation<User>>();
        }

        private void CreateWoWItemMaps()
        {
            this.CreateMap<WoWItem, WoWItemDto>();
        }

        private void CreateRealmMaps()
        {
            this.CreateMap<BlizzardConnectedRealm, ConnectedRealm>();
            this.CreateMap<BlizzardLocaleRealm, Realm>();
            this.CreateMap<Realm, RealmDto>();
            this.CreateMap<ConnectedRealm, ConnectedRealmDto>();
        }

        private void CreateWatchListMaps()
        {
            this.CreateMap<WatchList, WatchListDto>();
            this.CreateMap<CreateWatchListRequest, WatchList>();
            this.CreateMap<CreateWatchListForUserRequest, WatchList>();
            this.CreateMap<JsonPatchDocument<UpdateWatchListRequest>, JsonPatchDocument<WatchList>>();
            this.CreateMap<Operation<UpdateWatchListRequest>, Operation<WatchList>>();
        }

        private void CreateAuctionTimeSeriesMaps()
        {
            this.CreateMap<AuctionTimeSeriesEntry, AuctionTimeSeriesEntryDto>();
        }
    }
}