using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tricentis.TCCore.CredentialManager
{
    [Serializable]
    public class CredentialStore
    {
        public CredentialStore() { StoredCredentials = new List<StoredCredential>(); }
        public List<StoredCredential> StoredCredentials { get; set; }
    }
}
