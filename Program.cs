using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace mainApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            bool loggedIn = false;
            do
            {
                Console.Clear();
                DisplayTitle();

                Console.WriteLine();
                Console.WriteLine();

                // Change the color for the email prompt to cyan with light yellow text
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("1");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("] Discord Email: ");
                Console.ResetColor();
                string email = Console.ReadLine();

                Console.WriteLine();

                // Change the color for the password prompt to cyan with orange text
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[");
                Console.ForegroundColor = ConsoleColor.Red; // Orange color
                Console.Write("2");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("] Discord Password: ");
                Console.ResetColor();
                string pass = Console.ReadLine();

                loggedIn = await GetToken(email, pass);

                if (!loggedIn)
                {
                    Console.WriteLine();
                    Console.WriteLine("Press Enter to try again...");
                    Console.ReadLine();
                }

            } while (true);
        }

        static async Task<bool> GetToken(string email, string pass)
        {
            string url = "https://discord.com/api/v9/auth/login";
            var payload = new Dictionary<string, string>()
            {
                {"login", email}, {"password",  pass}
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            HttpClient client = new HttpClient();
            StringContent stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseContent);

            if (response.IsSuccessStatusCode)
            {
                // Print "Login Successful!" centered in the console, in green color
                PrintLoginSuccess(json);
                return true;
            }
            else if (json["code"]?.ToString() == "50002" && json["message"]?.ToString() == "Verify your account")
            {
                // Prompt for 2FA code if required
                Console.Write("Enter your 2FA code: ");
                string twoFactorCode = Console.ReadLine();

                // Verify 2FA code
                return await Verify2FA(json["ticket"].ToString(), twoFactorCode);
            }
            else
            {
                // Print "Login Failed!" centered in the console, in red color
                PrintLoginFailed();
                return false;
            }
        }

        static async Task<bool> Verify2FA(string ticket, string twoFactorCode)
        {
            string url = "https://discord.com/api/v9/auth/mfa/totp";
            var payload = new Dictionary<string, string>()
            {
                {"code", twoFactorCode}, {"ticket", ticket}
            };
            string jsonPayload = JsonConvert.SerializeObject(payload);
            HttpClient client = new HttpClient();
            StringContent stringContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, stringContent);
            string responseContent = await response.Content.ReadAsStringAsync();
            JObject json = JObject.Parse(responseContent);

            if (response.IsSuccessStatusCode)
            {
                // Print "Login Successful!" centered in the console, in green color
                PrintLoginSuccess(json);
                return true;
            }
            else
            {
                // Print "Login Failed!" centered in the console, in red color
                PrintLoginFailed();
                return false;
            }
        }

        static void PrintLoginSuccess(JObject json)
        {
            string loginMessage = "Login Successful! > Disabled 2FA in your account if Token doesn't appear <";
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            int loginMessageCenter = (Console.WindowWidth / 2) - (loginMessage.Length / 2);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(loginMessageCenter, Console.CursorTop);
            Console.WriteLine(loginMessage);
            Console.ResetColor();

            // Print user ID and token
            Console.WriteLine(" ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan; // Cyan text
            Console.Write("User ID");
            Console.ForegroundColor = ConsoleColor.DarkYellow; // Yellow brackets
            Console.Write("]: ");
            Console.ForegroundColor = ConsoleColor.Yellow; // Yellow text
            Console.WriteLine($"{json["user_id"]}");

            Console.WriteLine(" ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan; // Cyan text
            Console.Write("Token");
            Console.ForegroundColor = ConsoleColor.DarkYellow; // Yellow brackets
            Console.Write("]: ");
            Console.ForegroundColor = ConsoleColor.Yellow; // Yellow text
            Console.WriteLine($"{json["token"]}");

            Console.WriteLine();
            Console.WriteLine(" Press Enter to try again...");
            Console.ReadLine();
        }


        static void PrintLoginFailed()
        {
            string errorMessage = "Login Failed! > Check the details or you need to verify the login once in the Email <";
            Console.WriteLine(" ");
            Console.WriteLine(" ");
            int errorMessageCenter = (Console.WindowWidth / 2) - (errorMessage.Length / 2);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(errorMessageCenter, Console.CursorTop);
            Console.WriteLine(errorMessage);
            Console.ResetColor();
        }

        static void DisplayTitle()
        {
            string title = @"

███████╗██╗  ██╗         ██████╗ ██████╗ ██████╗ ███████╗██████╗ 
╚══███╔╝╚██╗██╔╝        ██╔════╝██╔═══██╗██╔══██╗██╔════╝██╔══██╗
  ███╔╝  ╚███╔╝         ██║     ██║   ██║██║  ██║█████╗  ██████╔╝
 ███╔╝   ██╔██╗         ██║     ██║   ██║██║  ██║██╔══╝  ██╔══██╗
███████╗██╔╝ ██╗███████╗╚██████╗╚██████╔╝██████╔╝███████╗██║  ██║
╚══════╝╚═╝  ╚═╝╚══════╝ ╚═════╝ ╚═════╝ ╚═════╝ ╚══════╝╚═╝  ╚═╝

";
            string subtitle = "Discord Token Finder";
            string githubLink = "GitHub: https://github.com/charlotte-zee";

            // Calculate the width of the console window
            int windowWidth = Console.WindowWidth;

            // Center each line of the title
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var line in title.Split('\n'))
            {
                int padding = (windowWidth - line.Length) / 2;
                Console.SetCursorPosition(padding, Console.CursorTop);
                Console.WriteLine(line);
            }

            // Center the subtitle
            int subtitlePadding = (windowWidth - subtitle.Length) / 2;
            Console.SetCursorPosition(subtitlePadding, Console.CursorTop);
            Console.WriteLine(subtitle);

            // Center the GitHub link
            int githubPadding = (windowWidth - githubLink.Length) / 2;
            Console.SetCursorPosition(githubPadding, Console.CursorTop);
            Console.WriteLine(githubLink);

            // Reset the color to default
            Console.ResetColor();
        }
    }
}
