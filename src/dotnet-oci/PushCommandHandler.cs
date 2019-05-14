using dotnet_oci.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace dotnet_oci
{
    internal class PushCommandHandler
    {
        public static Command CreateCommand() => new Command(
            name: "push",
            description: "Invoke dotnet publish and push resulting artifact to registry",
            symbols: new Option[]
            {
                RegistryOption.Create(),
                UsernameOption.Create(),
                PasswordOption.Create(),
                RepositoryOption.Create()
            },
            handler: CommandHandler.Create<IConsole, string?, string?, string?, string?>(PushAsync),
            isHidden: true
            );

        private static void PushAsync(IConsole arg1, string? registry, string? username, string? password, string? repository)
        {
            RegistryOption.EnsureNotNullorMalformed(registry);
            UsernameOption.EnsureNotNull(ref username);
            PasswordOption.EnsureNotNull(ref password);
            RepositoryOption.EnsureNotNullorMalformed(repository);

            throw new NotImplementedException();
        }
    }
}