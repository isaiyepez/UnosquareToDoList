using BusinessLogic.Contracts;
using Entities.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ToDoTasksController : ControllerBase
    {
        private readonly IToDoTaskService _toDoTaskService;
        public ToDoTasksController(IToDoTaskService toDoTaskService)
        {
            _toDoTaskService = toDoTaskService;
        }

        // GET: api/ToDoTasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ToDoTaskDto>>> GetTasksListAsync(
            [FromQuery] int? userId, 
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 20)
        {
            if (!userId.HasValue)
            {
                return BadRequest("UserId query parameter must be provided.");
            }

            if (take < 1 || take > 100) // Optional safeguard on page size
            {
                return BadRequest("Take must be between 1 and 100.");
            }

            var tasksForUser = await _toDoTaskService.GetToDoTasksListAsync(userId.Value, skip, take);

            return Ok(tasksForUser);
        }


        // GET: api/ToDoTasks/5
        [HttpGet("{id}", Name = "GetToDoTaskById")]
        public async Task<ActionResult<ToDoTaskDto>> GetToDoTaskByIdAsync(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be a positive integer.");
            }

            var toDoTask = await _toDoTaskService.GetToDoTaskByIdAsync(id);

            if (toDoTask == null)
            {
                return NotFound();
            }

            return Ok(toDoTask);
        }

        // PUT: api/ToDoTasks/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateToDoTask([FromRoute] int id, [FromBody] ToDoTaskDto toDoTask)
        {
            if (id != toDoTask.Id)
            {
                return BadRequest("Id in URL does not match Id in body.");
            }

            var updateSucceeded = await _toDoTaskService
                .UpdateToDoTaskAsync(toDoTask);

            if (!updateSucceeded)
            {
                return NotFound("Task with given id was not found");
            }

            return NoContent();
        }

        // POST: api/ToDoTasks
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ToDoTaskDto>> PostToDoTask([FromBody] ToDoTaskDto toDoTaskDto)
        {
            var createdToDoTask = await _toDoTaskService.AddToDoTaskAsync(toDoTaskDto);

            if (createdToDoTask == null)
            {
                return BadRequest("A task with the same title already exists for this user.");
            }

            return CreatedAtRoute("GetToDoTaskById", new { id = createdToDoTask.Id }, createdToDoTask);
        }

        // DELETE: api/ToDoTasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteToDoTask(int id)
        {
            if (!await _toDoTaskService.DeleteToDoTaskAsync(id))
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
