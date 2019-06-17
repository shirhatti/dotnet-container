using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;
using Dotnet.Container.CommandHandlers;

namespace Dotnet.Container
{
    class Program
    {
        public async static Task<int> Main(string[] args) => await new CommandLineBuilder()
                .AddCommand(TestCredentialsCommandHandler.CreateCommand())
                .AddCommand(PushCommandHandler.CreateCommand())
                .AddCommand(BuildManifestCommandHandler.CreateCommand())
                .AddCommand(ImportBaseCommandHandler.CreateCommand())
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
    }
}
