using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinApiCatalogo.Context;
using MinApiCatalogo.Models;
using MinApiCatalogo.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "minapicatalogo", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

string connectionsString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseMySql(connectionsString,
                                    ServerVersion.AutoDetect(connectionsString)));

// Token JWT
builder.Services.AddSingleton<ITokenService>(new TokenService());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                            Encoding.UTF8
                                            .GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// endpoint para login
app.MapPost("/login", [AllowAnonymous] (UserModel user, ITokenService tokenService) =>
{
    if (user == null)
    {
        return Results.BadRequest("Login Inválido");
    }

    if (user.UserName == "felipe" && user.Password == "teste123")
    {
        var tokenString = tokenService.GerarToken(app.Configuration["Jwt:key"],
            app.Configuration["Jwt:Issuer"],
            app.Configuration["Jwt:Audience"],
            user);

        return Results.Ok(new { token = tokenString });
    }

    return Results.BadRequest("Login Inválido");
}).Produces(StatusCodes.Status400BadRequest)
  .Produces(StatusCodes.Status200OK)
  .WithName("Login")
  .WithTags("Autenticação");


// endpoint para produtos
app.MapGet("/", () => "Api de Catálogos -- 2024").ExcludeFromDescription();

app.MapPost("/categorias", async (Categoria categoria, AppDbContext db) =>
{
    db.Categorias?.Add(categoria);
    await db.SaveChangesAsync();

    return Results.Created($"/Categorias/{categoria.CategoriaId}", categoria);
}).RequireAuthorization()
  .WithTags("Categorias");

//Método verboso

//app.MapPost("/categoria", async ([FromBody] Categoria categoria, [FromServices] AppDbContext db) =>
//{
//    db.Categorias?.Add(categoria);
//    await db.SaveChangesAsync();

//    return Results.Created($"/Categorias/{categoria.CategoriaId}", categoria);
//}).Accepts<Categoria>("application/json")
//  .Produces<Categoria>(StatusCodes.Status201Created)
//  .WithName("CriarNovaCategoria")
//  .WithTags("Setter");

app.MapGet("/categorias", async (AppDbContext db) => await db.Categorias!.ToListAsync())
    .RequireAuthorization()
    .WithTags("Categorias");

app.MapGet("/categorias/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Categorias!.FindAsync(id)
                is Categoria categoria
                ? Results.Ok(categoria)
                : Results.NotFound();
}).RequireAuthorization()
  .WithTags("Categorias");

app.MapPut("/categorias/{id:int}", async (int id, Categoria categoria, AppDbContext db) =>
{
    if (id != categoria.CategoriaId)
        return Results.BadRequest("Id de atualização é diferente");

    Categoria? categoriaDb = await db.Categorias!.FindAsync(id);

    if (categoriaDb is null)
        return Results.NotFound($"Categoria id={id} não encontrada");

    categoriaDb!.Nome = categoria.Nome;
    categoriaDb!.Descricao = categoria.Descricao;

    //db.Categorias!.Update(categoria);
    await db.SaveChangesAsync();

    return Results.Ok($"Categoria id={id} atualizada com sucesso");
}).RequireAuthorization()
  .WithTags("Categorias");

app.MapDelete("/categorias/{id:int}", async (int id, AppDbContext db) =>
{
    Categoria? categoria = await db.Categorias!.FindAsync(id);

    if (categoria is null)
        return Results.NotFound($"Categoria id={id} não encontrada");

    db.Categorias.Remove(categoria);
    await db.SaveChangesAsync();

    return Results.Ok(categoria);
}).RequireAuthorization()
  .WithTags("Categorias");

app.MapPost("/produtos", async (Produto produto, AppDbContext db) =>
{
    db.Produtos!.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/Categorias/{produto.ProdutoId}", produto);
}).RequireAuthorization()
  .WithTags("Produtos");

app.MapGet("/produtos", async (AppDbContext db) => await db.Produtos!.ToListAsync())
    .RequireAuthorization()
    .WithTags("Produtos");

app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) =>
{
    return await db.Produtos!.FindAsync(id)
                        is Produto produto
                        ? Results.Ok(produto)
                        : Results.NotFound();
}).RequireAuthorization()
  .WithTags("Produtos");

app.MapPut("/produtos/{id:int}", async (int id, Produto produto, AppDbContext db) =>
{
    if (produto.ProdutoId != id)
        return Results.BadRequest("Id de atualização é diferente");

    Produto? produtoDb = await db.Produtos!.FindAsync(id);

    if (produtoDb is null)
        return Results.NotFound($"Produto id={id} não encontrado");

    produtoDb.Nome = produto.Nome;
    produtoDb.Descricao = produto.Descricao;
    produtoDb.Estoque = produto.Estoque;
    produtoDb.Preco = produto.Preco;
    produtoDb.Imagem = produto.Imagem;
    produtoDb.DataCompra = produto.DataCompra;
    produtoDb.Estoque = produto.Estoque;
    produtoDb.CategoriaId = produto.CategoriaId;

    await db.SaveChangesAsync();

    return Results.Ok(produtoDb);
}).RequireAuthorization()
  .WithTags("Produtos");

app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) =>
{
    Produto? produto = await db.Produtos!.FindAsync(id);

    if (produto is null)
        return Results.NotFound($"Produtos id={id} não encontrado");

    db.Remove(produto);
    await db.SaveChangesAsync();

    return Results.Ok(produto);
}).RequireAuthorization()
  .WithTags("Produtos");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();