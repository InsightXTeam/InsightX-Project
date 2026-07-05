export function extractErrorMessage(err: any, fallback: string): string {
  const body = err?.error;
  if (typeof body === 'string' && body.trim()) return body;
  if (body && typeof body === 'object') {
    return body.error || body.Error || body.message || body.title || fallback;
  }
  if (err?.status === 0) return 'Unable to reach the server. Please check your connection and try again.';
  return fallback;
}
