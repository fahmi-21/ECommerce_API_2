using ECommerce_API_2.DTOs.Requests;
using ECommerce_API_2.DTOs.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
        
        public AccountsController (UserManager<ApplicationUser> userManager , SignInManager<ApplicationUser> signInManager ,
             IEmailSender emailSender , IRepository<ApplicationUser> applicationuserRepository 
            , IAccountServices accountservices)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountservices = accountservices;
            _applicationuserRepository = applicationuserRepository;
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

            //aset user's data using master
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

            var confirmationLink = Url.Action("Confirm", "Account", new
            {
                area = "Identity",
                token,
                user.Id
            }, Request.Scheme);

            await _accountservices.SendEmailAsync(EmailType.Confirmation, $"<h1>Click <a href='{confirmationLink}'>here</a>" +
                $"To Confirm Your Account </h1>", user);

            await _userManager.AddToRoleAsync(user, SD.CUSTOMER_ROLE);

            return Ok(new SuccessRespones()
            {
                Msg = "Account Has been Added Successfully"
            });
            //return Created();
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

            return Created($"{Request.Scheme}/{Request.Host}/Customer/Home/Index" ,new SuccessRespones ()
            {
                Msg = $"Welcome Back {user.UserName}"
            });
        }

    }
}
