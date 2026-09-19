using CIMC.Helper.SM4;
using Org.BouncyCastle.Utilities.Encoders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMC.Helper.SM4
{
    /// <summary>提供 SM4 加解密的便捷方法。</summary>
    public class SM4Utils
    {
        /// <summary>加解密密钥。</summary>
        public String secretKey = "";
        /// <summary>CBC 模式的初始向量。</summary>
        public String iv = "";
        /// <summary>十六进制编码文本。</summary>
        public bool hexString = false;

        /// <summary>以 ECB 模式加密数据。</summary>
        public String Encrypt_ECB(String plainText)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = SM4CryptoServiceProvider.SM4_ENCRYPT;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
            }
            else
            {
                keyBytes = Encoding.ASCII.GetBytes(secretKey);
            }

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_ecb(ctx, Encoding.ASCII.GetBytes(plainText));

            String cipherText = Encoding.ASCII.GetString(Hex.Encode(encrypted));
            return cipherText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="plainText"></param>    
        /// <returns></returns>
        public String Encrypt_ECB_ToBase64(String plainText)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = SM4CryptoServiceProvider.SM4_ENCRYPT;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
            }
            else
            {
                keyBytes = Encoding.UTF8.GetBytes(secretKey);
            }

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_ecb(ctx, Encoding.UTF8.GetBytes(plainText));

            return Convert.ToBase64String(encrypted);
        }

        /// <summary>以 ECB 模式加密数据。</summary>
        public byte[] Encrypt_ECB(byte[] plainBytes, byte[] keyBytes)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = false;
            ctx.mode = SM4CryptoServiceProvider.SM4_ENCRYPT;

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_ecb(ctx, plainBytes);
            return encrypted;
            //return Hex.Encode(encrypted);
        }

        /// <summary>以 ECB 模式解密数据。</summary>
        public String Decrypt_ECB(String cipherText)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = SM4CryptoServiceProvider.SM4_DECRYPT;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
            }
            else
            {
                keyBytes = Encoding.ASCII.GetBytes(secretKey);
            }

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            byte[] decrypted = sm4.sm4_crypt_ecb(ctx, Hex.Decode(cipherText));
            return Encoding.ASCII.GetString(decrypted);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64Str">Base64 编码的密文。</param>
        /// <returns></returns>
        public String Decrypt_ECB_ByBase64(String base64Str)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = SM4CryptoServiceProvider.SM4_DECRYPT;

            byte[] keyBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
            }
            else
            {
                keyBytes = Encoding.ASCII.GetBytes(secretKey);
            }
            byte[] decbuff = Convert.FromBase64String(base64Str);

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            byte[] decrypted = sm4.sm4_crypt_ecb(ctx, decbuff);
            return Encoding.UTF8.GetString(decrypted);
        }


        /// <summary>以 CBC 模式加密数据。</summary>
        public String Encrypt_CBC(String plainText)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = SM4CryptoServiceProvider.SM4_ENCRYPT;

            byte[] keyBytes;
            byte[] ivBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
                ivBytes = Hex.Decode(iv);
            }
            else
            {
                keyBytes = Encoding.ASCII.GetBytes(secretKey);
                ivBytes = Encoding.ASCII.GetBytes(iv);
            }

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_enc(ctx, keyBytes);
            byte[] encrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, Encoding.ASCII.GetBytes(plainText));

            String cipherText = Encoding.ASCII.GetString(Hex.Encode(encrypted));
            return cipherText;
        }

        /// <summary>以 CBC 模式解密数据。</summary>
        public String Decrypt_CBC(String cipherText)
        {
            SM4Context ctx = new SM4Context();
            ctx.isPadding = true;
            ctx.mode = SM4CryptoServiceProvider.SM4_DECRYPT;

            byte[] keyBytes;
            byte[] ivBytes;
            if (hexString)
            {
                keyBytes = Hex.Decode(secretKey);
                ivBytes = Hex.Decode(iv);
            }
            else
            {
                keyBytes = Encoding.ASCII.GetBytes(secretKey);
                ivBytes = Encoding.ASCII.GetBytes(iv);
            }

            SM4CryptoServiceProvider sm4 = new SM4CryptoServiceProvider();
            sm4.sm4_setkey_dec(ctx, keyBytes);
            byte[] decrypted = sm4.sm4_crypt_cbc(ctx, ivBytes, Hex.Decode(cipherText));
            return Encoding.ASCII.GetString(decrypted);
        }
    }
}
