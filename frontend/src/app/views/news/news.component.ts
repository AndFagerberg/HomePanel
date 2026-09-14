import { Component, computed, inject, signal } from '@angular/core';
import { NewsArticleInfo } from '../../core/models/dashboard.model';
import { DashboardService } from '../../core/services/dashboard.service';

@Component({
  selector: 'app-news-view',
  standalone: true,
  templateUrl: './news.component.html',
  styleUrl: './news.component.css',
})
export class NewsComponent {
  private readonly dashboardService = inject(DashboardService);

  readonly nationalNews = computed(() => this.dashboardService.data()?.nationalNews ?? []);
  readonly localNews = computed(() => this.dashboardService.data()?.localNews ?? []);
  readonly selectedArticle = signal<NewsArticleInfo | null>(null);

  openArticle(article: NewsArticleInfo): void {
    this.selectedArticle.set(article);
  }

  closeArticle(): void {
    this.selectedArticle.set(null);
  }

  formattedPublishedAt(publishedAt: string): string {
    return new Intl.DateTimeFormat('sv-SE', {
      day: 'numeric',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit',
    }).format(new Date(publishedAt));
  }
}
