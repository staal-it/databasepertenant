# Infrastructure 
Within this templates folder, you will find three subfolders.
- Nested
    - This folder contains all the building blocks we need in our infrastructure. All these files do just one particular thing. We have, for example, a template that creates a KeyVault, one that creates the App Service, and one that creates a SQL server
- Composing
    - This is where we find templates that use our building blocks to come to meaningful infrastructure. You will never find a template in this folder that creates anything itself; that work will always be deferred to a template in the nested folder.
- scripts
    - Folder used for additional PowerShell or Azure CLI scripts for stuff that we cannot do with ARM-templates