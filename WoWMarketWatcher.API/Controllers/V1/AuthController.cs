using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models.DTOs.Users;
using WoWMarketWatcher.API.Models.Entities;
using WoWMarketWatcher.API.Models.Requests.Auth;
using WoWMarketWatcher.API.Models.Responses.Auth;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;
using static WoWMarketWatcher.API.Utilities.UtilityFunctions;

namespace WoWMarketWatcher.API.Controllers.V1
{
    [Route("api/v{version:apiVersion}/auth")]
    [ApiVersion("1")]
    [AllowAnonymous]
    [ApiController]
    public sealed class AuthController : ServiceControllerBase
    {
        private readonly IUserRepository userRepository;

        private readonly IJwtTokenService jwtTokenService;

        private readonly AuthenticationSettings authSettings;

        private readonly IEmailService emailService;

        private readonly IMapper mapper;

        public AuthController(
            IUserRepository userRepository,
            IJwtTokenService jwtTokenService,
            IEmailService emailService,
            IOptions<AuthenticationSettings> authSettings,
            IMapper mapper,
            ICorrelationIdService correlationIdService)
                : base(correlationIdService)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
            this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="registerUserRequest">The register request object.</param>
        /// <returns>The user object and tokens.</returns>
        /// <response code="201">The user object and tokens.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("register", Name = nameof(RegisterAsync))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<LoginResponse>> RegisterAsync([FromBody] RegisterUserRequest registerUserRequest)
        {
            if (registerUserRequest == null)
            {
                return this.BadRequest();
            }

            var user = this.mapper.Map<User>(registerUserRequest);
            user.Preferences = new UserPreference
            {
                UITheme = UITheme.Dark
            };

            var result = await this.userRepository.CreateUserWithPasswordAsync(user, registerUserRequest.Password);

            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors.Select(e => e.Description).ToList());
            }

