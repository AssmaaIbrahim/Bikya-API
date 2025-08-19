using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using MimeKit;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bikya.Services.Services
{
    public class GmailServiceHelper
    {
        private static readonly string[] Scopes = { GmailService.Scope.GmailSend };
        private static readonly string ApplicationName = "Bikya Mailer";

        public static async Task<GmailService> GetGmailServiceAsync()
        {
            var clientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
            var refreshToken = Environment.GetEnvironmentVariable("GOOGLE_REFRESH_TOKEN");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
            {
                throw new InvalidOperationException("Google OAuth credentials (ClientId, ClientSecret, RefreshToken) not found in environment variables.");
            }

            var token = new TokenResponse { RefreshToken = refreshToken };
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                Scopes = Scopes
            });

            var credential = new UserCredential(flow, "user", token);

            await credential.RefreshTokenAsync(CancellationToken.None);

            return new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName
            });
        }

        public static async Task SendEmailAsync(string to, string subject, string body)
        {
            var gmailService = await GetGmailServiceAsync();

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress("Bikya", "me"));
            emailMessage.To.Add(MailboxAddress.Parse(to));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = body };

            using var stream = new MemoryStream();
            emailMessage.WriteTo(stream);
            var rawMessage = Convert.ToBase64String(stream.ToArray())
                .Replace("+", "-").Replace("/", "_").Replace("=", "");

            var message = new Google.Apis.Gmail.v1.Data.Message { Raw = rawMessage };
            await gmailService.Users.Messages.Send(message, "me").ExecuteAsync();
        }
    }
}