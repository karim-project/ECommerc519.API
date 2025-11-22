using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerc519.API.Areas.Identity.Controllers
{
    [Route("[Area]/[controller]")]
    [ApiController]
    [Area("Identity")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPrepositry;

        public AccountController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, SignInManager<ApplicationUser> signInManager, IRepository<ApplicationUserOTP> applicationUserOTPrepositry)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _signInManager = signInManager;
            _applicationUserOTPrepositry = applicationUserOTPrepositry;
        }



        [HttpPost("Registor")]
        public async Task<IActionResult> Registor(RegistorRequest registorRequest)
        {

            var user = new ApplicationUser()
            {
                FirstName = registorRequest.FirstName,
                LastName = registorRequest.LastName,
                Email = registorRequest.Email,
                UserName = registorRequest.UserName,
            };

            var result = await _userManager.CreateAsync(user, registorRequest.Password);

            if (!result.Succeeded)
            {
                //foreach (var item in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, item.Code);
                //}

                return BadRequest(result.Errors);
            }

            // send comfirm email 
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);


            await _emailSender.SendEmailAsync(registorRequest.Email, "ECommerc519 - Comfirm your email", $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            await _userManager.AddToRoleAsync(user, SD.Customer_Role);


            return Ok(new
            {
                msg = "Create Account Successfully"
            });
        }
        [HttpPost("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {

            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return NotFound(new
                {
                    msg = "Invalid User"
                }); //  notfound بيدور علي اليوزر وملقهوش ف عشان كده استخدمنا 
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    msg = "Invalid or expired token"
                }); // استخدمنا badreqest  

            }
            else
            {
                return Ok(new
                {
                    msg = "Email Confirmed Successfully"
                }); // عشان عمليه ناجحه
            }

        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {


            var user = await _userManager.FindByNameAsync(loginRequest.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(loginRequest.UserNameOrEmail);


            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "Invalid Cred",
                    Description = "Invalid User Name / Email OR Password"
                });

            }
            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "Too many attemps",
                        Description = "Too many attemps, try again after 5 min"
                    });
                else if (!user.EmailConfirmed)
                    return BadRequest(new ErrorModelResponse
                    {
                        Code = "Confirm Your Email",
                        Description = "Please Confirm Your Email First!!"
                    });
                else
                    return NotFound(new ErrorModelResponse
                    {
                        Code = "Invalid Cred.",
                        Description = "Invalid User Name / Email OR Password"
                    });
            }
            //generate Token
            var userRole = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Typ, String.Join(", ", userRole)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("2BBFD56151D59D1A5713B18BCDE5F2BBFD56151D59D1A5713B18BCDE5F"));
            var signinCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:7042",
                audience: "https://localhost:7042",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: signinCredentials
                );
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });

        }
        [HttpPost("ResendComfirmEmail")]
        public async Task<IActionResult> ResendComfirmEmail(ResendComfirmEmailRequset resendComfirmEmailRequset)
        {
            var user = await _userManager.FindByNameAsync(resendComfirmEmailRequset.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(resendComfirmEmailRequset.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "Invalid Cred",
                    Description = "Invalid User Name / Email OR Password"
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Already Confirmed!!",
                    Description = "Already Confirmed!!"
                });
            }

            // send comfirm email 
            var token = _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = "Identity", token, userId = user.Id }, Request.Scheme);


            await _emailSender.SendEmailAsync(user.Email!, "ECommerc519 - Resend Comfirm your email", $"<h1>Confirm Your Email By Clicking <a href='{link}'>Here</a></h1>");

            return Ok(new
            {
                msg = "Send msg successfully"
            });

        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await _userManager.FindByNameAsync(forgetPasswordRequest.UserNameOrEmail) ?? await _userManager.FindByEmailAsync(forgetPasswordRequest.UserNameOrEmail);

            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "Invalid Cred",
                    Description = "Invalid User Name / Email OR Password"
                });
            }

            if (user.EmailConfirmed)
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Already Confirmed!!",
                    Description = "Already Confirmed!!"
                });
            }

            var UserOTPs = await _applicationUserOTPrepositry.GetAsync(e => e.ApplicationUserId == user.Id);

            var totalOtps = UserOTPs.Count(e => (DateTime.UtcNow - e!.CreateAt).TotalHours < 24);

            if (totalOtps > 3)
            {
                return BadRequest(new ErrorModelResponse
                {
                    Code = "Too Many Attemp",
                    Description = "Too many attemps, try again later"
                });
            }
            var otp = new Random().Next(1000, 9999).ToString();

            await _applicationUserOTPrepositry.AddAsync(new()
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationUserId = user.Id,
                CreateAt = DateTime.UtcNow,
                IsValid = true,
                OTP = otp,
                ValidTO = DateTime.UtcNow.AddDays(1),
            });
            await _applicationUserOTPrepositry.CommitAsync();

            await _emailSender.SendEmailAsync(user.Email!, "ECommerc519 - Reset your password", $"<h1>Use This OTP: {otp} To Reset Your Account. Don't share it.</h1>");

            return CreatedAtAction("ValidateOTP", new { userId = user.Id });
        }
        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequset validateOTPRequset)
        {
            var result = await _applicationUserOTPrepositry.GetOneAsync(e => e.ApplicationUserId == validateOTPRequset.ApplicationUserId && e.OTP == validateOTPRequset.OTP && e.IsValid);

            if (result is null)
            {
                return CreatedAtAction("ValidateOTP", new { userId = validateOTPRequset.ApplicationUserId });

            }

            return CreatedAtAction("ValidateOTP", new { userId = validateOTPRequset.ApplicationUserId });
        }
        [HttpPost("NewPassword")]
        public async Task<IActionResult> NewPassword(NewPasswordRequset newPasswordRequset)
        {
            var user = await _userManager.FindByIdAsync(newPasswordRequset.ApplicationUserId);

            if (user is null)
            {
                return NotFound(new ErrorModelResponse
                {
                    Code = "Invalid Cred",
                    Description = "Invalid User Name / Email OR Password"
                });
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordRequset.Password);

            if (!result.Succeeded)
            {
                //foreach (var item in result.Errors)
                //{
                //    ModelState.AddModelError(string.Empty, item.Code);
                //}

                return BadRequest(result.Errors);
            }
            return Ok();
        }
    }
}
