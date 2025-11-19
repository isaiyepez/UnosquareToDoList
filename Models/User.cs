namespace Entities
{
    public class User
    {
        public int Id { get; set; }
        public required string DisplayName { get; set; }
        public required string Email { get; set; } = string.Empty;
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }
        public ICollection<ToDoTask> Tasks { get; set; }
    }
}
