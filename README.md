# dotnet-container
A .NET global tool for producing and publishing OCI images

### Creating a Service Principal for authentication

> Do **NOT** commit the file containing the Service Principal into source control.

Modify the script in `scripts/generate-sp.sh` and replace `ACR_NAME` with the name of your registry.
Generate a Service Principal (SP) by running the following command:

```sh
./scripts/generate-sp.sh > sp.azureauth
```

If you do not have the `az` cli installed locally, I recommend using the [Azure Cloud Shell](https://shell.azure.com/).
