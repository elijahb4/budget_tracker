using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;

namespace IndividualProjectInitial
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserModel
    {
        public User UserInstance { get; private set; }

        public UserModel()
        {
            UserInstance = new User();
        }

        public void SetUserInstance(User user)
        {
            UserInstance = user ?? throw new ArgumentNullException(nameof(user));
        }
    }
}
