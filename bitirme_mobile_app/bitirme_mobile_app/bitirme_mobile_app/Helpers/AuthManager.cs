using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bitirme_mobile_app.Helpers
{
    public class AuthManager
    {
        public static bool IsLoggedIn()
        {
            return DBHelper.checkLoggedIn();
        }
    }
}
