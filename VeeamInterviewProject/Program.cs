using VeeamInterviewProject.Startup;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var (sync, cts, log) = await Startup.Initialize(args);

        try
        {
            await sync.Sync(cts.Token);
        }
        catch (TaskCanceledException)
        {
            log.InfoMessage("Application terminated");
        }
    }
}