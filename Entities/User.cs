using Microsoft.AspNetCore.Identity;

namespace register.Entities;

public class User : IdentityUser
{
  public string Fullname { get; set; }
  public string Phone { get; set; }
  public DateTimeOffset Birthdate { get; set; }

}