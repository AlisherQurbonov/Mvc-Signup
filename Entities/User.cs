using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace register.Entities;

public class User : IdentityUser
{
    [Required]
    [MaxLength(64)]
  public string Fullname { get; set; }
 
   [Required] 
  public DateTimeOffset Birthdate { get; set; }

}