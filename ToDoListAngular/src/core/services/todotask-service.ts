import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ToDoTask } from '../../types/todotask';

@Injectable({
  providedIn: 'root',
})
export class TodotaskService {
  private http = inject(HttpClient);


  baseUrl = 'https://localhost:5001/api/ToDoTasks';

  getToDoTasks(userId?: number): Observable<ToDoTask[]> {
    let params = new HttpParams();


    if (userId) {
      params = params.set('userId', userId);
    }

    return this.http.get<ToDoTask[]>(this.baseUrl, { params });
  }


  addTask(task: Partial<ToDoTask>): Observable<ToDoTask> {
    console.log(task);
    return this.http.post<ToDoTask>(this.baseUrl, task);
  }

  updateTask(task: ToDoTask): Observable<void> {

    return this.http.put<void>(`${this.baseUrl}/${task.id}`, task);
  }

  deleteTask(id: number): Observable<void> {

    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}
