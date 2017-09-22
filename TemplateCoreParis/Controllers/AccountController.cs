using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TemplateCoreParis.Models;
using TemplateCoreParis.Models.AccountViewModels;
using TemplateCoreParis.Services;
using TemplateCoreParis.FacebookChat;
using TemplateCoreParis.Data;
using System.Net;
using OtpSharp;
using TemplateCoreParis.WebChat;

namespace TemplateCoreParis.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ISmsSender _smsSender;
        private readonly ILogger _logger;
        private readonly string _externalCookieScheme;

        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOptions<IdentityCookieOptions> identityCookieOptions,
            IEmailSender emailSender,
            ISmsSender smsSender,
            ILoggerFactory loggerFactory,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _logger = loggerFactory.CreateLogger<AccountController>();
            _context = context;
        }

        //
        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl = null)
        {
            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.Authentication.SignOutAsync(_externalCookieScheme);

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Require the user to have a confirmed email before they can log on.
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user))
                    {

                        var code = user.SecurityStamp;

                        var callbackUrl = Url.Action(nameof(ConfirmEmail), "account", new { userid = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                        await _emailSender.SendEmailAsync(model.Email, "VCA - Confirme su cuenta",
                            $"Por favor confirme su cuenta haciendo click <a href='{callbackUrl}'>AQUI</a>");

                        ModelState.AddModelError(string.Empty,
                                      "Tiene que confirmar su cuenta de correo electrónico");
                        return View(model);
                    }
                }


                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation(1, "User logged in.");

                    return RedirectToLocal(returnUrl);
                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning(2, "User account locked out.");
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Intento inválido de ingreso");

                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var _encrypt = Helpers.Helpers.EncryptString(model.SecretResponse, ChatBotController._keyEncode);

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Birthday = model.Birthday,
                    DocIdentity = model.DocIdentity,
                    Title = model.Title,
                    PhoneNumber = Helpers.Helpers.PhoneFormatter(model.PhoneNumber),
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var code = user.SecurityStamp;

                    var callbackUrl = Url.Action(nameof(ConfirmEmail), "account", new { userid = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "AJE Group - Confirme su cuenta",
                        $"Por favor confirme su cuenta haciendo click <a href='{callbackUrl}'>AQUI</a>");

                    _logger.LogInformation(3, "Usuario ha creado una nueva cuenta con clave");

                    return RedirectToLocal(returnUrl);
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            _logger.LogInformation(4, "User logged out.");

            return RedirectToAction(nameof(HomeController.Index), "Home");
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }

        //
        // GET: /Account/ExternalLoginCallback
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error de Proveedor Externo: {remoteError}");

                return View(nameof(Login));
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction(nameof(Login));
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);

            if (result.Succeeded)
            {
                _logger.LogInformation(5, "User logged in with {Name} provider.", info.LoginProvider);

                return RedirectToLocal(returnUrl);
            }

            if (result.RequiresTwoFactor)
            {
                return RedirectToAction(nameof(SendCode), new { ReturnUrl = returnUrl });
            }

            if (result.IsLockedOut)
            {
                return View("Lockout");
            }
            else
            {
                // If the user does not have an account, then ask the user to create an account.
                ViewData["ReturnUrl"] = returnUrl;
                ViewData["LoginProvider"] = info.LoginProvider;
                var email = info.Principal.FindFirstValue(ClaimTypes.Email);

                return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await _signInManager.GetExternalLoginInfoAsync();

                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Birthday = model.Birthday,
                    PhoneNumber = Helpers.Helpers.PhoneFormatter(model.PhoneNumber),
                    SecretQuestion = model.SecretQuestion,
                    SecretResponse = model.SecretResponse
                };

                var result = await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);

                        return RedirectToLocal(returnUrl);
                    }
                }

                AddErrors(result);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(model);
        }

        // GET: /Account/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return View("Error");
            }

            bool validate = false;

            if (user.SecurityStamp == code)
            {
                validate = true;
                user.EmailConfirmed = true;

                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return View(validate ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                var code = user.SecurityStamp;

                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    var callbackUrl1 = Url.Action(nameof(ConfirmEmail), "account", new { userid = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "VCA - Confirme su cuenta",
                        $"Por favor confirme su cuenta haciendo click <a href='{callbackUrl1}'>AQUI</a>");

                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { userId = user.Id, code = code }, protocol: HttpContext.Request.Scheme);

                await _emailSender.SendEmailAsync(model.Email, "VCA - Reinicio de Clave",
                   $"Por favor, reinicie su clave haciendo click <a href='{callbackUrl}'>AQUÍ</a>");

                return View("ForgotPasswordConfirmation");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string code = null)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
            }

            if (user.SecurityStamp == model.Code)
            {
                await _userManager.RemovePasswordAsync(user);

                var result = await _userManager.AddPasswordAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(AccountController.ResetPasswordConfirmation), "Account");
                }

                AddErrors(result);
            }

            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/SendCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl = null, bool rememberMe = false)
        {
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);

            List<SelectListItem> factorOptions = new List<SelectListItem>();

            foreach (var item in userFactors)
            {
                switch (item)
                {
                    case "Email":
                        factorOptions.Add(new SelectListItem { Text = "Correo Electrónico", Value = "Email" });
                        break;

                    case "Phone":
                        factorOptions.Add(new SelectListItem { Text = "Teléfono", Value = "Phone" });
                        break;
                    default:
                        factorOptions.Add(new SelectListItem { Text = item, Value = item });
                        break;
                }
            }

            factorOptions.Add(new SelectListItem { Text = "Pregunta secreta", Value = "Secret" });
            factorOptions.Add(new SelectListItem { Text = "Token Google", Value = "Token" });

            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            string myProvider = model.SelectedProvider;

            // Generate the token and send it
            if (model.SelectedProvider == "Token" || model.SelectedProvider == "Secret")
            {
                myProvider = "Email";

            }

            string code = await _userManager.GenerateTwoFactorTokenAsync(user, myProvider);

            if (string.IsNullOrWhiteSpace(code))
            {
                return View("Error");
            }

            var message = "Tú código de Seguridad es: " + code;

            switch (model.SelectedProvider)
            {
                case "Email":
                    await _emailSender.SendEmailAsync(await _userManager.GetEmailAsync(user), "AJE Group - Código de Seguridad", message);
                    break;

                case "Phone":
                    await _smsSender.SendSmsAsync(await _userManager.GetPhoneNumberAsync(user), message);
                    break;

                case "Secret":
                    return RedirectToAction(nameof(SecretAuth), new { Code = code, Provider = myProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });

                case "Token":
                    return RedirectToAction(nameof(GoogleTokenAsync), new { Code = code, Provider = myProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });

                default:
                    break;
            }

            return RedirectToAction(nameof(VerifyCode), new { Provider = myProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/VerifyCode
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCode(string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // The following code protects for brute force attacks against the two factor codes.
            // If a user enters incorrect codes for a specified amount of time then the user account
            // will be locked out for a specified amount of time.
            var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);

            if (result.Succeeded)
            {
                return RedirectToLocal(model.ReturnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning(7, "User account locked out.");

                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Código inválido");

                return View(model);
            }
        }

        // GET: /Account/GoogleTokenAsync
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleTokenAsync(string code, string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            string message = "Verificación de 2 pasos con Google Authorization";

            byte[] secretKey = KeyGeneration.GenerateRandomKey(20);
            string userId = user.Id;
            string barcodeUrl = KeyUrl.GetTotpUrl(secretKey, userId) + "&issuer=TemplateCoreParis";


            var model = new GoogleAuthenticatorViewModel
            {
                SecretKey = Convert.ToBase64String(secretKey),
                BarcodeUrl = WebUtility.UrlEncode(barcodeUrl),
                Provider = provider,
                RememberMe = rememberMe,
                ReturnUrl = returnUrl,
                Code = code
            };


            ViewData["Message"] = message;

            return View(model);
        }


        // POST: /Account/GoogleTokenAsync
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GoogleTokenAsync(GoogleAuthenticatorViewModel model)
        {
            if (ModelState.IsValid)
            {
                byte[] secretKey = Convert.FromBase64String(model.SecretKey);

                long timeStepMatched = 0;

                var otp = new Totp(secretKey);

                if (otp.VerifyTotp(model.Token, out timeStepMatched, new VerificationWindow(2, 2)))
                {
                    var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);

                    if (result.Succeeded)
                    {
                        return RedirectToLocal(model.ReturnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning(7, "User account locked out.");

                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Código inválido");

                        return View(model);
                    }

                }
                else
                    ModelState.AddModelError("Code", "El código no es válido");
            }





            return View(model);
        }


        // GET: /Account/SecretAuth
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SecretAuth(string code, string provider, bool rememberMe, string returnUrl = null)
        {
            // Require that the user has already logged in via username/password or external login
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            string message = "Verificación de 2 pasos con Pregunta Secreta";

            var model = new SecretAuthViewModel
            {
                SecretQuestion = user.SecretQuestion,
                Provider = provider,
                RememberMe = rememberMe,
                ReturnUrl = returnUrl,
                Code = code,
                Response = user.SecretResponse
            };

            ViewData["Message"] = message;

            return View(model);
        }


        // POST: /Account/SecretAuth
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SecretAuth(SecretAuthViewModel model)
        {
            if (ModelState.IsValid)
            {

                if (model.Token == model.Response)
                {
                    //var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
                    //return RedirectToAction("Index", "Manage");


                    var result = await _signInManager.TwoFactorSignInAsync(model.Provider, model.Code, model.RememberMe, model.RememberBrowser);

                    if (result.Succeeded)
                    {
                        return RedirectToLocal(model.ReturnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning(7, "User account locked out.");

                        return View("Lockout");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Código inválido");

                        return View(model);
                    }

                }
                else
                    ModelState.AddModelError("Code", "El código no es válido");
            }

            return View(model);
        }


        //
        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        #endregion
    }
}
