using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.Common.Models.QueryParameters;
using WoWMarketWatcher.Common.Models.DTOs;
using WoWMarketWatcher.API.Models.Responses;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.Common.Constants;
using Microsoft.AspNetCore.JsonPatch;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.Common.Models.Requests;

namespace WoWMarketWatcher.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ServiceControllerBase
    {
        private readonly UserRepository userRepository;
        private readonly IMapper mapper;


        public UsersController(UserRepository userRepository, IMapper mapper)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<CursorPaginatedResponse<UserDto>>> GetUsersAsync([FromQuery] CursorPaginationParameters searchParams)
        {
            var users = await userRepository.SearchAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<UserDto>.CreateFrom(users, mapper.Map<IEnumerable<UserDto>>, searchParams);

            return Ok(paginatedResponse);
        }

        [HttpGet("{id}", Name = "GetUserAsync")]
        public async Task<ActionResult<UserDto>> GetUserAsync(int id)
        {
            var user = await userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound($"User with id {id} does not exist.");
            }

            var userToReturn = mapper.Map<UserDto>(user);

            return Ok(userToReturn);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            var user = await userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound($"No User with Id {id} found.");
            }

            if (!IsUserAuthorizedForResource(user.Id))
            {
                return Unauthorized("You can only delete your own user.");
            }

            userRepository.Delete(user);
            var saveResults = await userRepository.SaveAllAsync();

            if (!saveResults)
            {
                return BadRequest("Failed to delete the user.");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUserAsync(int id, [FromBody] JsonPatchDocument<UpdateUserRequest> dtoPatchDoc)
        {
            if (dtoPatchDoc == null || dtoPatchDoc.Operations.Count == 0)
            {
                return BadRequest("A JSON patch document with at least 1 operation is required.");
            }

            if (!dtoPatchDoc.IsValid(out var errors))
            {
                return BadRequest(errors);
            }

            var user = await userRepository.GetByIdAsync(id);

            if (user == null)
            {
                return NotFound($"No user with Id {id} found.");
            }

            if (!User.TryGetUserId(out var userId))
            {
                return Unauthorized("You cannot do this.");
            }

            if (!User.IsAdmin() && userId != user.Id)
            {
                return Unauthorized("You cannot do this.");
            }

            var patchDoc = mapper.Map<JsonPatchDocument<User>>(dtoPatchDoc);

            patchDoc.ApplyTo(user);

            await userRepository.SaveAllAsync();

            var userToReturn = mapper.Map<UserDto>(user);

            return Ok(userToReturn);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<CursorPaginatedResponse<RoleDto>>> GetRolesAsync([FromQuery] CursorPaginationParameters searchParams)
        {
            var roles = await userRepository.GetRolesAsync(searchParams);
            var paginatedResponse = CursorPaginatedResponse<RoleDto>.CreateFrom(roles, mapper.Map<IEnumerable<RoleDto>>, searchParams);

            return Ok(paginatedResponse);
        }

        [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
        [HttpPost("{id}/roles")]
        public async Task<ActionResult<UserDto>> AddRolesAsync(int id, [FromBody] EditRoleRequest roleEditDto)
        {
            if (roleEditDto.RoleNames == null || roleEditDto.RoleNames.Length == 0)
            {
                return BadRequest("At least one role must be specified.");
            }

            var user = await userRepository.GetByIdAsync(id);
            var roles = await userRepository.GetRolesAsync();
            var userRoles = user.UserRoles.Select(ur => ur.Role.Name.ToUpper()).ToHashSet();
            var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpper()).ToHashSet();

            var rolesToAdd = roles.Where(role =>
            {
                var upperName = role.Name.ToUpper();
                return selectedRoles.Contains(upperName) && !userRoles.Contains(upperName);
            });

            if (!rolesToAdd.Any())
            {
                return Ok(mapper.Map<UserDto>(user));
            }

            user.UserRoles.AddRange(rolesToAdd.Select(role => new UserRole
            {
                Role = role
            }));

            var success = await userRepository.SaveAllAsync();

            if (!success)
            {
                return BadRequest("Failed to add roles.");
            }

            var userToReturn = mapper.Map<UserDto>(user);

            return Ok(userToReturn);
        }

        [Authorize(Policy = AuthorizationPolicyName.RequireAdminRole)]
        [HttpDelete("{id}/roles")]
        public async Task<ActionResult<UserDto>> RemoveRolesAsync(int id, [FromBody] EditRoleRequest roleEditDto)
        {
            if (roleEditDto.RoleNames == null || roleEditDto.RoleNames.Length == 0)
            {
                return BadRequest("At least one role must be specified.");
            }

            var user = await userRepository.GetByIdAsync(id);
            var roles = await userRepository.GetRolesAsync();
            var userRoles = user.UserRoles.Select(ur => ur.Role.Name.ToUpper()).ToHashSet();
            var selectedRoles = roleEditDto.RoleNames.Select(role => role.ToUpper()).ToHashSet();

            var roleIdsToRemove = roles.Where(role =>
            {
                var upperName = role.Name.ToUpper();
                return selectedRoles.Contains(upperName) && userRoles.Contains(upperName);
            }).Select(role => role.Id).ToHashSet();

            if (roleIdsToRemove.Count == 0)
            {
                return Ok(mapper.Map<UserDto>(user));
            }

            user.UserRoles.RemoveAll(ur => roleIdsToRemove.Contains(ur.RoleId));
            var success = await userRepository.SaveAllAsync();

            if (!success)
            {
                return BadRequest("Failed to remove roles.");
            }

            var userToReturn = mapper.Map<UserDto>(user);

            return Ok(userToReturn);
        }
    }
}