using MudBlazor.Services;
using YoutubeExplode;
using YouTubeGPT.Ingestion;
using YouTubeGPT.Ingestion.Components;
using YouTubeGPT.Ingestion.Operations;
using YouTubeGPT.Shared;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<MetadataDbContext>(ServiceNames.MetadataDB);

builder.AddSemanticKernel();
builder.AddSemanticKernelMemory();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddMudServices();

builder.Services.AddScoped<BuildVectorDatabaseOperationHandler>();

builder.Services.AddScoped<YoutubeClient>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Logger.LogInformation("Ingestion Service is up and running.");

app.Run();
