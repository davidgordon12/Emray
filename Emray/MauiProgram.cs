using Microsoft.AspNetCore.Components.WebView.Maui;
using Emray.Services;

namespace Emray;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder.UseMauiApp<App>();

		builder.Services.AddMauiBlazorWebView();

		builder.Services.AddScoped<IEncryptionService, EncryptionService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
#endif

		return builder.Build();
	}
}
