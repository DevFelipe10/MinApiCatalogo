using Microsoft.EntityFrameworkCore;
using MinApiCatalogo.Context;
using MinApiCatalogo.Models;

namespace MinApiCatalogo.ApiEndpoints;

public static class ProdutosEndpoints
{
    public static void MapProdutosEndpoints(this WebApplication app)
    {

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
    }
}
