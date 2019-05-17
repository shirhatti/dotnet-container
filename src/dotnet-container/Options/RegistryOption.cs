using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Runtime.CompilerServices;
using System.Text;

namespace dotnet_container.Options
{
    internal class RegistryOption
    {
        public static Option Create() => new Option(
                    alias: "--registry",
                    description: "Domain name of the container registry",
                    argument: new Argument<string>
                    {
                        Name = "registry",
                        Arity = ArgumentArity.ExactlyOne
                    });

        public static void EnsureNotNullorMalformed([EnsuresNotNull]string? option)
        {
            if (option == null)
            {
                throw new ArgumentNullException(nameof(option));
            }
            if (!Uri.IsWellFormedUriString(option, UriKind.Relative))
            {
                throw new ArgumentException("Not a fully qualified registry URL", nameof(option));
            }
        }
    }
}
