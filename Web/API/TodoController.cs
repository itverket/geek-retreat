using Microsoft.AspNet.Mvc;
using System.Collections.Generic;
using System.Linq;
using Web.ViewModels;

namespace Web.API
{
    [Route("/api/todo")]
    public class TodoController : Controller
    {
        private static List<TodoItem> _todoItems = new List<TodoItem>
        {
            new TodoItem { Id = 1, Text = "Kjøp Q-Tips" },
            new TodoItem { Id = 2, Text = "Ta en dusj" }
        };

        [HttpGet]
        public IActionResult Get()
        {
            return Json(_todoItems);
        }

        [HttpGet("{id:int}")]
        public IActionResult Get(int id)
        {
            var todoItem = _todoItems.FirstOrDefault(item => item.Id == id);
            if (todoItem == null)
                return HttpNotFound();

            return Json(todoItem);
        }

        [HttpPost]
        public IActionResult Add([FromBody]TodoItem todoItem)
        {
            var nextId = _todoItems.Max(item => item.Id) + 1;
            todoItem.Id = nextId;

            _todoItems.Add(todoItem);

            return Json(todoItem);
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var todoItem = _todoItems.FirstOrDefault(item => item.Id == id);
            if (todoItem == null)
                return HttpNotFound();

            _todoItems.Remove(todoItem);

            return Ok();
        }

        [HttpPut("{id:int}")]
        public IActionResult Edit([FromRoute]int id, [FromBody]TodoItem newTodoItem)
        {
            var todoItem = _todoItems.FirstOrDefault(item => item.Id == id);
            if (todoItem == null)
                return HttpNotFound();

            todoItem.Text = newTodoItem.Text;

            return Json(todoItem);
        }
    }
}
