using ApplicationCore.Models;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApplicationCore.Helpers
{
    public static class Helper
    {
        public static string CurrentFolder =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string UppercaseFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }
        

        // https://damienbod.com/2020/08/19/symmetric-and-asymmetric-encryption-in-net-core/
        public static (string Key, string IVBase64) InitSymmetricEncryptionKeyIV()
        {
            var key = GetEncodedRandomString(32); // 256
            Aes cipher = CreateCipher(key);
            var IVBase64 = Convert.ToBase64String(cipher.IV);
            return (key, IVBase64);
        }

        private static string GetEncodedRandomString(int length)
        {
            var base64 = Convert.ToBase64String(GenerateRandomBytes(length));
            return base64;
        }

        private static Aes CreateCipher(string keyBase64)
        {
            // Default values: Keysize 256, Padding PKC27
            Aes cipher = Aes.Create();
            cipher.Mode = CipherMode.CBC;  // Ensure the integrity of the ciphertext if using CBC

            cipher.Padding = PaddingMode.ISO10126;
            cipher.Key = Convert.FromBase64String(keyBase64);

            return cipher;
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            var byteArray = new byte[length];
            RandomNumberGenerator.Fill(byteArray);
            return byteArray;
        }

        public static string Encrypt(
            string text, 
            string IV, 
            string key)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = Convert.FromBase64String(IV);

            ICryptoTransform cryptTransform = cipher.CreateEncryptor();
            byte[] plaintext = Encoding.UTF8.GetBytes(text);
            byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

            return Convert.ToBase64String(cipherText);
        }

        public static string Decrypt(
            string encryptedText, 
            string IV, 
            string key)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = Convert.FromBase64String(IV);

            ICryptoTransform cryptTransform = cipher.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public static IEnumerable<Claim> ToClaims(
            this IEnumerable<RolePermission> permissions)
        {
            var claims = new List<Claim>();

            foreach (var p in permissions)
            {
                var c = p.ToClaim();

                if (c != null)
                    claims.Add(c);
            }

            return claims;
        }

        public static Claim? ToClaim(
            this RolePermission permission)
        {            
            var values = new List<string>();

            if (permission.can_read)
                values.Add(Constants.Permissions.read);
            if (permission.can_edit)
                values.Add(Constants.Permissions.edit);
            if (permission.can_create)
                values.Add(Constants.Permissions.create);
            if (permission.can_delete)
                values.Add(Constants.Permissions.delete);

            switch (permission.name)
            {
                case Constants.ClaimNames.roles:
                    return new Claim(Constants.ClaimNames.users, string.Join(" ", values));                    
                case Constants.ClaimNames.users:
                    return new Claim(Constants.ClaimNames.roles, string.Join(" ", values));                    
            }

            return null;
        }

        public static IEnumerable<RolePermission> ToRolePermissions(
            this IEnumerable<Claim> claims)
        {
            var permissions = Constants.PermissionCheckList;

            foreach (var claim in claims)
            {
                var found = permissions.FirstOrDefault(_ => _.name == claim.Type);

                if (found == null) continue;

                var values = claim.Value.Split(" ");

                if (values.Contains(Constants.Permissions.read))
                    found.can_read = true;
                if (values.Contains(Constants.Permissions.edit))
                    found.can_edit = true;
                if (values.Contains(Constants.Permissions.create))
                    found.can_create = true;
                if (values.Contains(Constants.Permissions.delete))
                    found.can_delete = true;
                if (values.Contains(Constants.Permissions.bulk))
                    found.can_bulk = true;
            }

            return permissions;
        }

        public static RolePermission GetPermission(
            this IEnumerable<Claim> claims, string name)
        {
            var permissions = claims.ToRolePermissions();

            return permissions.FirstOrDefault(_ => _.name == name);
        }

        // https://monkelite.com/how-to-generate-a-random-password-using-csharp-and-dotnet-core/
        public static string CreateRandomPasswordWithRandomLength()
        {
            // Create a string of characters, numbers, special characters that allowed in the password  
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
            Random random = new Random();

            // Minimum size 8. Max size is number of all allowed chars.  
            int size = random.Next(8, validChars.Length);

            // Select one random character at a time from the string  
            // and create an array of chars  
            char[] chars = new char[size];
            for (int i = 0; i < size; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            return new string(chars);
        }
    }
}
