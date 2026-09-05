using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Recipe_Project.Data;
using Recipe_Project.Models;
using Recipe_Project.Services;
using Recipe_Project.ViewModels;

namespace Recipe_Project.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            AppDbContext context, 
            IEmailService emailService, 
            ILogger<AccountController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "An account with this email address already exists.");
                return View(model);
            }

            var isChef = model.RegisterAsChef;
            var chefTitle = isChef 
                ? (string.IsNullOrWhiteSpace(model.ChefTitle) ? "Verified Master Chef" : model.ChefTitle.Trim()) 
                : null;

            var user = new ApplicationUser
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.ToLower().Trim(),
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = isChef ? "Chef" : "User",
                IsVerifiedChef = isChef,
                ChefTitle = chefTitle,
                AvatarUrl = isChef ? "/images/author/author1.jpg" : "/images/author/user.png",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Sign in newly registered user
            await SignInUserAsync(user, false);

            TempData["SuccessMessage"] = isChef
                ? $"Welcome Chef {user.FullName}! Your verified chef profile is ready."
                : $"Welcome {user.FullName}! Your account has been created successfully.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email address or password.");
                return View(model);
            }

            await SignInUserAsync(user, model.RememberMe);

            TempData["SuccessMessage"] = $"Welcome back, {user.FullName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpPost]
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "You have been logged out.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower().Trim());
            if (user == null)
            {
                ModelState.AddModelError("Email", "No account registered with this email address was found.");
                return View(model);
            }

            // Generate 6-digit cryptographic numeric OTP
            var otp = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.PasswordResetOtp = otp;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            // Dispatch OTP via Gmail SMTP
            var emailSent = await _emailService.SendPasswordResetOtpAsync(user.Email, user.FullName, otp);

            if (emailSent)
            {
                TempData["SuccessMessage"] = $"A 6-digit verification code has been sent to {user.Email}.";
            }
            else
            {
                TempData["WarningMessage"] = $"A 6-digit reset code was generated for {user.Email}. (Check server console/logs if Gmail SMTP is in demo mode: OTP is {otp})";
            }

            return RedirectToAction("ResetPassword", new { email = user.Email });
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string? email)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordViewModel
            {
                Email = email.Trim()
            };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower().Trim());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User account not found. Please try again.");
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(user.PasswordResetOtp) || 
                !string.Equals(user.PasswordResetOtp.Trim(), model.Otp.Trim(), StringComparison.Ordinal))
            {
                ModelState.AddModelError("Otp", "Invalid verification code (OTP). Please check and try again.");
                return View(model);
            }

            if (!user.PasswordResetOtpExpiry.HasValue || DateTime.UtcNow > user.PasswordResetOtpExpiry.Value)
            {
                ModelState.AddModelError("Otp", "This verification code has expired. Please request a new code.");
                return View(model);
            }

            // Update password
            user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            user.PasswordResetOtp = null;
            user.PasswordResetOtpExpiry = null;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your password has been reset successfully! Please sign in with your new password.";
            return RedirectToAction("Login");
        }

        // POST: /Account/ResendOtp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToAction("ForgotPassword");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower().Trim());
            if (user == null)
            {
                TempData["ErrorMessage"] = "User account not found.";
                return RedirectToAction("ForgotPassword");
            }

            var otp = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
            user.PasswordResetOtp = otp;
            user.PasswordResetOtpExpiry = DateTime.UtcNow.AddMinutes(15);
            await _context.SaveChangesAsync();

            var sent = await _emailService.SendPasswordResetOtpAsync(user.Email, user.FullName, otp);
            if (sent)
            {
                TempData["SuccessMessage"] = $"A fresh 6-digit code has been sent to {user.Email}.";
            }
            else
            {
                TempData["WarningMessage"] = $"New code generated for {user.Email}. (Check server console/logs: OTP is {otp})";
            }

            return RedirectToAction("ResetPassword", new { email = user.Email });
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(ApplicationUser user, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("AvatarUrl", user.AvatarUrl ?? "/images/author/user.png")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = isPersistent ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(12)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
