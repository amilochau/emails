using Microsoft.Extensions.Hosting;
using Milochau.Core.Functions.Infrastructure.Hosting;
using Milochau.Core.Infrastructure.Hosting;

namespace Milochau.Emails
{
    public static class Program
    {
        public static void Main()
        {
            CreateHostBuilder().Build().Run();
        }

        public static IHostBuilder CreateHostBuilder() =>
            new HostBuilder()
                .ConfigureCoreConfiguration()
                .ConfigureCoreHostBuilder<Startup>();
    }
}
