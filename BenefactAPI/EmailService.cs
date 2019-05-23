using BenefactAPI.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BenefactAPI
{
    public class EmailService
    {
        const string logo = "iVBORw0KGgoAAAANSUhEUgAAADwAAAA8CAYAAAA6/NlyAAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAF3AAABdwB9uetrwAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAMOSURBVGiB3dtNb9MwGAfwv52k7dpJRSA48TJBkZD4AhUHpo3uAKifZeLKBXGZ2JUDQuxbcCUSYvseCFVT0Q6Ijg6SLonNyUWaWJLHcV7c/7V2k1+tJPbjlKGEDA7GQ7aIfa/X6Tmck/sHZ0EQnkZPj19++mL63Ohnk5HBwXjYdt1DCdb7PZ0jEYL+JRJriBL/5t7OpunzMwpWWM65BwDyXEAXzQXcMtDGwBexKk1DGwFfhlVpErowOAur0hR0IXBerEoT0NpgKlalbrQWWBerUieaDC6KVakLTQKbwqrUgc4NNo1VqRqdC1wWVqVKdCa4bKxKVehU8IMPzx+1HfeobKyKQotID82ixL/1ZvtxWjs37cOzkz97PA5S2/wvkgNxV779/vrzLrXvxv5oGC7Cj6KP69S+gJxK5h2ntSBjMg/JgbgnIBl+Uvtu7I+GIkwOWYt5gKQeeSKZt3Xywv+a1sro8nCJ1fhWhYVgGpdPPixgEGwDFjAEtgULGADbhAUKgm3DAgXANmIBTbCtWEADbDMWIIJtxwIE8CpggZzgVcECOcCrhAUywIJjpotlEg+TUBxpYSVmYGzTNBbIWC25nttdv7YOnR3AMFzcm/P5BGB3qX07nbZYQ+sOgG/kA2ckfYQXcUu7CsHYL+54W4Akj5LDnavtrusP3o2r3z0sUnqZ7voTXTRn3C0Dne8uvULo/M/hFUHTZlorgKbPpS1H662WLEbrr4ctRRereFiILl7TsgxtpmppEdpcXdoStNmdBwvQxl89XKKT5Jza9x+avi+l0PffP9PfPWzf6J6GQaAxYgLxLNm5/WqbuiMGORNYOCGiKKR2heAAPIeltUkdYZfx/pV+X2s9rJNldcUh/04QHDE8Z5T1Bm6mhHOOKtBFSkl5sUDOa7hsdFVYgHDTKgtdJRYg3qVNo6vGAhqPJVPoOrCA5nO4KLouLFBg4qGLrhMLFJxpUdF1YwEDU8u86CZgAUNz6Sx0U7CAwcXDZegmYQHDq6WL6KZhgRKWhwrNPa6NlQw/WBtPyvgr3l+dRl6cd1QKMQAAAABJRU5ErkJggg==";
        string baseURL;
        string sendKey;
        public EmailService(IServiceProvider services)
        {
            var config = services.GetService<IConfiguration>();
            baseURL = config.GetValue<string>("BaseURL");
            sendKey = config.GetValue<string>("SendKey");
        }
        public async Task SendEmail(string toAddress, string subject, string emailPath, Dictionary<string, string> variables)
        {
            var client = new SendGridClient(sendKey);
            var from = new EmailAddress($"noreply@{baseURL}", "Benefact - No Reply");
            var to = new EmailAddress(toAddress, "Benefact User");
            var template = await File.ReadAllTextAsync(Path.Combine("Content", "email_template.html"));
            var content = await File.ReadAllTextAsync(Path.Combine("Content", emailPath));
            template = template.Replace("{{content}}", content);
            foreach (var variable in variables)
                template = template.Replace($"{{{{{variable.Key}}}}}", variable.Value);
            template = template.Replace("{{baseURL}}", $"https://{baseURL}");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, template);
            msg.AddAttachment(new Attachment()
            {
                Content = logo,
                ContentId = "logo",
                Disposition = "inline",
                Filename = "logo.png",
                Type = "image/png",
            });
            try
            {
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
                if (!(response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted))
                    throw new HTTPError($"Failed to send verification email\n{await response.Body.ReadAsStringAsync()}");
            }
            catch (HttpRequestException)
            {
                throw new HTTPError("Failed to send verification");
            }
        }
    }
}
