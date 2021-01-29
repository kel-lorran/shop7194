using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Shop.Services;

namespace Shop.Controllers
{
    [Route("v1/users")]
    public class UserController : Controller
    {
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<List<User>>> Get([FromServices]DataContext context)
        {
            var users = await context.Users.AsNoTracking().ToListAsync();
            return Ok(users);
        }
        [HttpPost]
        [Route("")]
        [AllowAnonymous]
        // [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Post([FromBody]User model, [FromServices]DataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.Role = "employee";
            try
            {
                context.Users.Add(model);
                await context.SaveChangesAsync();

                model.Password = "";
                return model;
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível criar o usuário" });
            }
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize(Roles = "manager")]
        public async Task<ActionResult<User>> Put(int id, [FromBody]User model, [FromServices]DataContext context)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);
            
            if(id != model.Id)
                return NotFound(new { message = "Usuário não encontrado" });

            try
            {
                context.Entry(model).State = EntityState.Modified;
                await context.SaveChangesAsync();
                return model;
            }
            catch
            {
                return BadRequest(new { message = "Não foi possível atualizar o Usuário" });
            }
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<ActionResult<dynamic>> Authenticate([FromBody]User model, [FromServices]DataContext context)
        {
            var user = await context.Users.AsNoTracking().Where(x => x.Username == model.Username && x.Password == model.Password).FirstOrDefaultAsync();
            if(user == null)
                return NotFound(new { message = "Usuário ou senha inválido" });
            
            var token = TokenServices.GenerateToken(user);

            user.Password = "";
            return new
            {
                user = user,
                token = token
            };
        }
    }
}