﻿using Dotnet.Container.RegistryClient;
using Dotnet.Container.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Dotnet.Container.CommandHandlers
{
    internal class TestCredentialsCommandHandler
    {
        public static Command CreateCommand() => new Command(
            name: "test-credentials",
            description: "Test credentials used to connect to container registry",
            symbols: new Option[]
            {
                RegistryOption.Create(),
                UsernameOption.Create(),
                PasswordOption.Create(),
            },
            handler: CommandHandler.Create<IConsole, string?, string?, string?>(TestCredentialsAsync),
            isHidden: true
            );
        private static async Task TestCredentialsAsync(IConsole console, string? registry, string? username, string? password)
        {
            try
            {
                RegistryOption.EnsureNotNullorMalformed(registry);
                UsernameOption.EnsureNotNull(ref username);
                PasswordOption.EnsureNotNull(ref password);
            }
            catch (ArgumentException e)
            {
                console.Error.WriteLine($"Push failed due to bad/missing argument:\t{e.ParamName}");
                return;
            }

            var registryUri = new UriBuilder("https", registry).Uri;
            var registryInstance = new Registry(registryUri, username, password);

            try
            {
                _ = await registryInstance.GetApiVersionAsync();
                console.Out.WriteLine("Credentials are valid");

            }
            catch (RegistryException)
            {
                console.Error.WriteLine("Credentials are invalid");
            }
        }
    }
}