import { Component, inject, Input, signal, SimpleChanges } from '@angular/core';
import { Register } from "../account/register/register";
import { ToDoTask } from '../../types/todotask';
import { TodotaskService } from '../../core/services/todotask-service';
import { FormsModule } from '@angular/forms';  // ngModel still from FormsModule

@Component({
  selector: 'app-home',
  imports: [Register, FormsModule],  // Remove NgIf, NgFor imports
  templateUrl: './home.html',
  styleUrls: ['./home.css'],
  standalone: true,
})
export class Home {
  @Input() userId = 0;

 private taskService = inject(TodotaskService);

  protected tasksSignal = signal<ToDoTask[]>([]);
  protected isLoading = signal<boolean>(false);
  protected registerMode = signal(false);
  protected newTaskTitle = '';

   ngOnChanges(changes: SimpleChanges) {
    if (changes['userId'] && this.userId > 0) {
      this.loadTasks();
    }
  }

  loadTasks() {
    this.isLoading.set(true);
    this.taskService.getToDoTasks(this.userId).subscribe({
      next: (tasks) => {
        this.tasksSignal.set(tasks);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading tasks', err);
        this.isLoading.set(false);
      }
    });
  }

  showRegister(value: boolean) {
    this.registerMode.set(value);
  }

  addTask() {
    if (!this.newTaskTitle.trim()) return;

    const task: Partial<ToDoTask> = {
      title: this.newTaskTitle.trim(),
      isDone: false,
      userId: this.userId,
    };

    this.taskService.addTask(task).subscribe({
      next: (createdTask) => {
        this.tasksSignal.update(tasks => [...tasks, createdTask]);
        this.newTaskTitle = '';
      },
      error: (err) => console.error('Add task error:', err)
    });
  }

  toggleTaskDone(task: ToDoTask) {
    const updatedTask = { ...task, isDone: !task.isDone };

    this.tasksSignal.update(tasks =>
      tasks.map(t => t.id === task.id ? updatedTask : t)
    );

    this.taskService.updateTask(updatedTask).subscribe({
      next: () => { /* Success */ },
      error: (err) => {
        console.error('Update task error:', err);
        // Revert on error
        this.tasksSignal.update(tasks =>
          tasks.map(t => t.id === task.id ? task : t)
        );
      }
    });
  }

  deleteTask(task: ToDoTask) {
    if(!confirm('Are you sure you want to delete this?')) return;

    this.taskService.deleteTask(task.id).subscribe({
      next: () => {
        this.tasksSignal.update(tasks => tasks.filter(t => t.id !== task.id));
      },
      error: (err) => console.error('Delete task error:', err)
    });
  }
}
