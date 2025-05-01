namespace BusinessLogicLayer.Helper
{
    public static class PasswordHelper
    {
        public static string PasswordHash(string plainPassword) =>
            BCrypt.Net.BCrypt.HashPassword(plainPassword);

        public static bool VerifyPassword(string plainPassword, string hashedPassword) =>
            BCrypt.Net.BCrypt.Verify(plainPassword, hashedPassword);
    }
}
