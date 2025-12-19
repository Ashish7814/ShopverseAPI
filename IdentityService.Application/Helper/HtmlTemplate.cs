using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityService.Application.Helper
{
    public class HtmlTemplate
    {
        public static string BuildEmailHtml(string displayName, string callbackUrl)
        {
            // You can move this to a template file or Razor view if preferred.
            return $@"
<!doctype html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <title>Confirm your email</title>
  </head>
  <body style=""font-family: Arial, sans-serif; line-height:1.5; color:#333;"">
    <div style=""max-width:600px; margin:0 auto; padding:20px;"">
      <h2 style=""color:#2E86C1;"">Welcome to ShiftEase, {System.Net.WebUtility.HtmlEncode(displayName)}!</h2>
      <p>Thanks for creating an account. Please confirm your email address by clicking the button below:</p>

      <p style=""text-align:center; margin:30px 0;"">
        <a href=""{callbackUrl}"" style=""background:#2E86C1; color:#fff; padding:12px 20px; text-decoration:none; border-radius:6px; display:inline-block;"">
          Confirm Email
        </a>
      </p>

      <p>If the button doesn't work, copy and paste the following link into your browser:</p>
      <p style=""word-break:break-all;""><a href=""{callbackUrl}"">{callbackUrl}</a></p>

      <hr />
      <p style=""font-size:12px; color:#888;"">If you didn't create an account, you can safely ignore this email.</p>
    </div>
  </body>
</html>
";
        }

        public static string BuildForgotPasswordEmailHtml(string displayName, string callbackUrl)
        {
            return $@"
<!doctype html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <title>Reset your password</title>
  </head>
  <body style=""font-family: Arial, sans-serif; line-height:1.5; color:#333;"">
    <div style=""max-width:600px; margin:0 auto; padding:20px;"">
      <h2 style=""color:#2E86C1;"">Hello {System.Net.WebUtility.HtmlEncode(displayName)},</h2>

      <p>We received a request to reset your password for your ShiftEase account.</p>
      <p>If you made this request, click the button below to choose a new password:</p>

      <p style=""text-align:center; margin:30px 0;"">
        <a href=""{callbackUrl}"" 
           style=""background:#2E86C1; color:#fff; padding:12px 20px; text-decoration:none; border-radius:6px; display:inline-block;"">
          Reset Password
        </a>
      </p>

      <p>If that doesn't work, copy and paste the link below into your browser:</p>
      <p style=""word-break:break-all;"">
        <a href=""{callbackUrl}"">{callbackUrl}</a>
      </p>

      <hr />
      <p style=""font-size:12px; color:#888;"">
        If you didn’t request a password reset, no action is required. You can safely ignore this email.
      </p>
    </div>
  </body>
</html>
";
        }
    }
}
