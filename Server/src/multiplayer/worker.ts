import { Arena, type Store, type Player, type Room } from './arena';
import { readProfile } from './profile';
interface Env { ARENA: DurableObjectNamespace; ALLOWED_ORIGINS: string; MULTIPLAYER_ENABLED: string; }
interface Attachment { id: string; count: number; window: number; opened: number; }
class SqlStore implements Store {
  constructor(readonly sql: SqlStorage) { sql.exec('CREATE TABLE IF NOT EXISTS records (key TEXT PRIMARY KEY, value TEXT NOT NULL)'); }
  get<T>(key: string): T | undefined { const rows = [...this.sql.exec<{ value: string }>('SELECT value FROM records WHERE key = ?', key)]; return rows[0] ? JSON.parse(rows[0].value) : undefined; }
  put(key: string, value: unknown) { this.sql.exec('INSERT INTO records VALUES (?, ?) ON CONFLICT(key) DO UPDATE SET value=excluded.value', key, JSON.stringify(value)); }
  delete(key: string) { this.sql.exec('DELETE FROM records WHERE key = ?', key); }
  list<T>(prefix: string): T[] { return [...this.sql.exec<{ value: string }>('SELECT value FROM records WHERE key >= ? AND key < ?', prefix, prefix + '\uffff')].map(row => JSON.parse(row.value)); }
}
async function hash(token: string) { return Array.from(new Uint8Array(await crypto.subtle.digest('SHA-256', new TextEncoder().encode(token))), b => b.toString(16).padStart(2, '0')).join(''); }
export class MultiplayerArena {
  store: SqlStore; arena: Arena;
  constructor(readonly state: DurableObjectState) {
    this.store = new SqlStore(state.storage.sql); this.arena = new Arena(this.store);
    state.setWebSocketAutoResponse(new WebSocketRequestResponsePair('ping', 'pong'));
  }
  async fetch(request: Request) {
    if (new URL(request.url).pathname === '/v1/profile') return readProfile(request, this.store);
    if (request.headers.get('Upgrade') !== 'websocket') return new Response('WebSocket required', { status: 426 });
    if (this.state.getWebSockets().length >= 200) return new Response('Capacity reached', { status: 503 });
    const pair = new WebSocketPair(); const client = pair[0], server = pair[1];
    this.state.acceptWebSocket(server);
    server.serializeAttachment({ id: '', count: 0, window: Date.now(), opened: Date.now() } satisfies Attachment);
    await this.schedule(); return new Response(null, { status: 101, webSocket: client });
  }
  async webSocketMessage(socket: WebSocket, message: string | ArrayBuffer) {
    const a = socket.deserializeAttachment() as Attachment;
    try {
      if (typeof message !== 'string' || message.length > 1024) throw new Error('INVALID_REQUEST');
      if (Date.now() - a.window > 60_000) { a.window = Date.now(); a.count = 0; }
      if (++a.count > 90) throw new Error('RATE_LIMIT'); socket.serializeAttachment(a);
      const input = JSON.parse(message);
      if (!input || typeof input !== 'object' || Array.isArray(input)) throw new Error('INVALID_REQUEST');
      if (!a.id) {
        if (input.type !== 'auth' || (input.token && !/^[a-f0-9]{64}$/.test(input.token))) throw new Error('AUTH');
        const token = input.token || [...crypto.getRandomValues(new Uint8Array(32))].map(x => x.toString(16).padStart(2, '0')).join('');
        const key = 'token:' + await hash(token);
        this.state.storage.transactionSync(() => {
          let id = this.store.get<string>(key);
          if (!id) {
            if (input.token) throw new Error('AUTH');
            // Global cap prevents an unauthenticated registration flood consuming unlimited writes.
            const bucket = Math.floor(Date.now() / 3_600_000), rate = this.store.get<{ bucket: number; count: number }>('registrations');
            const count = rate?.bucket === bucket ? rate.count : 0; if (count >= 100) throw new Error('CAPACITY');
            id = this.arena.register().id; this.store.put(key, id); this.store.put('registrations', { bucket, count: count + 1 });
          }
          a.id = id; this.arena.connect(id);
        });
        for (const old of this.state.getWebSockets()) if (old !== socket && old.deserializeAttachment()?.id === a.id) old.close(4001, 'OTHER_SESSION');
        socket.serializeAttachment(a); socket.send(JSON.stringify({ type: 'auth', token }));
      } else {
        const response = this.state.storage.transactionSync(() => { this.arena.tick(); return this.arena.handle(a.id, input); });
        if (response) socket.send(JSON.stringify(response));
      }
      this.broadcast(); await this.schedule();
    } catch (error) {
      const known = ['AUTH','CAPACITY','RATE_LIMIT','INVALID_REQUEST','IN_MATCH','INVITE_EXPIRED','SETTING_CHANGED','NOT_IN_MATCH','SETTING_LOCKED','STALE_MOVE','NOT_YOUR_TURN','INVALID_MOVE','OPPONENT_LEFT'];
      const code = error instanceof Error && known.includes(error.message) ? error.message : 'UNAVAILABLE';
      socket.send(JSON.stringify({ type: 'error', code }));
      if (a.id && code !== 'UNAVAILABLE') socket.send(JSON.stringify(this.arena.snapshot(a.id)));
    }
  }
  broadcast() {
    for (const ws of this.state.getWebSockets()) { const a = ws.deserializeAttachment() as Attachment; if (a?.id) try { ws.send(JSON.stringify(this.arena.snapshot(a.id))); } catch { /* close callback handles departure */ } }
  }
  async webSocketClose(socket: WebSocket) {
    try { socket.close(1000, 'CLOSED'); } catch { /* already closed */ }
    const a = socket.deserializeAttachment() as Attachment;
    if (a?.id && !this.state.getWebSockets().some(ws => ws !== socket && ws.deserializeAttachment()?.id === a.id)) {
      this.state.storage.transactionSync(() => this.arena.disconnect(a.id)); this.broadcast(); await this.schedule();
    }
  }
  async webSocketError(socket: WebSocket) { socket.close(1011, 'CONNECTION'); await this.webSocketClose(socket); }
  async schedule() {
    if (this.store.list<Room>('r:').length || this.store.list<Player>('q:').length || this.state.getWebSockets().some(ws => !ws.deserializeAttachment()?.id)) {
      if (await this.state.storage.getAlarm() === null) await this.state.storage.setAlarm(Date.now() + 10_000);
    }
  }
  async alarm() {
    for (const ws of this.state.getWebSockets()) { const a = ws.deserializeAttachment() as Attachment; if (!a.id && Date.now() - a.opened > 15_000) ws.close(4003, 'AUTH'); }
    this.state.storage.transactionSync(() => this.arena.tick()); this.broadcast(); await this.schedule();
  }
}
export default {
  async fetch(request: Request, env: Env) {
    const origin = request.headers.get('Origin');
    if (origin && !env.ALLOWED_ORIGINS.split(',').includes(origin)) return new Response('Origin denied', { status: 403 });
    const path = new URL(request.url).pathname;
    if (path !== '/v1/play' && path !== '/v1/profile') return new Response('Not found', { status: 404 });
    if (env.MULTIPLAYER_ENABLED !== 'true') return new Response('Unavailable', { status: 503 });
    if (path === '/v1/profile') {
      const headers = { 'Access-Control-Allow-Origin': origin || '*', 'Access-Control-Allow-Methods': 'GET, OPTIONS',
        'Access-Control-Allow-Headers': 'Authorization', 'Cache-Control': 'no-store', 'Vary': 'Origin' };
      if (request.method === 'OPTIONS') return new Response(null, { status: 204, headers });
      if (request.method !== 'GET') return new Response('Method not allowed', { status: 405, headers });
      const response = await env.ARENA.get(env.ARENA.idFromName('permission-to-win-v1')).fetch(request);
      const result = new Response(response.body, response);for (const [key, value] of Object.entries(headers)) result.headers.set(key, value);
      return result;
    }
    return env.ARENA.get(env.ARENA.idFromName('permission-to-win-v1')).fetch(request);
  }
} satisfies ExportedHandler<Env>;
