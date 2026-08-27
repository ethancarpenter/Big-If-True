export const API_URL = process.env.NEXT_PUBLIC_API_URL;

export class ApiNotFoundError extends Error {
  constructor() {
    super("Not found");
  }
}

export class ApiConflictError extends Error {
  reason?: string;

  constructor(reason?: string) {
    super("Conflict");
    this.reason = reason;
  }
}

export class ApiUnauthorizedError extends Error {
  constructor() {
    super("Unauthorized");
  }
}

export async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    if (response.status === 404) {
      throw new ApiNotFoundError();
    }
    if (response.status === 409) {
      const reason = await response
        .json()
        .then((body) => (typeof body?.reason === "string" ? body.reason : undefined))
        .catch(() => undefined);
      throw new ApiConflictError(reason);
    }
    if (response.status === 401) {
      // A failed login attempt is an *expected* business outcome (wrong
      // credentials), not a lost session - it must not trigger the global
      // redirect below, or the login page would navigate away before the
      // caller can show "invalid email or password". Every other 401 means
      // an established session died mid-use.
      const isLoginAttempt = response.url.endsWith("/api/auth/login");
      if (!isLoginAttempt && typeof window !== "undefined") {
        // Deliberate full reload, not router.push() - this fires from a
        // plain error-handling path with no router instance available, and
        // a full reload is the right response to a genuinely dead session
        // anyway (clears any stale client-side state, e.g. the cached CSRF
        // token).
        // eslint-disable-next-line @next/next/no-location-assign-relative-destination
        window.location.href = "/login";
      }
      throw new ApiUnauthorizedError();
    }
    const body = await response.text();
    throw new Error(`API request failed (${response.status}): ${body}`);
  }
  if (response.status === 204) {
    return undefined as T;
  }
  return (await response.json()) as T;
}
