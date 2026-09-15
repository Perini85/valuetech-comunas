using Microsoft.AspNetCore.Localization;
using Valuetech.WebMvc.Filters;
using Valuetech.WebMvc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options => options.Filters.Add<ApiExceptionFilter>());
builder.Services.AddHttpClient<ValuetechApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"]
        ?? throw new InvalidOperationException("Configura Api:BaseUrl."));
    client.Timeout = TimeSpan.FromSeconds(15);
});

var app = builder.Build();
// Los inputs HTML de tipo number envían decimales con punto.
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en-US", "es-CL"),
    SupportedCultures = [new System.Globalization.CultureInfo("en-US")],
    SupportedUICultures = [new System.Globalization.CultureInfo("es-CL")],
    RequestCultureProviders = []
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Regiones}/{action=Index}/{id?}");

app.Run();
