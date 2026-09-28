using Activadis.Domain.Models;
using System.Net;

namespace Activadis.Application.Templates
{
    /// <summary>
    /// The content of every email the application sends. Each method returns a complete
    /// message with an HTML version and a plain-text version, for mail clients that do not
    /// show HTML. Every value that comes from a user is HTML-encoded before it is placed in
    /// the HTML, so a name can never add its own markup to an email.
    /// </summary>
    public static class EmailTemplates
    {
        public static EmailMessage PasswordSetup(string email, string fullName, string link)
        {
            string encodedName = WebUtility.HtmlEncode(fullName);
            string encodedEmail = WebUtility.HtmlEncode(email);
            string encodedLink = WebUtility.HtmlEncode(link);

            string content = $"""
                <p style="margin: 0 0 16px 0;">Hallo {encodedName},</p>
                <p style="margin: 0 0 16px 0;">Er is een account voor je aangemaakt bij Activadis. Klik op de knop hieronder om je wachtwoord in te stellen.</p>
                {Button("Wachtwoord instellen", encodedLink)}
                <p style="margin: 0 0 16px 0;">Daarna log je in met je e-mailadres: <strong>{encodedEmail}</strong></p>
                <p style="margin: 0; font-size: 13px; color: #6b7280;">Werkt de knop niet? Kopieer dan deze link in je browser:<br /><a href="{encodedLink}" style="color: #0e1e3b; word-break: break-all;">{encodedLink}</a></p>
                """;

            return new EmailMessage()
            {
                ToEmail = email,
                ToName = fullName,

                Subject = "Stel je wachtwoord in voor Activadis",
                HtmlBody = Layout("Stel je wachtwoord in", content),
                TextBody = $"""
                    Hallo {fullName},

                    Er is een account voor je aangemaakt bij Activadis. Open de link hieronder om je wachtwoord in te stellen:
                    {link}

                    Daarna log je in met je e-mailadres: {email}

                    Heb je deze e-mail niet verwacht? Dan kun je hem negeren.
                    """
            };
        }

        private static string Button(string text, string encodedLink) => $"""
            <table role="presentation" cellpadding="0" cellspacing="0" style="margin: 8px 0 24px 0;">
                <tr>
                    <td style="border-radius: 8px; background: #faa21b;">
                        <a href="{encodedLink}" style="display: inline-block; padding: 12px 24px; font-weight: 600; color: #0e1e3b; text-decoration: none;">{text}</a>
                    </td>
                </tr>
            </table>
            """;

        /// <summary>
        /// The frame shared by every email: header, white card and footer. Mail clients
        /// ignore stylesheets, so everything is styled inline and laid out with tables.
        /// </summary>
        private static string Layout(string title, string content) => $"""
            <!DOCTYPE html>
            <html lang="nl">
            <head>
                <meta charset="utf-8" />
                <meta name="viewport" content="width=device-width, initial-scale=1.0" />
                <title>{title}</title>
            </head>
            <body style="margin: 0; padding: 0; background: #ebf5f7;">
                <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="background: #ebf5f7; padding: 32px 16px;">
                    <tr>
                        <td align="center">
                            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width: 560px; background: #ffffff; border-radius: 12px; font-family: Poppins, Arial, sans-serif; font-size: 15px; line-height: 1.6; color: #0e1e3b;">
                                <tr>
                                    <td style="background: #0e1e3b; border-radius: 12px 12px 0 0; padding: 24px 32px; font-size: 20px; font-weight: 600; color: #ffffff;">Activadis</td>
                                </tr>
                                <tr>
                                    <td style="padding: 32px;">
                                        <h1 style="margin: 0 0 16px 0; font-size: 20px;">{title}</h1>
                                        {content}
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 16px 32px; border-top: 1px solid #e5e7eb; font-size: 12px; color: #6b7280;">Heb je deze e-mail niet verwacht? Dan kun je hem negeren.</td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }
}
