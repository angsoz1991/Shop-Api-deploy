using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Models;
using System;
using Microsoft.AspNetCore.Authorization;

namespace Shop.Controllers {


    [Route("v1/products")]
    public class ProductController : ControllerBase 
    {

        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get(
            [FromServices] DataContext context)
        {
            var products = await context
                .Products
                .Include(x => x.Category)
                .AsNoTracking()
                .ToListAsync();

            return products;
        }

        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id, [FromServices] DataContext context){

            var product = await context
                .Products
                .Include(x => x.Category) 
                .AsNoTracking()
                .FirstOrDefaultAsync (x => x.Id == id);

            return product;
        }

        [HttpGet] // products/categories/1 --> Listas todos os produtos da categoria 1
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GeyByCategory(int id, [FromServices] DataContext context)
        {
            var products = await context
                .Products
                .Include(x => x.Category)
                .AsNoTracking()
                .Where(x => x.CategoryId == id)
                .ToListAsync();

            return products;   

            
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<Product>> Post(
            [FromServices] DataContext context, 
            [FromBody] Product model)
        {
            if(ModelState.IsValid)
            {
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
            }
            else 
            {
                return BadRequest(ModelState);
            }

           
        }


        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<Product>>> Put(
            int id,
            [FromServices] DataContext context,
            [FromBody] Product model)
        {
            //Verifica se o Id informado ?? o mesmo do modelo
            if(id != model.Id)
            {
                return NotFound(new {message = "Produto n??o encontrado"});
            }

            //verifica se o modelo ?? v??lido 
            if(!ModelState.IsValid) 
            {
                return BadRequest(ModelState);
            }

            try
            {
                context.Entry<Product>(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(model);
                
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new {menssage ="Este registro j?? foi atualizado!"});
            }
            catch (Exception)
            {
                return BadRequest(new {menssage ="N??o foi poss??vel atualizar o produto"});
            }
        }
        


        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<Product>>> Delete(
            int id, 
            [FromServices] DataContext context)
        {

            var product = await context.Products.FirstOrDefaultAsync ( x => x.Id == id);
                if(product == null)
                    return NotFound( new {menssage = " Produto n??o encontrado!"});
                
                try
                {
                    context.Products.Remove(product);
                    await context.SaveChangesAsync();
                    return Ok (new {message ="Produto removido com sucesso!"});
                }

                catch (Exception)
                {
                    return BadRequest( new {menssage = "N??o foi possiv??l remover o produto!"});
                }

        }
            
            

    }

}