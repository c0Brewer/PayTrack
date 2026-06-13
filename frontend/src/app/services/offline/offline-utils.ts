export const OFFLINE_READ_MESSAGE =
  'You are offline. Previously loaded data is shown where available; live data cannot be refreshed.';

export const OFFLINE_WRITE_MESSAGE =
  'You are offline. Actions are unavailable until the connection is restored.';

export function isBrowserOnline(): boolean {
  return typeof navigator === 'undefined' ? true : navigator.onLine;
}

export function ensureOnlineForMutation(): void {
  if (!isBrowserOnline()) {
    throw new Error(OFFLINE_WRITE_MESSAGE);
  }
}

export function withOfflineReadFallback<T>(
  promise: Promise<T>,
  fallbackMessage = 'Unexpected Error',
): Promise<T> {
  return promise.catch((error: unknown) => {
    if (!isBrowserOnline()) {
      throw new Error(OFFLINE_READ_MESSAGE);
    }

    if (error instanceof Error) {
      throw error;
    }

    throw new Error(fallbackMessage);
  });
}
