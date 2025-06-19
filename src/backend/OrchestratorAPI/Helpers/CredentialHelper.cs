using Azure.Identity;

namespace OrchestratorAPI.Helpers
{
    public class CredentialHelper
    {
        private static DefaultAzureCredentialOptions GetDefaultAzureCredentialOptionsImpl(string? clientId, string? tenantId, string environmentName)
        {

            DefaultAzureCredentialOptions credentialOptions = new()
            {
                Diagnostics =
            {
                LoggedHeaderNames = { "x-ms-request-id" },
                LoggedQueryParameters = { "api-version" },
                IsLoggingContentEnabled = true
            },

                //exclude all credential types for local development
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeAzureCliCredential = true,
                ExcludeInteractiveBrowserCredential = true,
                ExcludeEnvironmentCredential = true,
                ExcludeAzureDeveloperCliCredential = true,
                ExcludeAzurePowerShellCredential = true,
                //include all credential types for production
                ExcludeWorkloadIdentityCredential = false,
                ExcludeManagedIdentityCredential = false

            };

            if (clientId is not null)
            {
                credentialOptions.ManagedIdentityClientId = clientId;
                credentialOptions.WorkloadIdentityClientId = clientId;
            }

            if (tenantId is not null)
            {
                credentialOptions.TenantId = tenantId;
            }


            if (environmentName == "Development")
            {
                credentialOptions.ExcludeManagedIdentityCredential = true;
                credentialOptions.ExcludeWorkloadIdentityCredential = true;
                //default to AzureCli or VSCredentials for local development
                credentialOptions.ExcludeAzureCliCredential = false;
                credentialOptions.ExcludeVisualStudioCredential = false;
            }

            return credentialOptions;
        }

        public static DefaultAzureCredentialOptions GetDefaultAzureCredentialOptions(string environmentName)
        {
            return GetDefaultAzureCredentialOptionsImpl(null, null, environmentName);
        }

        public static DefaultAzureCredentialOptions GetDefaultAzureCredentialOptions(string clientId, string environmentName)
        {
            return GetDefaultAzureCredentialOptionsImpl(clientId, null, environmentName);
        }

        public static DefaultAzureCredentialOptions GetDefaultAzureCredentialOptions(string clientId, string tenantId, string environmentName)
        {
            return GetDefaultAzureCredentialOptionsImpl(clientId, tenantId, environmentName);
        }
    }
}
