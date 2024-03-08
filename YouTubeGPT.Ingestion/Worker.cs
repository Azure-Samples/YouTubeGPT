using Spectre.Console;
using YouTubeGPT.Ingestion.Operations;

namespace YouTubeGPT.Ingestion;

internal class Worker(
    BuildVectorDatabaseOperationHandler buildVectorDatabaseOperationHandler) : IHostedService
{
    private static Operation GetOperation()
    {
        return AnsiConsole.Prompt(
                    new SelectionPrompt<Operation>()
                    .Title("Select an operation to run.")
                    .PageSize(10)
                    .UseConverter(op => op switch
                    {
                        Operation.Build => "Build vector database",
                        Operation.Ask => "Ask a question",
                        Operation.Check => "Check a document status",
                        Operation.Quit => "Quit",
                        _ => throw new InvalidOperationException()
                    })
                    .AddChoices([Operation.Build, Operation.Ask, Operation.Check, Operation.Quit])
                );
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Operation operation = GetOperation();

        while (operation != Operation.Quit && !cancellationToken.IsCancellationRequested)
        {
            await (operation switch
            {
                Operation.Build => buildVectorDatabaseOperationHandler.Handle(),
                _ => throw new NotImplementedException()
            });

            operation = GetOperation();
        }

        AnsiConsole.MarkupLine("[bold]Goodbye[/]");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
