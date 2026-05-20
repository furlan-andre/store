using Store.API;
using Store.Application;
using Store.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApi(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApi();

app.Run();

public partial class Program;
