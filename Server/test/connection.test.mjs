import test from 'node:test';
import assert from 'node:assert/strict';
import worker from '../src/index.ts';

function env(result = { schema_version: 1 }) {
  let queries = 0;
  return {
    ALLOWED_ORIGINS: 'https://game.example',
    DB: { prepare() { queries++; return { async first() { if (result instanceof Error) throw result; return result; } }; } },
    get queries() { return queries; },
  };
}
const request = (path, options) => new Request(`https://server.example${path}`, options);

test('liveness works during a database outage without querying D1', async () => {
  const bindings = env(new Error('D1 unavailable'));
  const response = await worker.fetch(request('/healthz'), bindings);
  assert.equal(response.status, 200);
  assert.equal(bindings.queries, 0);
});

test('connected D1 never enables an unconfigured payment provider', async () => {
  const bindings = env();
  const response = await worker.fetch(request('/v1/status'), bindings);
  assert.equal(response.status, 200);
  assert.equal(bindings.queries, 1);
  assert.deepEqual(await response.json(), { apiVersion: 1, status: 'ready', purchases: { enabled: false, reason: 'PAYMENTS_NOT_CONFIGURED' } });
  assert.equal(response.headers.get('Cache-Control'), 'no-store');
});

for (const failure of [new Error('D1 daily read limit exceeded: private detail'), new Error('Network unavailable'), null, { schema_version: 2 }]) {
  test(`database failure/schema mismatch closes purchase availability (${String(failure)})`, async () => {
    const response = await worker.fetch(request('/v1/status'), env(failure));
    assert.equal(response.status, 503);
    assert.equal(response.headers.get('Retry-After'), '60');
    const body = await response.json();
    assert.deepEqual(body, { apiVersion: 1, status: 'unavailable', purchases: { enabled: false, reason: 'TEMPORARILY_UNAVAILABLE' } });
    assert.equal('entitlement' in body, false);
    assert.equal('removeAds' in body, false);
    assert.equal(JSON.stringify(body).includes('private detail'), false);
  });
}

test('allowed browser origin receives exact CORS headers including on failure', async () => {
  const response = await worker.fetch(request('/v1/status', { headers: { Origin: 'https://game.example' } }), env(new Error('offline')));
  assert.equal(response.headers.get('Access-Control-Allow-Origin'), 'https://game.example');
  assert.equal(response.status, 503);
});

test('unlisted origin cannot query the database', async () => {
  const bindings = env();
  const response = await worker.fetch(request('/v1/status', { headers: { Origin: 'https://game.example.evil' } }), bindings);
  assert.equal(response.status, 403);
  assert.equal(response.headers.has('Access-Control-Allow-Origin'), false);
  assert.equal(bindings.queries, 0);
});

test('read-only preflight is allowed, purchase POST is not', async () => {
  for (const method of ['GET', 'POST']) {
    const response = await worker.fetch(request('/v1/status', { method: 'OPTIONS', headers: { Origin: 'https://game.example', 'Access-Control-Request-Method': method } }), env());
    assert.equal(response.status, method === 'GET' ? 204 : 403);
  }
});

test('purchase submissions and other writes have no route or database side effects', async () => {
  const bindings = env();
  assert.equal((await worker.fetch(request('/v1/purchases/verify', { method: 'POST', body: '{"paid":true}' }), bindings)).status, 404);
  assert.equal((await worker.fetch(request('/v1/status', { method: 'POST' }), bindings)).status, 405);
  assert.equal(bindings.queries, 0);
});
