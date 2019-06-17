using System;
using System.CommandLine;
using System.Runtime.CompilerServices;

namespace Dotnet.Container.Options
{
    internal class UsernameOption
    {
        private static readonly string _environmentUsername = "REGISTRY_USERNAME";
        public static Option Create() => new Option(
                    alias: "--username",
                    description: $"Username (or appId if using a Service Principal) used to connect to the registry. Can also be specified via an environment variable {_environmentUsername}",
                    argument: new Argument<string>
                    {
                        Name = "username",
                        Arity = ArgumentArity.ZeroOrOne
                    });

        public static void EnsureNotNull([EnsuresNotNull] ref string? username)
        {
            if (username == null)
            {
                username = Environment.GetEnvironmentVariable(_environmentUsername);
                if (String.IsNullOrEmpty(username))
                {
                    throw new ArgumentNullException(nameof(username));
                }
            }
        }
    }
}
