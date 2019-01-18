using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace api.Helpers
{
    public class JwtTokenHelper
    {
        public static SymmetricSecurityKey GetSymmetricSecurityKey(string secret)
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }
    }
}
