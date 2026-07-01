using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Helper.SM4
{
    /// <summary>
    /// 
    /// </summary>
    public class SM4Context
    {
        public int mode;

        public int[] sk;

        public bool isPadding;

        public SM4Context()
        {
            this.mode = 1;
            this.isPadding = true;
            this.sk = new int[32];
        }
    }
}
