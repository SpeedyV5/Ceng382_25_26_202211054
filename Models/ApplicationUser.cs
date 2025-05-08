using Microsoft.AspNetCore.Identity;

namespace ClassManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Ekstra özellikler eklemek isterseniz, buraya ekleyebilirsiniz.
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
