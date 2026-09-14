import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { DashboardService } from '../../core/services/dashboard.service';
import { NewsComponent } from './news.component';

describe('NewsComponent', () => {
  it('opens article details inside HomePanel and closes them again', async () => {
    const article = {
      title: 'Testnyhet',
      summary: 'En sammanfattning för panelen.',
      source: 'SVT',
      publishedAt: '2026-09-14T12:00:00+02:00',
      url: 'https://example.com/news',
    };
    const dashboard = signal({ localNews: [article], nationalNews: [] });

    await TestBed.configureTestingModule({
      imports: [NewsComponent],
      providers: [{ provide: DashboardService, useValue: { data: dashboard.asReadonly() } }],
    }).compileComponents();

    const fixture = TestBed.createComponent(NewsComponent);
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('a.article')).toBeNull();
    (element.querySelector('button.article') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(element.querySelector('[role="dialog"]')).toBeTruthy();

    (element.querySelector('.close-button') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(element.querySelector('[role="dialog"]')).toBeNull();
  });
});
