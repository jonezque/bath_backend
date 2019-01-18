using System.Collections.Generic;

namespace api.Models
{
    public class UserModel
    {
        public string Name { get; set; }

        public ICollection<string> Roles { get; set; }
    }
}
