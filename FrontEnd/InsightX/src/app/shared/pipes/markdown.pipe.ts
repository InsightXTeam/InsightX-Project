import { Pipe, PipeTransform, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { marked } from 'marked';

@Pipe({
  name: 'markdown',
  standalone: true
})
export class MarkdownPipe implements PipeTransform {
  private sanitizer = inject(DomSanitizer);

  transform(value: string): SafeHtml {
    if (!value) return '';
    const parsedHtml = marked.parse(value) as string;
    // Bypassing security trust so that code blocks and markdown styling aren't stripped by Angular.
    // Ensure you only use this with trusted AI responses.
    return this.sanitizer.bypassSecurityTrustHtml(parsedHtml);
  }
}
