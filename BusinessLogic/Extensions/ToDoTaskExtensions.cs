using Entities;
using Entities.DTOs;

namespace BusinessLogic.Extensions
{
    public static class ToDoTaskExtensions
    {
        public static ToDoTaskDto ToDto(this ToDoTask toDoTask)
        {
            return new ToDoTaskDto
            {
                Id = toDoTask.Id,
                Title = toDoTask.Title,
                IsDone = toDoTask.IsDone,
                UserId = toDoTask.UserId
            };
        }
    }
}
