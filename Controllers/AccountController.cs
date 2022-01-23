using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
         if(!ModelState.IsValid)
        {
            return View(model);
        }

        if(await _userM.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email already exists.");
            return View(model);
        }

        if(await _userM.Users.AnyAsync(u => u.PhoneNumber == model.Phone))
        {
            ModelState.AddModelError("Phone", "Phone already exists.");
            return View(model);
        }

        var newUser = new User()
        {
            Fullname = model.Fullname,
            Email = model.Email,
            PhoneNumber = model.Phone,
            Birthdate = model.Birthdate,
            UserName = model.Email.Substring(0, model.Email.IndexOf('@'))
        };

       var result = await _userM.CreateAsync(newUser, model.Password);
        await _userM.AddToRoleAsync(newUser, "superadmin");

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
        if(!ModelState.IsValid)
        {
            return View(model);
        }

        var user = _userM.Users.FirstOrDefault(u => u.Email == model.Email);

        var result = await _signInM.PasswordSignInAsync(user, model.Password, false, false);
        if(result.Succeeded)
        {
            return LocalRedirect($"/");
        }

        return BadRequest(result.IsNotAllowed);
    }

    
    
    [Authorize(Roles = "superadmin")]
    [HttpGet]
    public async Task<IActionResult> Users(string query, int page = 1, int limit = 10)
    {
        var filteredUsers = string.IsNullOrWhiteSpace(query) 
        ? _userM.Users
        : _userM.Users.Where(u => u.Fullname == query || u.Email == query || u.PhoneNumber == query);

        var users = await filteredUsers
        .Skip((page - 1) * limit)
        .Take(limit)
        .Select(u => new UserViewModel
        {
            Fullname = u.Fullname,
            Phone = u.PhoneNumber,
            Email = u.Email,
            Username = u.UserName,
            Birthdate = u.Birthdate
        }).ToListAsync();

        var totalUsers = filteredUsers.Count();

        return View(new UsersViewModel()
        {
            Users = users,
            TotalUsers = totalUsers,
            Pages =  (int)Math.Ceiling(totalUsers / (double)page),
            Page = page
        });
    }

}