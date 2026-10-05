using Activadis.Domain.Models;
using System.Net;

namespace Activadis.Application.Templates
{
    public static class EmailTemplates
    {
        public static EmailMessage SignUpConfirmation(string email, string fullName, string link)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                ["FullName"] = fullName,
                ["Email"] = email,
                ["Link"] = link
            };

            return new EmailMessage()
            {
                ToEmail = email,
                ToName = fullName,

                Subject = "Bevestig je inschrijving voor de activiteit",
                HtmlBody = RenderHtml("SignUpConfirmation.html", "Bevestig je inschrijving", values),
                TextBody = Fill(Read("SignUpConfirmation.txt"), values, false)
            };
        }

        public static EmailMessage PasswordSetup(string email, string fullName, string link)
        {
            Dictionary<string, string> values = new Dictionary<string, string>()
            {
                ["FullName"] = fullName,
                ["Email"] = email,
                ["Link"] = link
            };

            return new EmailMessage()
            {
                ToEmail = email,
                ToName = fullName,

                Subject = "Stel je wachtwoord in voor Activadis",
                HtmlBody = RenderHtml("PasswordSetup.html", "Stel je wachtwoord in", values),
                TextBody = Fill(Read("PasswordSetup.txt"), values, encode: false)
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
