using MinApiCatalogo.ApiEndpoints;
using MinApiCatalogo.AppServicesExtensions;

var builder = WebApplication.CreateBuilder(args);

builder.AddApiSwagger();
builder.AddPersistece();
builder.Services.AddCors();
builder.AddAutenticationJwt();

var app = builder.Build();

app.MapAutenticacaoEndpoints();
app.MapCategoriasEndpoints();
app.MapProdutosEndpoints();

/// Configure the HTTP request pipeline.
var environment = app.Environment;

app.UseExceptionHandling(environment)
    .UseSwaggerMiddleware()
    .UseAppCors();

//if (app.Environment.IsDevelopment())
//{
//}

app.UseAuthentication();
app.UseAuthorization();

app.Run();