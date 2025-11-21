import { TestBed } from '@angular/core/testing';

import { TodotaskService } from './todotask-service';

describe('TodotaskService', () => {
  let service: TodotaskService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TodotaskService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
