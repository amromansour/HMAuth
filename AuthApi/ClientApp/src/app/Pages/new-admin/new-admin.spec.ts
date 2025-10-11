import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NewAdmin } from './new-admin';

describe('NewAdmin', () => {
  let component: NewAdmin;
  let fixture: ComponentFixture<NewAdmin>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NewAdmin]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NewAdmin);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
