using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tricentis.TCCore.CredentialManager
{
    [Serializable]
    public class StoredCredential
    {
        public byte[] entropy { get; set; }
        public byte[] encryptedText { get; set; }
        /// <summary>
        /// Retrieves a user readable Credential object from the secure credential store
        /// </summary>
        /// <returns></returns>
        public Credential GetCredential()
        {
            byte[] decryptedData = System.Security.Cryptography.ProtectedData.Unprotect(encryptedText, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
            string encodedText = System.Text.Encoding.UTF8.GetString(decryptedData);
            string[] records = encodedText.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            return new Credential
            {
                BaseURL = records[0],
                Description = records[1],
                Password = records[2],
                Username = records[3]
            };
        }

        /// <summary>
        /// Stores the credential in the new user store, sets the encryptedText property to the recieved encrypted bytes
        /// </summary>
        /// <param name="credential"></param>
        public void SetCredential(Credential credential)
        {
            string encodedText = $"{credential.BaseURL}\r\n{credential.Description}\r\n{credential.Password}\r\n{credential.Username}";
            byte[] decryptedData = System.Text.Encoding.UTF8.GetBytes(encodedText);
            this.encryptedText = System.Security.Cryptography.ProtectedData.Protect(decryptedData, entropy, System.Security.Cryptography.DataProtectionScope.CurrentUser);
        }
    }
}
