using ECommerce_API_2.DTOs.Requests;
using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using static System.Net.WebRequestMethods;

namespace ECommerce_API_2.Areas.Identity
{
    [Route("[area]/[controller]")]
    [Area (SD.IDENTITY_AREA)]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IAccountServices _accountservices;
        private readonly IRepository<ApplicationUser> _applicationuserRepository;
        private readonly IConfiguration _configuration;

        public AccountsController (UserManager<ApplicationUser> userManager , SignInManager<ApplicationUser> signInManager ,
             IEmailSender emailSender , IRepository<ApplicationUser> applicationuserRepository 
            , IAccountServices accountservices, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountservices = accountservices;
            _applicationuserRepository = applicationuserRepository;
            _configuration = configuration;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register ( RegisterRequest registerrequest)
        {
            //set user's data maniualy
            ApplicationUser user = new ApplicationUser()
            {
                FName = registerrequest.FName,
                LName = registerrequest.LName,
                Email = registerrequest.Email,
                UserName = registerrequest.UserName,
                Address = registerrequest.Address
            };

            //set user's data using mapster
            //var user = registerrequest.Adapt<ApplicationUser>();

            var result = await _userManager.CreateAsync(user, registerrequest.Password);
            
            if (!result.Succeeded)     
            {
                ModelStateDictionary keyValuePairs = new ModelStateDictionary();

                foreach (var item in result.Errors)
                    keyValuePairs.AddModelError(String.Empty ,item.Description );

                return BadRequest(keyValuePairs);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = Url
                .Action("ConfirmEmail",
                "Account",
                new { area = "Identity", token, Id = user.Id },
                Request.Scheme);


            await _accountservices.SendEmailAsync(EmailType.Confirmation,
                $"<h1>Click <a href='{confirmationLink}'>here</a> to cofirm youyr account</h1>", user);

            await _userManager.AddToRoleAsync(user, SD.CUSTOMER_ROLE);

            Console.WriteLine($"Confirmation link is: { confirmationLink}");
            return Ok(new SuccessRespones()
            {
                Msg = "Account Has been Added Successfully"
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login ( LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.EmailOrUserName) ??
                await _userManager.FindByNameAsync(loginRequest.EmailOrUserName);

            ModelStateDictionary keyValuePairs = new();

            if (user is  null)
            {

                keyValuePairs.AddModelError("EmailOrUserName" , "Invalid User Name or Email");
                keyValuePairs.AddModelError("PassWord" , "Invalid PassWord");

                return BadRequest(keyValuePairs);
            }

            var result = await _signInManager.PasswordSignInAsync(user ,loginRequest.Password , loginRequest.RemeberMe , true);

            List<Claim> userClaims =  new();

            userClaims.Add(new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()));
            userClaims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            userClaims.Add(new Claim(ClaimTypes.Name, user.UserName!));
            userClaims.Add(new Claim(ClaimTypes.Email, user.Email!));

            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles != null)
            {
                foreach (var roleName in userRoles)
                {
                    userClaims.Add(new Claim(ClaimTypes.Role, roleName));
                }
            }

            var signInKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0f24664b0f76166225004ff2cf5db2c68f5273a995cc7029c6d030f79b3779da"));
            SigningCredentials signingCredentials = new(signInKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken userToken = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(Convert.ToInt32(_configuration["JWT:DurationInMinutes"])),
                claims: userClaims,
                signingCredentials: signingCredentials
                );


          


            if (!result.Succeeded)
            {
                if (result.IsNotAllowed)
                {
                    keyValuePairs.AddModelError("EmailOrUserName", "Please Confirm Your Email Firast");
                    return BadRequest(keyValuePairs);
                }
                if (result.IsLockedOut)
                {
                    keyValuePairs.AddModelError(String.Empty, "Too Many, Attemps Please Try Again Leter");
                    return BadRequest(keyValuePairs);
                }
                keyValuePairs.AddModelError("EmailOrUserName", "Invalid User Name or Email");
                keyValuePairs.AddModelError("PassWord", "Invalid PassWord");

                return BadRequest(keyValuePairs);
            }

            return Created("", new SuccessRespones()
            {
                Msg = "Logged in Successfully",
                OptinalData = new List<string>{ new JwtSecurityTokenHandler().WriteToken(userToken)},
                ExpireIn = userToken.ValidTo
            });

            
        }

    }
}
