using Plugin.SecureStorage.Abstractions;
using System.Linq;
using Windows.ApplicationModel;
using Windows.Security.Credentials;

namespace Plugin.SecureStorage
{
    /// <summary>
    /// Android implementation of secure storage. Done using KeyStore
    /// Make sure to initialize store password for Android.
    /// </summary>
    internal class SecureStorage : ISecureStorage
    {
        private string CredentialsResource => Package.Current.Id.Name;

        // Windows password vault
        private readonly PasswordVault Vault;

        /// <summary>
        /// Default constructor created or loads the store
        /// </summary>
        public SecureStorage()
        {
            Vault = new PasswordVault();
        }

        #region ISecureStorage implementation

        /// <summary>
        /// Retrieves the value from storage.
        /// If value with the given key does not exist,
        /// returns default value
        /// </summary>
        /// <returns>The value.</returns>
        /// <param name="key">Key.</param>
        /// <param name="defaultValue">Default value.</param>
        public string GetValue(string key, string defaultValue)
        {
            // try retrieving credential
            var output = GetCredential(key);
            if (output == null)
            {
                return defaultValue;
            }

            output.RetrievePassword();
            return output.Password;
        }

        /// <summary>
        /// Sets the value for the given key. If value exists, overwrites it
        /// Else creates new entry.
        /// Does not accept null value.
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        public bool SetValue(string key, string value)
        {
            // delete previous value
            DeleteKey(key);

            try
            {
                // create entry
                var credential = new PasswordCredential(CredentialsResource, key, value);
                Vault.Add(credential);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Deletes the key and corresponding value from the storage
        /// </summary>
        public bool DeleteKey(string key)
        {
            // retrieve the entry
            var credential = GetCredential(key);
            if (credential == null)
            {
                return false;
            }

            // if entry exists, delete from vault and return true
            try
            {
                Vault.Remove(credential);
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether specified key exists in the storage
        /// </summary>
        public bool HasKey(string key)
        {
            // retrieve to see, if it exists
            return GetCredential(key) != null;
        }

        #endregion

        private PasswordCredential GetCredential(string key)
        {
            var credentials = Vault.RetrieveAll();
            var output = credentials.FirstOrDefault(d => d.UserName == key);
            return output;
        }
    }
}