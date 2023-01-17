using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Compact;

namespace Infrastructure.Configuration.Extensions;

public static class LoggerExtension
{
    public static ILoggingBuilder AddSerilogFile(this ILoggingBuilder loggerBuilder)
    {
        loggerBuilder.AddSerilog(new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(new CompactJsonFormatter(), "Logs/logs.json")
            .CreateLogger());

        return loggerBuilder;
    }

    public static ILoggingBuilder AddSerilogSeq(this ILoggingBuilder loggerBuilder)
    {
        loggerBuilder.AddSerilog(new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.Seq("http://localhost:5341")
            .CreateLogger());

        return loggerBuilder;
    }
}