            var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
            var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, registerUserRequest.DeviceId);

            await this.SendConfirmEmailLink(user);

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.CreatedAtRoute(
                nameof(UsersController.GetUserAsync),
                new
                {
                    controller = GetControllerName<UsersController>(),
                    id = user.Id
                },
                new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userToReturn
                });
        }

        /// <summary>
        /// Registers a new user using a google login and links their account to their google account.
        /// </summary>
        /// <param name="registerUserRequest">The register request object.</param>
        /// <returns>The user object and tokens.</returns>
        /// <response code="201">If the user was registered.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("register/google", Name = nameof(RegisterWithGoogleAccountAsync))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<LoginResponse>> RegisterWithGoogleAccountAsync([FromBody] RegisterUserUsingGoolgleRequest registerUserRequest)
        {
            try
            {
                if (registerUserRequest == null)
                {
                    return this.BadRequest();
                }

                var validatedToken = await GoogleJsonWebSignature.ValidateAsync(
                    registerUserRequest.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings { Audience = this.authSettings.GoogleOAuthAudiences });

                var user = new User
                {
                    UserName = registerUserRequest.UserName,
                    Email = validatedToken.Email,
                    EmailConfirmed = validatedToken.EmailVerified,
                    LinkedAccounts = new List<LinkedAccount>
                    {
                        new LinkedAccount
                        {
                            Id = validatedToken.Subject,
                            LinkedAccountType = LinkedAccountType.Google
                        }
                    },
                    Preferences = new UserPreference
                    {
                        UITheme = UITheme.Dark
                    }
                };

                var createResult = await this.userRepository.CreateUserWithAsync(user);

                if (!createResult.Succeeded)
                {
                    return this.BadRequest(createResult.Errors.Select(e => e.Description).ToList());
                }

                if (!validatedToken.EmailVerified)
                {
                    await this.SendConfirmEmailLink(user);
                }

                var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
                var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, registerUserRequest.DeviceId);

                var userToReturn = this.mapper.Map<UserDto>(user);

                return this.CreatedAtRoute(
                    nameof(UsersController.GetUserAsync),
                    new
                    {
                        controller = GetControllerName<UsersController>(),
                        id = user.Id
                    },
                    new LoginResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        User = userToReturn
                    });
            }
            catch (InvalidJwtException)
            {
                return this.Unauthorized("Invaid Id Token.");
            }
            catch
            {
                return this.InternalServerError("Unable to register using Google account.");
            }
        }

        /// <summary>
        /// Logs the user in.
        /// </summary>
        /// <param name="loginRequest">The login request object.</param>
        /// <returns>The user object and tokens.</returns>
        /// <response code="200">The user object and tokens.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided login information is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("login", Name = nameof(LoginAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                return this.BadRequest();
            }

            var user = await this.userRepository.GetByUsernameAsync(loginRequest.Username, user => user.RefreshTokens);

            if (user == null)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            var result = await this.userRepository.CheckPasswordAsync(user, loginRequest.Password);

            if (!result)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
            var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, loginRequest.DeviceId);

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.Ok(
                new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    User = userToReturn
                });
        }

        /// <summary>
        /// Logs the user in using Google callback credentials.
        /// </summary>
        /// <param name="loginRequest">The login request object.</param>
        /// <returns>The user object and tokens.</returns>
        /// <response code="200">If the user was logged in.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided login information is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("login/google", Name = nameof(LoginGoogleAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<LoginResponse>> LoginGoogleAsync([FromBody] GoogleLoginRequest loginRequest)
        {
            try
            {
                if (loginRequest == null)
                {
                    return this.BadRequest();
                }

                var validatedToken = await GoogleJsonWebSignature.ValidateAsync(loginRequest.IdToken, new GoogleJsonWebSignature.ValidationSettings { Audience = this.authSettings.GoogleOAuthAudiences });

                var user = await this.userRepository.GetByLinkedAccountAsync(validatedToken.Subject, LinkedAccountType.Google, user => user.RefreshTokens);

                if (user == null)
                {
                    return this.NotFound("No account found for this Google account.");
                }

                var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
                var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, loginRequest.DeviceId);

                var userToReturn = this.mapper.Map<UserDto>(user);

                return this.Ok(
                    new LoginResponse
                    {
                        Token = token,
                        RefreshToken = refreshToken,
                        User = userToReturn
                    });
            }
            catch (InvalidJwtException)
            {
                return this.Unauthorized("Invaid Id Token");
            }
            catch
            {
                return this.InternalServerError("Unable to login with Google.");
            }
        }

        /// <summary>
        /// Refreshes a user's access token.
        /// </summary>
        /// <param name="refreshTokenRequest">The refresh token request.</param>
        /// <returns>A new set of tokens.</returns>
        /// <response code="200">If the token was refreshed.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="401">If provided token pair was invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("refreshToken", Name = nameof(RefreshTokenAsync))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            if (refreshTokenRequest == null)
            {
                return this.BadRequest();
            }

            var (isTokenEligibleForRefresh, user) = await this.jwtTokenService.IsTokenEligibleForRefreshAsync(
                refreshTokenRequest.Token,
                refreshTokenRequest.RefreshToken,
                refreshTokenRequest.DeviceId);

            if (!isTokenEligibleForRefresh || user == null)
            {
                return this.Unauthorized("Invalid token.");
            }

            var token = this.jwtTokenService.GenerateJwtTokenForUser(user);
            var refreshToken = await this.jwtTokenService.GenerateAndSaveRefreshTokenForUserAsync(user, refreshTokenRequest.DeviceId);

            return this.Ok(
                new RefreshTokenResponse
                {
                    Token = token,
                    RefreshToken = refreshToken
                });
        }

        /// <summary>
        /// Sends a link to reset password if a user forgot.
        /// </summary>
        /// <param name="request">The forgot password request.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the password reset link was sent.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("forgotPassword", Name = nameof(ForgotPasswordAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ForgotPasswordAsync([FromBody] ForgotPasswordRequest request)
        {
            if (request == null)
            {
                return this.BadRequest();
            }

            var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return this.BadRequest();
            }

            var token = await this.userRepository.UserManager.GeneratePasswordResetTokenAsync(user);

            var confLink = $"{this.authSettings.ForgotPasswordCallbackUrl}?token={token}&email={user.Email}";
            await this.emailService.SendEmailAsync(user.Email, "Reset your password", $"Please click {confLink} to reset your password");

            return this.NoContent();
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="request">The reset password request.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the password was reset.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("resetPassword", Name = nameof(ResetPasswordAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest request)
        {
            if (request == null)
            {
                return this.BadRequest();
            }

            var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return this.BadRequest();
            }

            var result = await this.userRepository.UserManager.ResetPasswordAsync(user, request.Token, request.Password);

            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors.Select(e => e.Description).ToList());
            }

            return this.NoContent();
        }

        /// <summary>
        /// Confirms a user's email.
        /// </summary>
        /// <param name="request">The confirm email request.</param>
        /// <returns>No content.</returns>
        /// <response code="204">If the email was confirmed.</response>
        /// <response code="400">If the request is invalid.</response>
        /// <response code="500">If an unexpected server error occured.</response>
        /// <response code="504">If the server took too long to respond.</response>
        [HttpPost("confirmEmail", Name = nameof(ConfirmEmailAsync))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<ActionResult> ConfirmEmailAsync([FromBody] ConfirmEmailRequest request)
        {
            if (request == null)
            {
                return this.BadRequest();
            }

            var user = await this.userRepository.UserManager.FindByEmailAsync(request.Email);

            if (user == null)
            {
                return this.BadRequest("Unable to confirm email.");
            }

            var decoded = request.Token.ConvertToStringFromBase64Url();

            var confirmResult = await this.userRepository.UserManager.ConfirmEmailAsync(user, decoded);

            if (!confirmResult.Succeeded)
            {
                return this.BadRequest(confirmResult.Errors.Select(e => e.Description).ToList());
            }

            return this.NoContent();
        }

        private async Task SendConfirmEmailLink(User user)
        {
            var emailToken = await this.userRepository.UserManager.GenerateEmailConfirmationTokenAsync(user);
            var encoded = emailToken.ConvertToBase64Url();
            var confLink = $"{this.authSettings.ConfirmEmailCallbackUrl}?token={encoded}&email={user.Email}";
            await this.emailService.SendEmailAsync(user.Email, "Confirm your email", $"Please click {confLink} to confirm email");
        }
    }
}