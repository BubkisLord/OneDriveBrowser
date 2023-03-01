using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace OneDriveApiBrowser
{
    public class AuthenticationHelper
    {
        public static bool SignedOut = true;
        public static string SigninKey = "";
        // The Client ID is used by the application to uniquely identify itself to the v2.0 authentication endpoint.
        static string clientId = FormBrowser.MsaClientId;
        public static string[] Scopes = { "Files.ReadWrite.All" };

        

        public static string TokenForUser = null;
        public static DateTimeOffset Expiration;

        private static GraphServiceClient graphClient = null;

        // Get an access token for the given context and resourceId. An attempt is first made to 
        // acquire the token silently. If that fails, then we try to acquire the token by prompting the user.
        public static GraphServiceClient GetAuthenticatedClient()
        {
            if (graphClient == null)
            {
                // Create Microsoft Graph client.
                try
                {
                    graphClient = new GraphServiceClient(
                        "https://graph.microsoft.com/v1.0",
                        new DelegateAuthenticationProvider(
                            async (requestMessage) =>
                            {
                                var token = await GetTokenForUserAsync();
                                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                                // This header has been added to identify our sample in the Microsoft Graph service.  If extracting this code for your project please remove.
                                requestMessage.Headers.Add("SampleID", "uwp-csharp-apibrowser-sample");

                            }));
                    return graphClient;
                }

                catch (Exception ex)
                {
                    Debug.WriteLine("Could not create a graph client: " + ex.Message);
                }
            }

            return graphClient;
        }


        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            if (SignedOut || SigninKey == "")
            {
                FormInputDialog dialog = new FormInputDialog("Sign In", "Put in your OneDrive account's Access Key:");
                var result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                {
                    SignedOut = false;
                    SigninKey = dialog.InputText;
                    return dialog.InputText;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    dialog = new FormInputDialog("Sign In", "Please try again. Put your OneDrive account's Access Key:");
                    result = dialog.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(dialog.InputText))
                    {
                        SignedOut = false;
                        SigninKey = dialog.InputText;
                        return dialog.InputText;
                    }
                    else
                    {
                        SignedOut = true;
                        return null;
                    }
                }
                SignedOut = true;
                return null;
            }
            else
            {
                return SigninKey;
            }
        }

        /// <summary>
        /// Signs the user out of the service.
        /// </summary>
        public static void SignOut()
        {
            graphClient = null;
            TokenForUser = null;
        }
    }
}
