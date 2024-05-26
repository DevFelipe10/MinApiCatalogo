using Microsoft.EntityFrameworkCore;
using MinApiCatalogo.Context;
using MinApiCatalogo.Models;

namespace MinApiCatalogo.ApiEndpoints;

public static class CategoriasEndpoints
{
    public static void MapCategoriasEndpoints(this WebApplication app)
    {
        // endpoint para categorias
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
    }
}
