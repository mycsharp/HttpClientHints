// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpClientHints.AspNetCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Request all high-entropy User-Agent Client Hints; Sec-CH-UA and Sec-CH-UA-Mobile are requested by default
builder.Services.AddHttpClientHints(options =>
{
    options.Platform = true;
    options.Architecture = true;
    options.Device = true;
    options.FullVersionList = true;

    // The page shows the hints, so the browser repeats the first request once with these hints
    options.Critical = ["Sec-CH-UA-Platform-Version", "Sec-CH-UA-Full-Version-List"];
});

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Register Http Client Hints Middleware
app.UseHttpClientHints();

// Optional: provide IHttpClientHintsFeature in HttpContext.Features
app.UseHttpClientHintsFeature();

app.UseRouting();

app.UseAuthorization();

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
