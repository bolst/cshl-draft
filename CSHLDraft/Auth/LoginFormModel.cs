using System.ComponentModel.DataAnnotations;

namespace CSHLDraft.Data;



public class LoginFormModel
{
    [Required(ErrorMessage="Email is required")] 
    public string Email { get; set; }

    [Required(ErrorMessage="Password is required")] 
    public string Password { get; set; }
}