using System.Security.Cryptography;
using System.Text;

public static string HashPassword(string password)
{
    // สร้าง Salt สุ่ม
    byte[] salt;
    new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

    // สร้าง Hash ด้วย PBKDF2 (Password-Based Key Derivation Function 2)
    var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000); // 10000 คือ iterations
    byte[] hash = pbkdf2.GetBytes(20); // 20 คือขนาดของ hash

    // รวม Salt และ Hash เพื่อเก็บ
    byte[] hashBytes = new byte[36];
    Array.Copy(salt, 0, hashBytes, 0, 16);
    Array.Copy(hash, 0, hashBytes, 16, 20);

    // แปลงเป็น Base64 String เพื่อเก็บใน DB
    return Convert.ToBase64String(hashBytes);
}

public static bool VerifyPassword(string enteredPassword, string storedHash)
{
    byte[] hashBytes = Convert.FromBase64String(storedHash);
    byte[] salt = new byte[16];
    Array.Copy(hashBytes, 0, salt, 0, 16);

    var pbkdf2 = new Rfc2898DeriveBytes(enteredPassword, salt, 10000);
    byte[] hash = pbkdf2.GetBytes(20);

    // เปรียบเทียบ hash ที่คำนวณได้กับ hash ที่เก็บไว้
    for (int i = 0; i < 20; i++)
    {
        if (hashBytes[i + 16] != hash[i])
            return false;
    }
    return true;
}