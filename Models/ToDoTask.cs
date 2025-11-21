namespace Entities
{
    public class ToDoTask
    {
        public int Id { get; set; }
        public required string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
    }
}
