using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers
{
    [Route("v1/products")]
    public class ProductControllers : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> Get([FromServices]DataContext context)
        {
            var products = await context.Products.Include(x => x.Category).AsNoTracking().ToListAsync();
            return Ok(products);
        }
        [HttpGet]
        [Route("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<Product>> GetById(int id, [FromServices]DataContext context)
        {
            var product = await context.Products.Include(x => x.Category).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
                return NotFound(new { message = "Produto não encontrada"});
            return Ok(product);
        }
        [HttpGet]
        [Route("categories/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<List<Product>>> GetByCategory(int id, [FromServices]DataContext context)
        {
            var products = await context.Products.Include(x => x.Category).AsNoTracking().Where(x => x.CategoryId == id).ToListAsync();
            return Ok(products);
        }

        [HttpPost]
        [Route("")]
        [Authorize(Roles = "employee")]
        public async Task<ActionResult<Product>> Post([FromBody]Product model, [FromServices]DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
        
            try{
                context.Products.Add(model);
                await context.SaveChangesAsync();
                return Ok(model);
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível salvar o Produto" });
            }
        }
    }
}