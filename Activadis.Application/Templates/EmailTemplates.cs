using Activadis.Application.DTOs;
using Activadis.Domain.Models;
using System.Net;

namespace Activadis.Application.Templates
{
    public static class EmailTemplates
    {
        public static EmailMessage AddWaitlistConfirmation(string email, string fullName, int position)
            => CreateEmailMessage(
                   email, fullName,
                   "AddWaitlistConfirmation",
                   "De activiteit waarvoor je hebt ingeschreven zit helaas vol",
                   "De activiteit zit helaas vol",
                   new Dictionary<string, string>()
                   {
                       ["FullName"] = fullName,
                       ["Email"] = email,
                       ["Position"] = position.ToString()
                   }
               );

        public static EmailMessage RemoveWaitlistConfirmation(string email, string fullName, string token)
            => CreateEmailMessage(
                   email, fullName,
                   "RemoveWaitlistConfirmation",
                   "Bevestig je uitschrijving voor de wachtrij van de activiteit",
                   "Bevestig je uitschrijving",
                   new Dictionary<string, string>()
                   {
                       ["FullName"] = fullName,
                       ["Email"] = email,
                       ["Token"] = $"{ApplicationConfiguration.RedirectLinks.ConfirmSignOut}/{token}"
                   }
               );

        public static EmailMessage ResignUpConfirmation(string email, string fullName, string token)
            => CreateEmailMessage(
                   email, fullName,
                   "ResignUpConfirmation",
                   "De activiteit waarvoor je hebt ingeschreven heeft weer een open plek",
                   "De activiteit heeft weer een open plek",
                   new Dictionary<string, string>()
                   {
                       ["FullName"] = fullName,
                       ["Email"] = email,
                       ["Token"] = $"{ApplicationConfiguration.RedirectLinks.ConfirmResignUp}/{token}"
                   }
               );

        public static EmailMessage SignUpConfirmation(string email, string fullName, string token)
             => CreateEmailMessage(
                   email, fullName,
                   "SignUpConfirmation",
                   "Bevestig je inschrijving voor de activiteit",
                   "Bevestig je inschrijving",
                   new Dictionary<string, string>()
                   {
                       ["FullName"] = fullName,
                       ["Email"] = email,
                       ["Token"] = $"{ApplicationConfiguration.RedirectLinks.ConfirmSignUp}/{token}"
                   }
               );

        public static EmailMessage SignOutConfirmation(string email, string fullName, string token)
            => CreateEmailMessage(
                   email, fullName,
                   "SignOutConfirmation",
                   "Bevestig je uitschrijving voor de activiteit",
                   "Bevestig je uitschrijving",
                   new Dictionary<string, string>()
                   {
                       ["FullName"] = fullName,
                       ["Email"] = email,
                       ["Token"] = $"{ApplicationConfiguration.RedirectLinks.ConfirmSignOut}/{token}"
                   }
               );

        public static EmailMessage PasswordSetup(string email, string fullName, string token)
            => CreateEmailMessage(
                   email, fullName,
                   "PasswordSetup",
                   "Stel je wachtwoord in voor Activadis",
                   "Stel je wachtwoord in",
                   new Dictionary<string, string>()
                   {
                       ["FullName"] = fullName,
                       ["Email"] = email,
                       ["Token"] = $"{ApplicationConfiguration.RedirectLinks.SetupPassword}/{token}"
                   }
               );

        private static EmailMessage CreateEmailMessage(string email, string fullName, string fileName, string subject, string title, Dictionary<string, string> values)
        {
            values["BaseUrl"] = ApplicationConfiguration.BaseUrl;

            return new EmailMessage()
            {
                ToEmail = email,
                ToName = fullName,
                Subject = subject,
                HtmlBody = RenderHtml($"{fileName}.html", title, values),
                TextBody = Fill(Read($"{fileName}.txt"), values, false)
            };
        }

        private static string RenderHtml(string fileName, string title, Dictionary<string, string> values)
        {
            string content = Fill(Read(fileName), values, encode: true);

            return Read("Layout.html")
                .Replace("{{Title}}", WebUtility.HtmlEncode(title))
                .Replace("{{Content}}", content);
        }

        private static string Fill(string template, Dictionary<string, string> values, bool encode)
        {
            foreach (KeyValuePair<string, string> value in values)
                template = template.Replace("{{" + value.Key + "}}", encode ? WebUtility.HtmlEncode(value.Value) : value.Value);

            return template;
        }

        private static string Read(string fileName)
        {
            string resourceName = $"Activadis.Application.Templates.Email.{fileName}";

            using Stream stream = typeof(EmailTemplates).Assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"The email template '{fileName}' was not found.");

            using StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
