import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ImageformComponent } from './imageform.component';

describe('ImageformComponent', () => {
  let component: ImageformComponent;
  let fixture: ComponentFixture<ImageformComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ImageformComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ImageformComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
