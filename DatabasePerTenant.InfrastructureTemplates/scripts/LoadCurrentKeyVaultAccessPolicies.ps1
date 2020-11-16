param(
   [string][parameter(Mandatory = $true)] $keyVaultName
)

$keyVaultAccessPolicies = (Get-AzKeyVault -VaultName $keyVaultName).accessPolicies
$armAccessPolicies = @()

if($keyVaultAccessPolicies)
{
   foreach($keyVaultAccessPolicy in $keyVaultAccessPolicies)
   {
      $armAccessPolicy = [pscustomobject]@{
         tenantId = $keyVaultAccessPolicy.TenantId
         objectId = $keyVaultAccessPolicy.ObjectId
      }

      $armAccessPolicyPermissions = [pscustomobject]@{
         keys = $keyVaultAccessPolicy.PermissionsToKeys
         secrets = $keyVaultAccessPolicy.PermissionsToSecrets
         certificates = $keyVaultAccessPolicy.PermissionsToCertificates
         storage = $keyVaultAccessPolicy.PermissionsToStorage
     }

     $armAccessPolicy | Add-Member -MemberType NoteProperty -Name permissions -Value $armAccessPolicyPermissions

     $armAccessPolicies += $armAccessPolicy
   }
}

$armAccessPoliciesParameter = [pscustomobject]@{
   list = $armAccessPolicies
}

$armAccessPoliciesParameter = $armAccessPoliciesParameter | ConvertTo-Json -Depth 5 -Compress

Write-Host ("##vso[task.setvariable variable=Infra.KeyVault.AccessPolicies;]$armAccessPoliciesParameter")