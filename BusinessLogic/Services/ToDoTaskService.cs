

using BusinessLogic.Contracts;
using BusinessLogic.Extensions;
using Data;
using Entities;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;

namespace BusinessLogic.Services
{
    public class ToDoTaskService : IToDoTaskService
    {
        private readonly AppDbContext _appDbContext;
        public ToDoTaskService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<ToDoTaskDto?> AddToDoTaskAsync(ToDoTaskDto toDoTaskDto)
        {
            if (await ToDoTaskExistByTitle(toDoTaskDto))
            {
                return null;
            }

            ToDoTask toDoTask = new()
            {
                Title = toDoTaskDto.Title,
                IsDone = toDoTaskDto.IsDone,
                UserId = toDoTaskDto.UserId
            };

            _appDbContext.Tasks.Add(toDoTask);

            await _appDbContext.SaveChangesAsync();

            return toDoTask.ToDto();
        }

        public async Task<bool> DeleteToDoTaskAsync(int id)
        {
            var toDoTask = await _appDbContext.Tasks.FindAsync(id);

            if (toDoTask == null)
            {
                return false;
            }

            _appDbContext.Tasks.Remove(toDoTask);
            await _appDbContext.SaveChangesAsync();

            return true;
        }

        public async Task<ToDoTaskDto?> GetToDoTaskByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            var toDoTask = await _appDbContext.Tasks
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            return toDoTask?.ToDto();
        }

        public async Task<List<ToDoTaskDto>> GetToDoTasksListAsync(int userId, int skip = 0, int take = 50)
        {
            return await _appDbContext.Tasks
                .Where(t => t.UserId == userId)
                .Skip(skip)
                .Take(take)
                .Select(t => t.ToDto())
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> UpdateToDoTaskAsync(ToDoTaskDto toDoTask)
        {
            var toDoTaskToUpdate = await _appDbContext
                .Tasks
                .FindAsync(toDoTask.Id);

            if (toDoTaskToUpdate is null)
            {
                return false;
            }

            toDoTaskToUpdate.Title = toDoTask.Title;
            toDoTaskToUpdate.IsDone = toDoTask.IsDone;

            await _appDbContext.SaveChangesAsync();

            return true;
        }

        private async Task<bool> ToDoTaskExistByTitle(ToDoTaskDto toDoTaskDto)
        {
            return await _appDbContext.Tasks
                .AsNoTracking()
                .AnyAsync(t => t.Title
                .Equals(toDoTaskDto.Title, StringComparison.CurrentCultureIgnoreCase)
            && t.UserId == toDoTaskDto.UserId);
        }
    }
}
