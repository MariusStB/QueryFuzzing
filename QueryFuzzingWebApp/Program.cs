using Microsoft.Extensions.Hosting;
using NLog;
using NLog.Web;
using QueryFuzzing;
using QueryFuzzing.Joern;
using QueryFuzzing.Models;
using QueryFuzzing.Valgrind;
using QueryFuzzing.Windranger;
using QueryFuzzingWebApp.Database;


var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
try
{

       var builder = WebApplication.CreateBuilder(args);
        // Add services to the container.
       var settings = builder.Configuration.GetSection("QueryFuzzingApiSettings").Get<QueryFuzzingApiSettings>();
       
       builder.Services.AddControllersWithViews();
       builder.Services.AddTransient<IJoernService, JoernService>();
       builder.Services.AddTransient<IJoernClient, JoernClient>();
       builder.Services.AddTransient<IQueryFuzzService, WindrangerService>();
       builder.Services.AddTransient<IValgrindService, ValgrindService>();
       builder.Services.AddEntityFrameworkSqlite().AddDbContext<QueryFuzzContext>();
       builder.Services.AddHttpClient("joernServer", c => c.BaseAddress = new Uri(settings.JoernHost));
       
       builder.Logging.ClearProviders();
       builder.Host.UseNLog();
       
       var app = builder.Build();
       
       // Configure the HTTP request pipeline.
       if (!app.Environment.IsDevelopment())
       {
           app.UseExceptionHandler("/QueryFuzz/Error");
           // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
           app.UseHsts();
       }
       
       app.UseHttpsRedirection();
       app.UseStaticFiles();
       
       app.UseRouting();
       
       app.UseAuthorization();
       
       app.MapControllerRoute(
           name: "default",
           pattern: "{controller=QueryFuzz}/{action=Index}/{id?}");
       
       app.Run();
}catch(Exception e)
{
    logger.Error(e, "Stopped programm because of Exception");
}
