using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace dotnet_oci
{
    public class TestCredentialsCommandHandler
    {
        private static readonly string _environmentUsername = "OCI_USERNAME";
        private static readonly string _environmentPassword = "OCI_PASSWORD";
        public static Command CreateCommand() => new Command(
            name: "test-credentials",
            description: "Test credentials used to connect to OCI registry",
            symbols: new Option[]
            {
                new Option(
                    alias: "--registry",
                    description: "Domain name of the container registry",
                    argument: new Argument<string> { Name = "registry",
                                                     Arity = ArgumentArity.ExactlyOne}),
                new Option(
                    alias: "--username",
                    description: $"Username (or appId if using a Service Principal) used to connect to the registry. Can also be specified via an environment variable {_environmentUsername}",
                    argument: new Argument<string> { Name = "username",
                                                     Arity = ArgumentArity.ZeroOrOne}),
                new Option(
                    alias: "--password",
                    description: $"Password used to connect to the registry. Can also be specified via an environment variable {_environmentPassword}",
                    argument: new Argument<string> { Name = "password",
                                                     Arity = ArgumentArity.ZeroOrOne}),
            },
            handler: CommandHandler.Create<IConsole, string, string, string>(TestCredentialsAsync),
            isHidden: true
            );
        private static async Task TestCredentialsAsync(IConsole console, string registry, string username, string password)
        {
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }
            if (!Uri.IsWellFormedUriString(registry, UriKind.Relative))
            {
                throw new ArgumentException("Not a fully qualified registry URL", nameof(registry));
            }
            if (username == null)
            {
                username = Environment.GetEnvironmentVariable(_environmentUsername);
                if (String.IsNullOrEmpty(username))
                {
                    throw new ArgumentNullException(nameof(username));
                }
            }
            if (password == null)
            {
                password = Environment.GetEnvironmentVariable(_environmentPassword);
                if (String.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password));
                }
            }

            var requestPath = new UriBuilder("https", registry, 443, "v2").Uri;
            var request = new HttpRequestMessage(HttpMethod.Get, requestPath);
            request.AddBasicAuthorizationHeader(username, password);

            var statusCode = (await new HttpClient().SendAsync(request)).StatusCode;

            if (statusCode == HttpStatusCode.Unauthorized)
            {
                console.Out.WriteLine("Invalid credentials");
            }
            if (statusCode == HttpStatusCode.OK)
            {
                return;
            }
            else
            {
                throw new ApplicationException($"Credential test failed with {statusCode}");
            }
        }
    }
}