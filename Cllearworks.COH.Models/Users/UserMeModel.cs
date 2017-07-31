using System.Collections.Generic;

namespace Cllearworks.COH.Models.Users
{
    public class UserMeModel : UserModel
    {
        public Dictionary<string, bool> Permissions { get; set; }
    }
}
