using System.Text.RegularExpressions;

namespace Its.Onix.Api.Utils
{
    public static class ValidationUtils
    {
        public static ValidationResult ValidatePassword(string password)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{7,15}$");
            var ok = regex.IsMatch(password);

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_PASSWORD";
                result.Description = @"
1) Lenght of password must be between 7-15
2) Atleast 1 lower letter
3) Atleast 1 capital letter    
4) Atleast 1 special letter in this set {#, !, @, #}
";
            }

            return result;
        }

        public static ValidationResult ValidateUserName(string userName)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            var regex = new Regex(@"^[a-zA-Z0-9._]{4,20}$");
            var ok = regex.IsMatch(userName);

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_USERNAME";
                result.Description = "User name must be in this regex format --> [a-zA-Z0-9._]{4,20}";
            }

            return result;
        }

        public static ValidationResult ValidateEmail(string email)
        {
            var result = new ValidationResult() { Status = "OK", Description = "" };

            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            var ok = regex.IsMatch(email);

            if (!ok)
            {
                result.Status = "ERROR_VALIDATION_EMAIL";
                result.Description = "Incorrect email format";
            }

            return result;
        }
    }
}
