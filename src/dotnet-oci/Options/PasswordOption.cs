using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Text;

namespace dotnet_oci.Options
{
    internal class PasswordOption
    {
        private static readonly string _environmentPassword = "OCI_PASSWORD";
        public static Option Create() => new Option(
                    alias: "--password",
                    description: $"Password used to connect to the registry. Can also be specified via an environment variable {_environmentPassword}",
                    argument: new Argument<string>
                    {
                        Name = "password",
                        Arity = ArgumentArity.ZeroOrOne
                    });
        public static void EnsureNotNull(ref string? password)
        {
            if (password == null)
            {
                password = Environment.GetEnvironmentVariable(_environmentPassword);
                if (String.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password));
                }
            }
        }
    }
}
