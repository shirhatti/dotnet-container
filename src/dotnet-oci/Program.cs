using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace dotnet_oci
{
    class Program
    {
        public async static Task<int> Main(string[] args) => await new CommandLineBuilder()
                .AddCommand(TestCredentialsCommandHandler.CreateCommand())
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
    }
}
