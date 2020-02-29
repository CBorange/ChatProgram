using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatProgram.Util
{
    public class NetworkUtil
    {
        public static bool ValidateIPv4(string ipString)
        {
            string[] splitedIP = ipString.Split('.');
            if (splitedIP.Length != 4)
                return false;

            byte tempForParse;
            return splitedIP.All(r => byte.TryParse(r, out tempForParse));
        }
    }
}
