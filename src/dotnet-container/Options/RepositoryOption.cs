using System;
using System.CommandLine;

namespace Dotnet.Container.Options
{
    internal class RepositoryOption
    {
        public static Option Create() => new Option(
                    alias: "--repository",
                    description: "Name of the repository",
                    argument: new Argument<string>
                    {
                        Name = "repository",
                        Arity = ArgumentArity.ExactlyOne
                    });

        public static void EnsureNotNullorMalformed(string? repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }
        }
    }
}
