using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using register.Entities;
using register.ViewModels;

namespace register.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<User> _userM;
    private readonly SignInManager<User> _signInM;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AccountController> logger)
    {
        _userM = userManager;
        _signInM = signInManager;
        _logger = logger;
    }
    
[HttpGet]
 public IActionResult Signup(string returnUrl)
    {
        return View(new SignupViewModel() { ReturnUrl = returnUrl ?? string.Empty });
    }

 [HttpPost]
  public async Task<IActionResult> Signup(SignupViewModel model)
    {
        var user = new User()
        {
            Fullname = model.Fullname,
            Email = model.Email,
            UserName =model.Username,
            PhoneNumber = model.Phone,
            Birthdate = model.Birthdate
        };

        var result = await _userM.CreateAsync(user, model.Password);

        if(result.Succeeded)
        {
            return LocalRedirect($"{model.ReturnUrl}");
        }

        return BadRequest(JsonSerializer.Serialize(result.Errors));

    }


    [HttpGet]
    public  IActionResult Login(string returnUrl)
    {
        return View(new SigninViewModel() { ReturnUrl  = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(SigninViewModel model)
    {
        var user = await _userM.FindByEmailAsync(model.Email);
        if(user != null)
        {
            await _signInM.PasswordSignInAsync(user, model.Password, false, false); // isPersistant

            return LocalRedirect($"{model.ReturnUrl ?? "/"}");
        }

        return BadRequest("Wrong credentials");
    }

    [Authorize]
    [HttpGet]
    public IActionResult Users()
    {
        var users = _userM.Users.ToList();
        return View(users);
    }
}