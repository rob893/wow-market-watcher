using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs.Users;
using WoWMarketWatcher.API.Models.QueryParameters;
using WoWMarketWatcher.API.Models.Requests.Users;
using WoWMarketWatcher.API.Models.Responses.Pagination;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/users")]
    [ApiVersion("1")]
    [ApiController]
    public sealed class UsersController : ServiceControllerBase
    {
        private readonly IUserRepository userRepository;

        private readonly IMapper mapper;

        public UsersController(IUserRepository userRepository, IMapper mapper, ICorrelationIdService correlationIdService)
            : base(correlationIdService)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<UserDto>>> GetUsersAsync([FromQuery] CursorPaginationQueryParameters searchParams)
        {
            var users = await this.userRepository.SearchAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<UserDto>>(users.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetUserAsync")]
        public async Task<ActionResult<UserDto>> GetUserAsync(int id)
        {
            var user = await this.userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return this.NotFound($"User with id {id} does not exist.");
            }

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.Ok(userToReturn);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            var user = await this.userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return this.NotFound($"No User with Id {id} found.");
            }

            if (!this.IsUserAuthorizedForResource(user.Id))
            {
                return this.Unauthorized("You can only delete your own user.");
            }

            this.userRepository.Remove(user);
            var saveResults = await this.userRepository.SaveAllAsync();

            if (!saveResults)
            {
                return this.BadRequest("Failed to delete the user.");
            }

            return this.NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUserAsync(int id, [FromBody] JsonPatchDocument<UpdateUserRequest> dtoPatchDoc)
        {
            if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
            {
                return this.BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            if (!dtoPatchDoc.IsValid(out var errors))
            {
                return this.BadRequest(errors);
            }

            var user = await this.userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return this.NotFound($"No user with Id {id} found.");
            }

            if (!this.User.TryGetUserId(out var userId))
            {
                return this.Unauthorized("You cannot do this.");
            }

            if (!this.User.IsAdmin() && userId != user.Id)
            {
                return this.Unauthorized("You cannot do this.");
            }

            var patchDoc = this.mapper.Map<JsonPatchDocument<User>>(dtoPatchDoc);

            patchDoc.ApplyTo(user);

            await this.userRepository.SaveAllAsync();

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.Ok(userToReturn);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<CursorPaginatedResponse<RoleDto>>> GetRolesAsync([FromQuery] CursorPaginationQueryParameters searchParams)
        {
            var roles = await this.userRepository.GetRolesAsync(searchParams);
            var paginatedResponse = this.mapper.Map<CursorPaginatedResponse<RoleDto>>(roles.ToCursorPaginatedResponse(searchParams));

            return this.Ok(paginatedResponse);
        }

        [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult<UserDto>> AddRolesAsync(int id, [FromBody] EditRoleRequest roleEditDto)
        {
            if (roleEditDto == null)
            {
                throw new ArgumentNullException(nameof(roleEditDto));
            }

            if (roleEditDto.RoleNames == null || roleEditDto.RoleNames.Count == 0)
            {
                return this.BadRequest("At least one role must be specified.");
            }

            var user = await this.userRepository.GetByIdAsync(id);
            var roles = await this.userRepository.GetRolesAsync();
            var userRoles = user.UserRoles.Select(ur => ur.Role.Name.ToUpperInvariant()).ToHashSet();
            var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpperInvariant()).ToHashSet();

            var rolesToAdd = roles.Where(role =>
            {
                var upperName = role.Name.ToUpperInvariant();
                return selectedRoles.Contains(upperName) && !userRoles.Contains(upperName);
            });

            if (!rolesToAdd.Any())
            {
                return this.Ok(this.mapper.Map<UserDto>(user));
            }

            user.UserRoles.AddRange(rolesToAdd.Select(role => new UserRole
            {
                Role = role
            }));

            var success = await this.userRepository.SaveAllAsync();

            if (!success)
            {
                return this.BadRequest("Failed to add roles.");
            }

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.Ok(userToReturn);
        }

        [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult<UserDto>> RemoveRolesAsync(int id, [FromBody] EditRoleRequest roleEditDto)
        {
            if (roleEditDto == null)
            {
                throw new ArgumentNullException(nameof(roleEditDto));
            }

            if (roleEditDto.RoleNames == null || roleEditDto.RoleNames.Count == 0)
            {
                return this.BadRequest("At least one role must be specified.");
            }

            var user = await this.userRepository.GetByIdAsync(id);
            var roles = await this.userRepository.GetRolesAsync();
            var userRoles = user.UserRoles.Select(ur => ur.Role.Name.ToUpperInvariant()).ToHashSet();
            var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpperInvariant()).ToHashSet();

            var roleIdsToRemove = roles.Where(role =>
            {
                var upperName = role.Name.ToUpperInvariant();
                return selectedRoles.Contains(upperName) && userRoles.Contains(upperName);
            }).Select(role => role.Id).ToHashSet();

            if (roleIdsToRemove.Count == 0)
            {
                return this.Ok(this.mapper.Map<UserDto>(user));
            }

            user.UserRoles.RemoveAll(ur => roleIdsToRemove.Contains(ur.RoleId));
            var success = await this.userRepository.SaveAllAsync();

            if (!success)
            {
                return this.BadRequest("Failed to remove roles.");
            }

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.Ok(userToReturn);
        }
    }
}