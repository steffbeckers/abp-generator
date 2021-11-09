WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
