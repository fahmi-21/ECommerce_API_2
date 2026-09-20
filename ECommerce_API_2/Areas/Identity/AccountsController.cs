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
        private readonly IRepository<ApplicationUserOTP> _applicationuserOtpRepository;
        private readonly IConfiguration _configuration;

        public AccountsController (UserManager<ApplicationUser> userManager , SignInManager<ApplicationUser> signInManager ,
             IEmailSender emailSender , IRepository<ApplicationUserOTP> applicationuserotpRepository 
            , IAccountServices accountservices, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _accountservices = accountservices;
            _applicationuserOtpRepository = applicationuserotpRepository;
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
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail( string Id , string Email)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user is null)
                return NotFound(new ErrorResponse()
                {
                    ErorMsg = "User Not Found"
                });

            var result = await _userManager.ConfirmEmailAsync(user, Email);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Invalid confirmation link. Please try again.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            return Ok(new SuccessRespones()
            {
                Msg = "Email Confirmed Successfully"
            });
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword( ForgotPasswordRequest forgotPasswordDto)
        {
            var user =
                await _userManager.FindByEmailAsync(forgotPasswordDto.EmailOrUserName)
                ?? await _userManager.FindByNameAsync(forgotPasswordDto.EmailOrUserName);

            if (user is null)
            {
                return NotFound(new ErrorResponse
                {
                    ErorMsg = "User Not Found"
                });
            }

            if (!user.EmailConfirmed)
            {
                return BadRequest(new ErrorResponse
                {
                    ErorMsg = "Please Confirm Your Email First"
                });
            }

            var userOtpCount =
                (await _applicationuserOtpRepository.GetAsync(
                    e => e.ApplicationUserId == user.Id &&
                         e.CreateAt >= DateTime.UtcNow.AddHours(-24)
                )).Count();

            if (userOtpCount >= 3)
            {
                return BadRequest(new ErrorResponse
                {
                    ErorMsg =
                        "You Have Reached The Maximum Number Of OTP Requests. Please Try Again Later."
                });
            }

            var otp = Random.Shared.Next(100000, 999999).ToString();

            string msg =
                $"<h1>Your OTP Is: {otp}. Don't Share It.</h1>";

            await _accountservices.SendEmailAsync(
                EmailType.CorgetPassword,
                msg,
                user);

            var userOtp = new ApplicationUserOTP
            {
                ApplicationUserId = user.Id,
                OTP = otp,
                CreateAt = DateTime.UtcNow
            };

            await _applicationuserOtpRepository.CreateAsync(userOtp);
            await _applicationuserOtpRepository.CommitAsync();

            return Ok(new SuccessRespones
            {
                Msg = "OTP Has Been Sent Successfully"
            });
        }
        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPDTO)
        {
            var user = await _userManager.FindByEmailAsync(validateOTPDTO.Email);

            if (user is null) return NotFound();

            var otp = (await _applicationuserOtpRepository.GetAsync()).Where(e => e.ApplicationUserId == user.Id && e.IsValid).OrderBy(e => e.Id).LastOrDefault();

            if (otp is null)
                return BadRequest(new ErrorResponse { ErorMsg = "Invalid OTP"});

            var resetToken =await _userManager.GeneratePasswordResetTokenAsync(user);

            return Ok(new
            {
                Msg = "OTP Verified Successfully",
                ResetToken = resetToken
            });
        }
        [HttpPost("resend-email-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationDTO)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationDTO.EmailOrUserName) ??
                await _userManager.FindByNameAsync(resendEmailConfirmationDTO.EmailOrUserName);

            if (user is null)
                return NotFound(new ErrorResponse{ErorMsg = "User Not Found"});

            if (user.EmailConfirmed)
                return BadRequest(new ErrorResponse {ErorMsg = "Email is already confirmed."});

            if (user is not null && !user.EmailConfirmed)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var confirmationLink = Url.Action("ConfirmEmail", "Account",
                    new { area = "Identity", token, Id = user.Id },
                    Request.Scheme);

                await _accountservices.SendEmailAsync(EmailType.Redsendconfirmation, $"<h1>Click <a href='{confirmationLink}'>here</a> to cofirm youyr account</h1>", user);
            }


            return Ok(new SuccessRespones
            {
                Msg = "Email Has Been Resended Successfully ,Please Confirm It "
            });
        }
        [HttpGet ("ResetPassword")]
        public async Task<IActionResult> ResetPassword( ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.Email);

            if (user is null)
                return NotFound(new ErrorResponse { ErorMsg = "User Not Found" });



            var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.ResetToken, resetPasswordRequest.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new ErrorResponse
                {
                    ErorMsg = string.Join(
                        ", ",
                        result.Errors.Select(x => x.Description))
                });
            }

            return Ok(new SuccessRespones
            {
                Msg = "Password Reset Successfully"
            });
        }
    }
}
