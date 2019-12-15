using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace VCRSharp.TestServer.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("/api/users")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [Route("{id}", Name = "/api/users/get")]
        public async Task<UserDto> Get(int id)
        {
            await Task.Yield();
            
            return new UserDto
            {
                Id = id,
                Name = $"User {id}",
            };
        }

        [Route("~/api/get_users/{id}")]
        public async Task<RedirectToRouteResult> OldGet(int id)
        {
            await Task.Yield();

            return RedirectToRoute("/api/users/get", new {id = id});
        }

        [Route("login")]
        public async Task<ActionResult> Login(string username, string password)
        {
            await Task.Yield();
            
            Response.Cookies.Append("value", "123");

            return Ok();
        }

        [Route("me/info")]
        public async Task<ActionResult> UserInfo()
        {
            await Task.Yield();

            if (Request.Cookies["value"] != "123")
            {
                return StatusCode(401);
            }

            return Ok(new {Info = "Secret Info"});
        }
        
        public class UserDto
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}