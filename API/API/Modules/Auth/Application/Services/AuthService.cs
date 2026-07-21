using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Auth.Application.DTO;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Authorization.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Settings.Application.DTO;
using Settings.Application.Interfaces;
using Settings.Domain.Entities;
using Users.Domain.Entities;

namespace Auth.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;
        private readonly ILoginTrackingService _trackingService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILdapService _ldapService;
        private readonly ISettingService _settingService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IPermissionService permissionService,
            IConfiguration configuration,
            ILoginTrackingService trackingService,
            IHttpContextAccessor httpContextAccessor,
            ILdapService ldapService,
            ISettingService settingService
        )
        {
            _userManager = userManager;
            _permissionService = permissionService;
            _configuration = configuration;
            _trackingService = trackingService;
            _httpContextAccessor = httpContextAccessor;
            _ldapService = ldapService;
            _settingService = settingService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = _userManager.Users.FirstOrDefault(u => u.Matricule == request.Username);

            var httpContext = _httpContextAccessor.HttpContext;

            var ip = httpContext?.Connection?.RemoteIpAddress?.ToString();
            var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

            // Si aucun utilisateur n'existe
            if (user == null)
            {
    
                // On sauvegarde les tentatives de connexions
                await _trackingService.TrackAsync(
                    new LoginHistory
                    {
                        Username = request.Username,
                        LoginDate = DateTime.UtcNow,
                        IsSuccess = false,
                        IpAddress = ip,
                        UserAgent = userAgent,
                        FailureReason = "Invalid credentials",
                    }
                );

                return new LoginResponse
                {
                    Success = false,
                    Message = "Les paramètres de connexions sont incorrects.",
                };
            }

            // Si le compte de l'utilisateur est désactivé
            if (!user.IsActive)
            {
      
                // On sauvegarde la tentative de connexion
                await _trackingService.TrackAsync(
                    new LoginHistory
                    {
                        Username = request.Username,
                        LoginDate = DateTime.UtcNow,
                        IsSuccess = false,
                        IpAddress = ip,
                        UserAgent = userAgent,
                        FailureReason = "Compte utilisateur désactivé",
                    }
                );

                return new LoginResponse
                {
                    Success = false,
                    Message = "Le Compte utilisateur a été désactivé.",
                };
            }

            var roles = await _userManager.GetRolesAsync(user);
            var permissions = await _permissionService.GetPermissionsByUserId(user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName!),
            };

            // Roles
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            // Permissions
            claims.AddRange(permissions.Select(p => new Claim("permission", p)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddHours(2);

            var token = new JwtSecurityToken( 
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );
 

            // On récupère le mode d'authentification
            var authMode = await _settingService.GetValueAsync(SettingKeys.AUTH_MODE);
            return authMode == "LDAP"
                ? await AuthenticateLdapAsync(user, request, ip, userAgent, token, expiration)
                : await AuthenticateIdentityAsync(user, request, ip, userAgent, token, expiration);
        }

        private async Task<LoginResponse> AuthenticateLdapAsync(
            ApplicationUser user,
            LoginRequest request,
            string ip,
            string userAgent,
            JwtSecurityToken token,
            DateTime expiration
        )
        {
            // Paramètre de connexion avec LDAP
            var ldapConfig = await _settingService.GetAsync<LdapConfig>(SettingKeys.LDAP_CONFIG);

            var isValid = await _ldapService.AuthenticateAsync(
                request.Username,
                request.Password,
                ldapConfig!
            );
            if (!isValid)
            {
                // tracking échec
                await _trackingService.TrackAsync(
                    new LoginHistory
                    {
                        Username = request.Username,
                        LoginDate = DateTime.UtcNow,
                        IsSuccess = false,
                        IpAddress = ip,
                        UserAgent = userAgent,
                        FailureReason = "Paramètres de connexion à l'annuaire incorrects.",
                    }
                );
                return new LoginResponse
                {
                    Success = false,
                    Message = "Les paramètres de connexions sont incorrects.",
                };
            }
            // Historisation de la connexion
            await _trackingService.TrackAsync(
                new LoginHistory
                {
                    UserId = user.Id,
                    Username = user.UserName!,
                    LoginDate = DateTime.UtcNow,
                    IsSuccess = true,
                    IpAddress = ip,
                    UserAgent = userAgent,
                }
            );

            return new LoginResponse
            {
                Success = true,
                Message = "Authentification succeded!!",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
            };
        }

        private async Task<LoginResponse> AuthenticateIdentityAsync(
            ApplicationUser user,
            LoginRequest request,
            string ip,
            string userAgent,
            JwtSecurityToken token,
            DateTime expiration
        )
        {
            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _trackingService.TrackAsync(
                    new LoginHistory
                    {
                        Username = request.Username,
                        LoginDate = DateTime.UtcNow,
                        IsSuccess = false,
                        IpAddress = ip,
                        UserAgent = userAgent,
                        FailureReason = "Paramètres incorrects",
                    }
                );

                return new LoginResponse
                {
                    Success = false,
                    Message = "Les paramètres de connexions sont incorrects.",
                };
            }

            // Historisation de la connexion
            await _trackingService.TrackAsync(
                new LoginHistory
                {
                    UserId = user.Id,
                    Username = user.UserName!,
                    LoginDate = DateTime.UtcNow,
                    IsSuccess = true,
                    IpAddress = ip,
                    UserAgent = userAgent,
                }
            );

            return new LoginResponse
            {
                Success = true,
                Message = "Authentification succeded!!",
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration,
            };
        }
    }
}
