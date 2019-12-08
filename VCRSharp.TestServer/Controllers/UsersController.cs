using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace VCRSharp.TestServer.Controllers
{
    [Microsoft.AspNetCore.Mvc.Route("/api/users")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        [Route("{id}", Name = "/api/users/get")]
        public async Task<User> Get(int id)
        {
            await Task.Yield();
            
            return new User
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
        
        public class User
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}