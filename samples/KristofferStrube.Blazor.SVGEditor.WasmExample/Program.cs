using KristofferStrube.Blazor.FileAPI;
using KristofferStrube.Blazor.SVGEditor.WasmExample;
using KristofferStrube.Blazor.SVGEditor.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// This is used in the Save example
builder.Services.AddURLServiceInProcess();

builder.Services.AddSVGEditor();

await builder.Build().RunAsync();
