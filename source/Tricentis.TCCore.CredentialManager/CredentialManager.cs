using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Tricentis.TCCore.CredentialManager
{
    public class CredentialManager
    {
        #region Singleton Stuff
        private static volatile CredentialManager _instance;
        private static object syncRoot = new object();

        public static CredentialManager Instance
        {
            get
            {
                //Double locking pattern. Dont delete either of the conditionals
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new CredentialManager();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        private CredentialStore storedCreds;
        private string secretFile;
        //Constructor is thread safe (called within the lock) so create the file here
        public CredentialManager()
        {
            string projectsSource = Environment.ExpandEnvironmentVariables("%TRICENTIS_PROJECTS%");
            secretFile = string.Format($"{projectsSource}\\{Resources.SecretsFile}", System.Security.Principal.WindowsIdentity.GetCurrent().User.Value);
            storedCreds = new CredentialStore();
            if (!File.Exists(secretFile))
            {
                Stream s = File.Create(secretFile);
                s.Dispose();
                FileInfo info = new FileInfo(secretFile);
                var currentAccessControl = info.GetAccessControl();
                currentAccessControl.SetAccessRuleProtection(true, false);//Isolate file from inherrited permissions
                currentAccessControl.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().Name, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
                File.SetAccessControl(secretFile, currentAccessControl);

                List<FileSystemAccessRule> rulesToRemove = new List<FileSystemAccessRule>();
                foreach (FileSystemAccessRule acr in currentAccessControl.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
                {
                    if (acr.IdentityReference.Value == WindowsIdentity.GetCurrent().Name //The current user
                        || acr.IdentityReference.Value == @"NT AUTHORITY\SYSTEM") //The SYSTEM user
                    {
                        continue;
                    }
                    rulesToRemove.Add(acr);
                }
                rulesToRemove.ForEach(fsar => currentAccessControl.RemoveAccessRule(fsar));
                File.SetAccessControl(secretFile, currentAccessControl);
            }
            LoadStoredCredentials(secretFile);
        }
        public void StoreOrUpdateCredential(Credential credential)
        {
            if (!Credentials.Any(x => x.BaseURL == credential.BaseURL))
            {
                StoredCredential storedCred = new StoredCredential
                {
                    entropy = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())
                };
                storedCred.SetCredential(credential);
                storedCreds.StoredCredentials.Add(storedCred);
            }
            else
            {
                StoredCredential oldCredential = storedCreds.StoredCredentials.First(x => x.GetCredential().BaseURL == credential.BaseURL);
                oldCredential.SetCredential(credential);
            }
            SaveCredentials(secretFile);
        }
        public void ClearCredentials()
        {
            storedCreds.StoredCredentials.Clear();
            SaveCredentials(secretFile);
        }
        public void DeleteCredential(string url)
        {
            var storedCredential = storedCreds.StoredCredentials.FirstOrDefault(x => x.GetCredential().BaseURL == url);
            storedCreds.StoredCredentials.Remove(storedCredential);
            SaveCredentials(secretFile);
        }
        public IEnumerable<Credential> Credentials
        {
            get
            {
                return storedCreds.StoredCredentials.Select(x => x.GetCredential());
            }
        }
        private void LoadStoredCredentials(string credentialFile)
        {
            using (Stream stream = File.Open(credentialFile, FileMode.Open))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(CredentialStore));
                try
                {
                    storedCreds = (CredentialStore)formatter.Deserialize(stream);
                }
                catch (Exception)
                {
                    storedCreds = new CredentialStore();
                }
            }
        }
        private void SaveCredentials(string credentialFile)
        {
            using (Stream stream = File.Open(credentialFile,FileMode.Truncate))
            {
                XmlSerializer formatter = new XmlSerializer(typeof(CredentialStore));
                formatter.Serialize(stream, storedCreds);
            }
        }
    }
}
