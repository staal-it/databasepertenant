﻿using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace DatabasePerTenant.Data.Catalog
{
    public interface IKeyVaultClient
    {
        Task AddSecret(string secretName, string secretValue);
        string GetPasswordKey(int tenantId);
        Task<string> GetSecret(string secretName);
        string GetUsernameKey(int tenantId);
    }

    public class KeyVaultClient : IKeyVaultClient
    {
        private readonly SecretClient Client;

        public KeyVaultClient(IConfiguration configuration)
        {
            var keyVaultUrl = configuration["KeyVault:Endpoint"];
            var cred = new ChainedTokenCredential(new ManagedIdentityCredential(), new AzureCliCredential());
            Client = new SecretClient(new Uri(keyVaultUrl), cred);
        }

        public async Task AddSecret(string secretName, string secretValue)
        {
            var secret = new KeyVaultSecret(secretName, secretValue);

            await Client.SetSecretAsync(secret);
        }

        public async Task<string> GetSecret(string secretName)
        {
            var secret = await Client.GetSecretAsync(secretName);

            return secret.Value.Value;
        }

        public string GetUsernameKey(int tenantId) => $"{tenantId}Username";
        public string GetPasswordKey(int tenantId) => $"{tenantId}Password";
    }
}
