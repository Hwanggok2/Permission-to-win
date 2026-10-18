export interface Env {
  DB: D1Database;
  ALLOWED_ORIGINS?: string;
}

const API_VERSION = 1;

export default {
  async fetch(request: Request, env: Env): Promise<Response> {
    const origin = request.headers.get('Origin');
    const allowedOrigins = (env.ALLOWED_ORIGINS ?? '').split(',').map(value => value.trim()).filter(Boolean);
    const allowedOrigin = origin !== null && allowedOrigins.includes(origin);
    const headers = new Headers({
      'Content-Type': 'application/json; charset=utf-8',
      'Cache-Control': 'no-store',
      'X-Content-Type-Options': 'nosniff',
      'Vary': 'Origin',
    });
    const respond = (body: object, status = 200) => new Response(JSON.stringify({ apiVersion: API_VERSION, ...body }), { status, headers });

    // CORS only controls browser access. It is not purchase authentication.
    if (origin && !allowedOrigin) return respond({ code: 'ORIGIN_NOT_ALLOWED' }, 403);
    if (allowedOrigin) headers.set('Access-Control-Allow-Origin', origin!);

    const path = new URL(request.url).pathname;
    if (path !== '/healthz' && path !== '/v1/status') return respond({ code: 'NOT_FOUND' }, 404);
    if (request.method === 'OPTIONS') {
      if (!allowedOrigin || request.headers.get('Access-Control-Request-Method') !== 'GET'
          || request.headers.get('Access-Control-Request-Headers')) {
        return respond({ code: 'PREFLIGHT_NOT_ALLOWED' }, 403);
      }
      headers.set('Access-Control-Allow-Methods', 'GET');
      return new Response(null, { status: 204, headers });
    }
    if (request.method !== 'GET') {
      headers.set('Allow', 'GET, OPTIONS');
      return respond({ code: 'METHOD_NOT_ALLOWED' }, 405);
    }
    if (path === '/healthz') return respond({ status: 'ok' });

    try {
      const row = await env.DB.prepare('SELECT schema_version FROM server_metadata WHERE id = 1').first<{ schema_version: number }>();
      if (row?.schema_version !== 1) throw new Error('Schema unavailable');
      return respond({ status: 'ready', purchases: { enabled: false, reason: 'PAYMENTS_NOT_CONFIGURED' } });
    } catch {
      // A D1 limit or outage is not proof of a refund. Never return entitlement=false here.
      headers.set('Retry-After', '60');
      return respond({ status: 'unavailable', purchases: { enabled: false, reason: 'TEMPORARILY_UNAVAILABLE' } }, 503);
    }
  },
} satisfies ExportedHandler<Env>;
