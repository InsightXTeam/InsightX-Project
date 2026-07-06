import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ReportUpload } from './report-upload';

describe('ReportUpload', () => {
  let component: ReportUpload;
  let fixture: ComponentFixture<ReportUpload>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ReportUpload],
    }).compileComponents();

    fixture = TestBed.createComponent(ReportUpload);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
