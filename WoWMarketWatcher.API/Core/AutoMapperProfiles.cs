using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Models.DTOs;
using WoWMarketWatcher.API.Models.QueryParameters;

namespace WoWMarketWatcher.API.Core
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateUserMaps();
        }

        private void CreateUserMaps()
        {
            CreateMap<User, UserDto>()
                .ForMember(dto => dto.Roles, opt =>
                    opt.MapFrom(u => u.UserRoles.Select(ur => ur.Role.Name)));
            CreateMap<RegisterUserDto, User>();
            CreateMap<Role, RoleDto>();
            CreateMap<LinkedAccount, LinkedAccountDto>();
            CreateMap<JsonPatchDocument<UpdateUserDto>, JsonPatchDocument<User>>();
            CreateMap<Operation<UpdateUserDto>, Operation<User>>();
        }
    }
}