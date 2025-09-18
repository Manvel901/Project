using Diplom.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace Diplom.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BookAutorController:ControllerBase
    {
        private readonly IBookAutor _bookAutor;
        public BookAutorController()
        {
            
        }
        public BookAutorController(IBookAutor bookAutor)
        {
            _bookAutor = bookAutor;
        }


    }
}
