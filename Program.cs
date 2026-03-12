// ?嚗??函?撘???頝舐蝞∠?閮剖???// 頛詨嚗ppsettings 閮剖??憓??詻TTP 隢???// 頛詨嚗ebApplication ?瑁?蝞∠??楝?梯身摰?// 靘陷嚗SP.NET Core Hosting?iddleware?ession?xceptionHandler??
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.RegularExpressions;
using Web_EIP_Restruct.Helpers;
using Web_EIP_Restruct.Services;
using Web_EIP_Restruct.Services.Dashboard;
using Web_EIP_Restruct.Services.Programs;

var builder = WebApplication.CreateBuilder(args);

DbHelper.Configure(builder.Configuration);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IDemoProgramRepository, DemoProgramRepository>();
builder.Services.AddScoped<IIDMGD01QueryService, IDMGD01QueryService>();
builder.Services.AddScoped<IIDMGD01CommandService, IDMGD01CommandService>();
builder.Services.AddScoped<IDashboardQueryService, DashboardQueryService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    var timeoutHours = builder.Configuration.GetValue<int>("SessionTimeoutHours", 2);
    options.Cookie.Name = ".WebEIP.Session";
    options.IdleTimeout = System.TimeSpan.FromHours(timeoutHours);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerPathFeature>();
        var ex = feature?.Error;

        static (int? lineNumber, string fileName) ParseLine(string? stack)
        {
            if (string.IsNullOrWhiteSpace(stack)) return (null, "");
            var m = Regex.Match(stack, @" in (?<file>.*):line (?<line>\d+)");
            if (!m.Success) return (null, "");
            var file = m.Groups["file"].Value ?? "";
            var lineText = m.Groups["line"].Value ?? "";
            if (!int.TryParse(lineText, out var line)) return (null, file);
            return (line, file);
        }

        var (lineNumber, fileName) = ParseLine(ex?.StackTrace);

        var isApi = context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                    || context.Request.Headers.Accept.Any(h => (h?.Contains("application/json", StringComparison.OrdinalIgnoreCase)).GetValueOrDefault())
                    || string.Equals(context.Request.Headers["X-Requested-With"], "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        if (isApi)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new
            {
                status = "error",
                message = ex?.Message ?? "Unhandled exception",
                lineNumber,
                fileName,
                detail = app.Environment.IsDevelopment() ? ex?.ToString() : ""
            });
            return;
        }

        context.Response.Redirect("/Home/Error");
    });
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers.Append("Cache-Control", "no-store, no-cache, must-revalidate, max-age=0");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "files",
    pattern: "Files/{action=Index}/{id?}",
    defaults: new { controller = "Files" });

app.MapControllerRoute(
    name: "dashboard",
    pattern: "Dashboard/{action=Index}/{id?}",
    defaults: new { controller = "Dashboard" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();


