using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.DTOs.Alerts;
using WoWMarketWatcher.API.Models.DTOs.Realms;
using WoWMarketWatcher.API.Models.DTOs.Users;
using WoWMarketWatcher.API.Models.Requests.Alerts;
using WoWMarketWatcher.API.Models.Requests.Auth;
using WoWMarketWatcher.API.Models.Requests.WatchLists;
using WoWMarketWatcher.API.Models.Responses.Blizzard;
using WoWMarketWatcher.API.Models.Responses.Pagination;

namespace WoWMarketWatcher.API.Core
{
    public sealed class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            this.CreateCoreMaps();
            this.CreateUserMaps();
            this.CreateWoWItemMaps();
            this.CreateRealmMaps();
            this.CreateWatchListMaps();
            this.CreateAuctionTimeSeriesMaps();
            this.CreateAlertMaps();
        }

        private void CreateCoreMaps()
        {
            this.CreateMap(typeof(CursorPaginatedResponse<,>), typeof(CursorPaginatedResponse<,>))
                .ForAllMembers(opts => opts.AllowNull());
            this.CreateMap(typeof(CursorPaginatedResponse<,>), typeof(CursorPaginatedResponse<>))
                .ForAllMembers(opts => opts.AllowNull());
            this.CreateMap(typeof(CursorPaginatedResponse<>), typeof(CursorPaginatedResponse<,>))
                .ForAllMembers(opts => opts.AllowNull());
            this.CreateMap(typeof(CursorPaginatedResponse<>), typeof(CursorPaginatedResponse<>))
                .ForAllMembers(opts => opts.AllowNull());
            this.CreateMap(typeof(CursorPaginatedResponseEdge<>), typeof(CursorPaginatedResponseEdge<>));

            this.CreateMap(typeof(JsonPatchDocument<>), typeof(JsonPatchDocument<>));
            this.CreateMap(typeof(Operation<>), typeof(Operation<>));
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
        }

        private void CreateWoWItemMaps()
        {
            this.CreateMap<WoWItem, WoWItemDto>();
        }

        private void CreateRealmMaps()
        {
            this.CreateMap<BlizzardConnectedRealm, ConnectedRealm>()
                .ForMember(realm => realm.Population, opt => opt.MapFrom(blizzRealm => blizzRealm.Population.Name.EnUS));
            this.CreateMap<BlizzardLocaleRealm, Realm>()
                .ForMember(realm => realm.Name, opt => opt.MapFrom(blizzRealm => blizzRealm.Name.EnUS))
                .ForMember(realm => realm.Region, opt => opt.MapFrom(blizzRealm => blizzRealm.Region.Name.EnUS))
                .ForMember(realm => realm.Category, opt => opt.MapFrom(blizzRealm => blizzRealm.Category.EnUS))
                .ForMember(realm => realm.Type, opt => opt.MapFrom(blizzRealm => blizzRealm.Type.Name.EnUS));
            this.CreateMap<Realm, RealmDto>();
            this.CreateMap<ConnectedRealm, ConnectedRealmDto>();
        }

        private void CreateWatchListMaps()
        {
            this.CreateMap<WatchList, WatchListDto>();
            this.CreateMap<CreateWatchListRequest, WatchList>();
            this.CreateMap<CreateWatchListForUserRequest, WatchList>();
        }

        private void CreateAlertMaps()
        {
            this.CreateMap<Alert, AlertDto>();
            this.CreateMap<AlertCondition, AlertConditionDto>();
            this.CreateMap<AlertAction, AlertActionDto>();
            this.CreateMap<CreateAlertForUserRequest, Alert>();
            this.CreateMap<CreateAlertActionRequest, AlertAction>();
            this.CreateMap<CreateAlertConditionRequest, AlertCondition>();
            this.CreateMap<PutAlertActionRequest, AlertAction>();
        }

        private void CreateAuctionTimeSeriesMaps()
        {
            this.CreateMap<AuctionTimeSeriesEntry, AuctionTimeSeriesEntryDto>();
        }
    }
}