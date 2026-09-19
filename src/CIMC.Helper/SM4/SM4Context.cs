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
        /// <summary>加解密模式。</summary>
        public int mode;

        /// <summary>SM4 轮密钥。</summary>
        public int[] sk;

        /// <summary>是否启用数据填充。</summary>
        public bool isPadding;

        /// <summary>初始化SM4 加解密。</summary>
        public SM4Context()
        {
            this.mode = 1;
            this.isPadding = true;
            this.sk = new int[32];
        }
    }
}
