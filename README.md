# dotnet-container

[![Build Status](https://shirhatti.visualstudio.com/dotnet-container/_apis/build/status/shirhatti.dotnet-container?branchName=master)](https://shirhatti.visualstudio.com/dotnet-container/_build/latest?definitionId=8&branchName=master)
![MyGet (with prereleases)](https://img.shields.io/myget/shirhatti-tools/vpre/dotnet-container.svg)

A .NET global tool for producing and publishing OCI images

## Installing

```sh
dotnet tool install --global dotnet-container --version 0.1.0-buildyyyymmdd.xx --add-source https://www.myget.org/F/shirhatti-tools/api/v3/index.json
```

## Appendix 

### Creating a Service Principal for authentication

> Do **NOT** commit the file containing the Service Principal into source control.

Modify the script in `scripts/generate-sp.sh` and replace `ACR_NAME` with the name of your registry.
Generate a Service Principal (SP) by running the following command:

```sh
./scripts/generate-sp.sh > sp.azureauth
```

If you do not have the `az` cli installed locally, I recommend using the [Azure Cloud Shell](https://shell.azure.com/).
 
