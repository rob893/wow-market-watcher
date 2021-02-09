using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.Common.Models.Requests;

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
            CreateMap<RegisterUserRequest, User>();
            CreateMap<Role, RoleDto>();
            CreateMap<LinkedAccount, LinkedAccountDto>();
            CreateMap<JsonPatchDocument<UpdateUserRequest>, JsonPatchDocument<User>>();
            CreateMap<Operation<UpdateUserRequest>, Operation<User>>();
        }
    }
}