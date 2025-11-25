

using Entities.DTOs;

namespace BusinessLogic.Contracts
{
    public interface IToDoTaskService
    {
        Task<List<ToDoTaskDto>> GetToDoTasksListAsync(int userId, int skip = 0, int take = 50);
        Task<ToDoTaskDto?> GetToDoTaskByIdAsync(int id);
        Task<bool> UpdateToDoTaskAsync(ToDoTaskDto toDoTask);
        Task<ToDoTaskDto?> AddToDoTaskAsync(ToDoTaskDto toDoTask);
        Task<bool> DeleteToDoTaskAsync(int id);

    }
}
