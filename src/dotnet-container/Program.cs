using Dotnet.Container.CommandHandlers;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace Dotnet.Container
{
    class Program
    {
        public async static Task<int> Main(string[] args) => await new CommandLineBuilder()
                .AddCommand(TestCredentialsCommandHandler.CreateCommand())
                .AddCommand(PushCommandHandler.CreateCommand())
                .AddCommand(BuildManifestCommandHandler.CreateCommand())
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
    }
}
