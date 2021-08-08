using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WoWMarketWatcher.API.Constants;
using WoWMarketWatcher.API.Data.Repositories;
using WoWMarketWatcher.API.Entities;
using WoWMarketWatcher.API.Extensions;
using WoWMarketWatcher.API.Models;
using WoWMarketWatcher.API.Models.DTOs.Users;
using WoWMarketWatcher.API.Models.Requests.Auth;
using WoWMarketWatcher.API.Models.Responses.Auth;
using WoWMarketWatcher.API.Models.Settings;
using WoWMarketWatcher.API.Services;

namespace WoWMarketWatcher.API.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthController : ServiceControllerBase
    {
        private readonly IUserRepository userRepository;

        private readonly AuthenticationSettings authSettings;

        private readonly IEmailService emailService;

        private readonly IMapper mapper;

        public AuthController(
            IUserRepository userRepository,
            IEmailService emailService,
            IOptions<AuthenticationSettings> authSettings,
            IMapper mapper,
            ICorrelationIdService correlationIdService)
                : base(correlationIdService)
        {
            this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            this.emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            this.authSettings = authSettings?.Value ?? throw new ArgumentNullException(nameof(authSettings));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> RegisterAsync([FromBody] RegisterUserRequest userForRegisterDto)
        {
            if (userForRegisterDto == null)
            {
                return this.BadRequest();
            }

            var user = this.mapper.Map<User>(userForRegisterDto);

            var result = await this.userRepository.CreateUserWithPasswordAsync(user, userForRegisterDto.Password);

            if (!result.Succeeded)
            {
                return this.BadRequest(result.Errors.Select(e => e.Description).ToList());
            }

            var token = this.GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expiration = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
                DeviceId = userForRegisterDto.DeviceId
            });

            await this.userRepository.SaveAllAsync();

            await this.SendConfirmEmailLink(user);

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.CreatedAtRoute("GetUserAsync", new { controller = "Users", id = user.Id }, new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = userToReturn
            });
        }

        [HttpPost("forgotPassword")]
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

        [HttpPost("resetPassword")]
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

        [HttpPost("confirmEmail")]
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

        [HttpPost("register/google")]
        public async Task<ActionResult<LoginResponse>> RegisterWithGoogleAccountAsync([FromBody] RegisterUserUsingGoolgleRequest userForRegisterDto)
        {
            try
            {
                if (userForRegisterDto == null)
                {
                    return this.BadRequest();
                }

                var validatedToken = await GoogleJsonWebSignature.ValidateAsync(userForRegisterDto.IdToken, new GoogleJsonWebSignature.ValidationSettings { Audience = this.authSettings.GoogleOAuthAudiences });

                var user = new User
                {
                    UserName = userForRegisterDto.UserName,
                    Email = validatedToken.Email,
                    EmailConfirmed = validatedToken.EmailVerified,
                    LinkedAccounts = new List<LinkedAccount>
                    {
                        new LinkedAccount
                        {
                            Id = validatedToken.Subject,
                            LinkedAccountType = LinkedAccountType.Google
                        }
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

                var token = this.GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    Expiration = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
                    DeviceId = userForRegisterDto.DeviceId
                });

                await this.userRepository.SaveAllAsync();

                var userToReturn = this.mapper.Map<UserDto>(user);

                return this.CreatedAtRoute("GetUserAsync", new { controller = "Users", id = user.Id }, new LoginResponse
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
        /// Logs the user in
        /// </summary>
        /// <param name="userForLoginDto"></param>
        /// <returns>200 with user object on success. 401 on failure.</returns>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest userForLoginDto)
        {
            if (userForLoginDto == null)
            {
                return this.BadRequest();
            }

            var user = await this.userRepository.GetByUsernameAsync(userForLoginDto.Username, user => user.RefreshTokens);

            if (user == null)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            var result = await this.userRepository.CheckPasswordAsync(user, userForLoginDto.Password);

            if (!result)
            {
                return this.Unauthorized("Invalid username or password.");
            }

            user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTime.UtcNow || token.DeviceId == userForLoginDto.DeviceId);

            var token = this.GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expiration = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
                DeviceId = userForLoginDto.DeviceId
            });

            await this.userRepository.SaveAllAsync();

            var userToReturn = this.mapper.Map<UserDto>(user);

            return this.Ok(new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                User = userToReturn
            });
        }

        [HttpPost("login/google")]
        public async Task<ActionResult<LoginResponse>> LoginGoogleAsync([FromBody] GoogleLoginRequest userForLoginDto)
        {
            try
            {
                if (userForLoginDto == null)
                {
                    return this.BadRequest();
                }

                var validatedToken = await GoogleJsonWebSignature.ValidateAsync(userForLoginDto.IdToken, new GoogleJsonWebSignature.ValidationSettings { Audience = this.authSettings.GoogleOAuthAudiences });

                var user = await this.userRepository.GetByLinkedAccountAsync(validatedToken.Subject, LinkedAccountType.Google, user => user.RefreshTokens);

                if (user == null)
                {
                    return this.NotFound("No account found for this Google account.");
                }

                user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTime.UtcNow || token.DeviceId == userForLoginDto.DeviceId);

                var token = this.GenerateJwtToken(user);
                var refreshToken = GenerateRefreshToken();

                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    Expiration = DateTimeOffset.UtcNow.AddMinutes(authSettings.RefreshTokenExpirationTimeInMinutes),
                    DeviceId = userForLoginDto.DeviceId
                });

                await this.userRepository.SaveAllAsync();

                var userToReturn = this.mapper.Map<UserDto>(user);

                return this.Ok(new LoginResponse
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

        [HttpPost("refreshToken")]
        public async Task<ActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest refreshTokenDto)
        {
            if (refreshTokenDto == null)
            {
                return this.BadRequest();
            }

            // Still validate the passed in token, but ignore its expiration date by setting validate lifetime to false
            var validationParameters = new TokenValidationParameters
            {
                ClockSkew = TimeSpan.Zero,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(this.authSettings.APISecrect)),
                RequireSignedTokens = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                RequireExpirationTime = true,
                ValidateLifetime = false,
                ValidAudience = this.authSettings.TokenAudience,
                ValidIssuer = this.authSettings.TokenIssuer
            };

            ClaimsPrincipal tokenClaims;

            try
            {
                tokenClaims = new JwtSecurityTokenHandler().ValidateToken(refreshTokenDto.Token, validationParameters, out var rawValidatedToken);
            }
            catch (Exception e)
            {
                return this.Unauthorized(e.Message);
            }

            var userIdClaim = tokenClaims.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            {
                return this.Unauthorized("Invalid token.");
            }

            var user = await this.userRepository.GetByIdAsync(userId, user => user.RefreshTokens);

            if (user == null)
            {
                return this.Unauthorized("Invalid token.");
            }

            user.RefreshTokens.RemoveAll(token => token.Expiration <= DateTime.UtcNow);

            var currentRefreshToken = user.RefreshTokens.FirstOrDefault(token => token.DeviceId == refreshTokenDto.DeviceId && token.Token == refreshTokenDto.RefreshToken);

            if (currentRefreshToken == null)
            {
                await this.userRepository.SaveAllAsync();
                return this.Unauthorized("Invalid token.");
            }

            user.RefreshTokens.Remove(currentRefreshToken);

            var token = this.GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                Expiration = DateTimeOffset.UtcNow.AddMinutes(this.authSettings.RefreshTokenExpirationTimeInMinutes),
                DeviceId = refreshTokenDto.DeviceId
            });

            await this.userRepository.SaveAllAsync();

            return this.Ok(new
            {
                token,
                refreshToken
            });
        }

        private static string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            return Convert.ToBase64String(randomNumber);
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(AppClaimTypes.EmailVerified, user.EmailConfirmed.ToString(), ClaimValueTypes.Boolean),
                new Claim(AppClaimTypes.MembershipType, user.MembershipLevel.ToString())
            };

            if (user.FirstName != null)
            {
                claims.Add(new Claim(ClaimTypes.GivenName, user.FirstName));
            }

            if (user.LastName != null)
            {
                claims.Add(new Claim(ClaimTypes.Surname, user.LastName));
            }

            if (user.UserRoles != null)
            {
                foreach (var role in user.UserRoles.Select(r => r.Role.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.authSettings.APISecrect));

            if (key.KeySize < 128)
            {
                throw new InvalidJwtException("API Secret must be longer");
            }

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(this.authSettings.TokenExpirationTimeInMinutes),
                NotBefore = DateTime.UtcNow,
                SigningCredentials = creds,
                Audience = this.authSettings.TokenAudience,
                Issuer = this.authSettings.TokenIssuer
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
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