
using Microsoft.AspNetCore.Http;

using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace BookStoreAPI.Areas.Identity
{
    [Route("auth/[area]/[controller]")]
    [ApiController]
    [Area ("Identity")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        // to check eligiblity of signin
        public AccountController(UserManager<ApplicationUser> userManager, IRepository<ApplicationUserOTP> applicationUserOTPRepository, SignInManager<ApplicationUser> signInManager
            , IEmailSender emailSender)
        {
            _userManager = userManager;
            _applicationUserOTPRepository = applicationUserOTPRepository;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }



        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {


            ApplicationUser user = new()
            {
                Email = registerRequest.Email,
                UserName = registerRequest.UserName,
                Name = registerRequest.Name,
                Address = registerRequest.Address,
            };

            //instead
            //ApplicationUser user = registerRequest.Adapt<ApplicationUser>();
            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded) foreach (var item in result.Errors)
                {
                    return BadRequest(result.Errors);
                }
            // for sending confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var Link = Url.Action("Confirm", "Account",
                new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);


            await _emailSender.SendEmailAsync(registerRequest.Email, "BookStoreAPI - Confirm Your Email",
                   $"<h1> Please Confirm your Email by clicking  <a href=' {Link} '>Here</a></h1>"
                   );

            //save the registration as a customer
            await _userManager.AddToRoleAsync(user, SD.Customer_Role);

            // to return direction to the Page 
            return Created( $"{Request.Scheme}//{Request.Host}/Identity/Account/Login", new
            {
                success_notification = " Account Added Successfully , please Confirm your Email"
            });


        }
        

        [HttpGet ("Confirm")]
        public async Task<IActionResult> Confirm(string token, string userId)
        {
            // to find the user in DB
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound();

            //validate the  Email
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);

            }
            else
            {
                return Created($"{Request.Scheme}//{Request.Host}/Identity/Account/Login", new
                {
                    success_notification = " Account Confirmed Successfully"
                });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {


            var user = await _userManager.FindByEmailAsync(loginRequest.EmailOrUserName) ?? await _userManager.FindByNameAsync(loginRequest.EmailOrUserName);
            if (user is null)

            {
                return BadRequest(new ErrorModel
                {
                    Code = "Invalid UserName / Email Or Password",
                    Message = "Invalid UserName / Email Or Password"
                });

            }

            // to check the password validity
            //    var password = await _userManager.CheckPasswordAsync(user , loginRequest.Password);
            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, lockoutOnFailure: true, isPersistent: loginRequest.RememberMe);

            if (!result.Succeeded)
            {
                if (!user.LockoutEnabled)
                {
                    ModelState.AddModelError(string.Empty, $"Your Account is locked till {user.LockoutEnd}");

                }
                if (result.IsNotAllowed)
                {
                    return NotFound(new ErrorModel
                    {
                        Code = "Invalid UserName / Email Or Password",
                        Message = "Invalid UserName / Email Or Password"
                    });
                }
                else if (result.IsLockedOut)
                {
                    return NotFound(new ErrorModel
                    {
                        Code = "Invalid UserName / Email Or Password",
                        Message = "Invalid UserName / Email Or Password"
                    });
                }
                else
                {
                    return NotFound(new ErrorModel
                    {
                        Code = "Invalid UserName / Email Or Password",
                        Message = "Invalid UserName / Email Or Password"
                    });
                }

            }


            return Ok(new {
                success_notification = "Welcome Back"
        } );

        }


        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {

            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.EmailOrUserName) ?? await _userManager.FindByNameAsync(resendEmailConfirmationRequest.EmailOrUserName);
            if (user is null)

            {
                return NotFound(new
                {
                    error_notifiaction = "Invalid UserName / Email"
                });
            }

            if (user.EmailConfirmed)
            {
                ModelState.AddModelError(string.Empty, "Your Account Already Confirmed");

                return NotFound(new ErrorModel
                {
                    Code = "Your Account Already Confirmed",
                    Message = "Your Account Already Confirmed"
                });

            }

            // for sending confirmation email
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var Link = Url.Action(nameof(Confirm), "Account",
                new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);


            await _emailSender.SendEmailAsync(user.Email!, "BookStoreAPI - Resend Your Email",
                  $"<h1> Please Confirm your Email by clicking  <a href=' {Link} '>Here</a></h1>"
                  );


            //return Created($"{Request.Scheme}//{Request.Host}/Identity/Account/Login", new 
            //{
            //    userId = user.Id,
            //    success_notification = " Email sent Successfully , please Confirm your Email"
            //});

            return CreatedAtAction(nameof(Login), new {
                userId = user.Id
            },new SuccessModel { Message = " Email sent Successfully , please Confirm your Email" });
        }

      

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {


            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.EmailOrUserName) ??
             await _userManager.FindByNameAsync(forgetPasswordRequest.EmailOrUserName);
            if (user is null)

            {
                ModelState.AddModelError(string.Empty, "Invalid UserName / Emai");

                return NotFound(new
                {
                    error_notifiaction = "Invalid UserName / Email"
                });
            }

            // for sending confirmation email

            var otp = new Random().Next(1000, 9999);

            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id &&
            e.CreatedAt < DateTime.UtcNow.AddHours(-24));

            if (userOTPs.ToList().Count > 5)
            {
                 return BadRequest(new
                {
                    error_notifiaction = "Too Many Attempts , please try again Later"
                });
            }

            // create OTP in DB
            await _applicationUserOTPRepository.CreateAsync(new()
            {
                OTP = otp.ToString(),
                ApplicationUserId = user.Id,
            });
            await _applicationUserOTPRepository.CommitAsync();
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var Link = Url.Action(nameof(Confirm), "Account",
                new { area = "Identity", token = token, userId = user.Id }, Request.Scheme);


            await _emailSender.SendEmailAsync(user.Email!, "BookStoreAPI - Reset Your Password",
                  $"<h1> Please reset your password using this OTP {otp} " +
                  $"please do not share your OTP with any One'></h1>"
                  );

            //          return Created($"{Request.Scheme}//{Request.Host}/Identity/Account/ValidateOTP", new
            //{
            //    success_notification = " Email sent Successfully , please Check your Email"
            //});

            return CreatedAtAction(nameof(ValidateOTP),new SuccessModel { 
                Message = " Email sent Successfully , please Check your Email" });

        }




        [HttpPost("ValidateOTP")]
        public async Task<IActionResult> ValidateOTP(ValidateOTPRequest validateOTPRequest)
        {


            var user = await _userManager.FindByIdAsync(validateOTPRequest.UserId);
            if (user is null) return NotFound();
            var valideOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id && e.isValid &&
            e.Validto > DateTime.UtcNow);

            var resul = valideOTPs.Any(e => e.OTP == validateOTPRequest.OTP);

            if (!resul)
            {
              
                return BadRequest(new ErrorModel
                {
                    Code = "Invalid OR Expired OTP",
                    Message = "Invalid OR Expired OTP"
                 });

            }

          
            return CreatedAtAction(nameof(ResetPassword), new { 
                userId = user.Id
            },new SuccessModel { 
            Message = " Valid OTP"
            } );


        }


        [HttpPost ("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {

            var user = await _userManager.FindByIdAsync(resetPasswordRequest.UserId);
            if (user is null) return NotFound();

            var dummyToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, dummyToken, resetPasswordRequest.Password);

            if (!result.Succeeded)
                            return BadRequest(result.Errors);
            
           
            return CreatedAtAction(nameof(Login), new
            {
                userId = user.Id
            },new SuccessModel{ Message= "Password has been changed Suceessfully"});


        }
    }


}


