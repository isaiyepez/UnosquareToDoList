import { Component, inject, OnInit, signal } from '@angular/core';
import { Nav } from "../layout/nav/nav";
import { Home } from "../features/home/home";
import { AccountService } from '../core/services/account-service';
import { ToDoTask } from '../types/todotask';
import { TodotaskService } from '../core/services/todotask-service';

@Component({
  selector: 'app-root',
  imports: [Nav, Home],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected accountService = inject(AccountService)
  private toDoTaskService = inject(TodotaskService)
  protected title = 'Client';
  protected toDoTasks = signal<ToDoTask[]>([]);

async ngOnInit() {
  this.setCurrentUser();

  const user = this.accountService.currentUser();
  if (user) {
    this.toDoTaskService.getToDoTasks(user.id).subscribe({
      next: (tasks) => {
        this.toDoTasks.set(tasks);
        console.log('Tasks received:', tasks);
      },
      error: (error) => console.error('Error fetching tasks:', error)
    });
  }
}


  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;

    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
  }

}

