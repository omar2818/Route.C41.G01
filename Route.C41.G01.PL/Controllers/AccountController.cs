﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Route.C41.G01.DAL.Models;
using Route.C41.G01.PL.Hepers;
using Route.C41.G01.PL.Services.EmailSender;
using Route.C41.G01.PL.ViewModels.User;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Route.C41.G01.PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMailSettings _mailSettings;
		private readonly ISMSService _smsService;

		public AccountController(IEmailSender emailSender
            , IConfiguration configuration
            , UserManager<ApplicationUser> userManager
            , SignInManager<ApplicationUser> signInManager
            , IMailSettings mailSettings
            , ISMSService sms)
        {
            _emailSender = emailSender;
            _configuration = configuration;
			_userManager = userManager;
			_signInManager = signInManager;
            _mailSettings = mailSettings;
			_smsService = sms;
		}

        #region Sign Up

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);

                if (user is null)
                {
                    user = new ApplicationUser()
                    {
                        FName = model.FirstName,
                        LName = model.LastName,
                        UserName = model.UserName,
                        Email = model.Email,
                        IsAgree = model.IsAgree
                    };

                    var Result = await _userManager.CreateAsync(user, model.Password);

                    if(Result.Succeeded)
                    {
                        return RedirectToAction(nameof(SignIn));
                    }

                    foreach(var error in Result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);

					}

                }
                ModelState.AddModelError(string.Empty, "This userName is Already in use for another Account");
                
            }
            return View(model);
        }

        #endregion

        #region Sign In

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(SignInViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user is not null)
                {
                    var password = await _userManager.CheckPasswordAsync(user, model.Password);
                    if (password)
                    {
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                        if(result.IsLockedOut)
                        {
                            ModelState.AddModelError(string.Empty, "Your Account is Locked!!");
                        }

                        if (result.Succeeded)
                        {
                            return RedirectToAction(nameof(HomeController.Index), "Home");
                        }

						if (result.IsNotAllowed)
						{
							ModelState.AddModelError(string.Empty, "Your Account is Not Confirmed Yet!!");
						}

					}
                }
				ModelState.AddModelError(string.Empty, "Invalid Login");
			}
			return View(model);
        }

        public IActionResult GoogleLogin()
        {
            var prop = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };

            return Challenge(prop, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!result.Succeeded) return RedirectToAction("Login");

            // Retrieve the user's information from Google claims
            var claims = result.Principal.Identities.FirstOrDefault().Claims
                .Select(c => new { c.Type, c.Value });

            // Find or create the user in the local database
            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email };
                await _userManager.CreateAsync(user);
            }

            // Sign in the user
            await _signInManager.SignInAsync(user, isPersistent: false);

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        #endregion

        public async new Task<IActionResult> SignOut()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(SignIn));
        }

        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendResetPasswordEmail(ForgetPasswordViewModel model)
        {
            if(ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is not null)
                {
                    var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var PasswordURL = Url.Action("ResetPassword", "Account", new { email = user.Email, token = resetPasswordToken }, Request.Scheme);
                    await _emailSender.SendAsync(
                        from: _configuration["EmailSettings:SenderEmail"],
                        recipients: model.Email,
                        subject: "reset your password",
                        body: PasswordURL
                        );

                    return Redirect(nameof(CheckYourInbox));
                }
                ModelState.AddModelError(string.Empty, "There is No Account With this Email!!");
            }
            return View(nameof(ForgetPassword), model);
        }

        [HttpPost]
        public async Task<IActionResult> SendResetPasswordEmailUsingMailKit(ForgetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user is not null)
                {
                    var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var PasswordURL = Url.Action("ResetPassword", "Account", new { email = user.Email, token = resetPasswordToken }, Request.Scheme);
                    
                    var email = new Email()
                    {
                        To = model.Email,
                        Subject = "Reset Your Password",
                        Body = PasswordURL
                    };

                    _mailSettings.SendEmail(email);

                    return Redirect(nameof(CheckYourInbox));
                }
                ModelState.AddModelError(string.Empty, "There is No Account With this Email!!");
            }
            return View(nameof(ForgetPassword), model);
        }

        [HttpPost]
		public async Task<IActionResult> SendSMS(ForgetPasswordViewModel model)
		{
			if (ModelState.IsValid)
			{
				var user = await _userManager.FindByEmailAsync(model.Email);

				if (user is not null)
				{
					var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
					var PasswordURL = Url.Action("ResetPassword", "Account", new { email = user.Email, token = resetPasswordToken }, Request.Scheme);

                    var sms = new SMSMessage()
                    {
                        PhoneNumber = user.PhoneNumber,
                        Body = PasswordURL
                    };

                    _smsService.SendSMS(sms);

					return Ok("Check Your Phone");
				}
				ModelState.AddModelError(string.Empty, "There is No Account With this Email!!");
			}
			return View(nameof(ForgetPassword), model);
		}

		public IActionResult CheckYourInbox()
        {
            return View();
        }
        public IActionResult ResetPassword(string email, string token)
        {
            TempData["Email"] = email;
            TempData["token"] = token;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var email = TempData["Email"] as string;
                var token = TempData["token"] as string;
                var user = await _userManager.FindByNameAsync(email);
                if (user is not null)
                {
                    _userManager.ResetPasswordAsync(user, token, model.NewPassword);
                    return RedirectToAction(nameof(SignIn));
                }
                ModelState.AddModelError(string.Empty, "Url is not valid");
            }
            return View(model);
        }
    }
}
