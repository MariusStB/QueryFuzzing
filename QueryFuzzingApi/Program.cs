using QueryFuzzing.Joern;
using QueryFuzzingApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


var settings = builder.Configuration.GetSection("QueryFuzzingApiSettings").Get<QueryFuzzingApiSettings>();

builder.Services.AddTransient<IJoernService, JoernService>();
builder.Services.AddTransient<IJoernClient, JoernClient>();
builder.Services.AddHttpClient("joernServer", c => c.BaseAddress = new Uri(settings.JoernHost));

builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
