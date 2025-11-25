using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.DTOs
{
    public class ToDoTaskDto
    {
        public int Id { get; set; }
        public required string Title { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public int UserId { get; set; }
    }
}
