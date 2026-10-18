// All changes run synchronously inside a Durable Object SQLite transaction.
import config from '../../../GameData/Generated/multiplayer.json' with { type: 'json' };
export interface Store { get<T>(key: string): T | undefined; put(key: string, value: unknown): void; delete(key: string): void; list<T>(prefix: string): T[]; }
export interface Player { id: string; rating: number; games: number; room: string; queued: number; disconnected: number; }
export interface Room {
  id: string; invite: string; players: string[]; rated: boolean; friendly: boolean; created: number;
  round: number; seq: number; board: number[]; turn: number; first: number; last: number[];
  status: 'waiting' | 'playing' | 'finished'; winner: number; reason: string;
  requested: boolean[]; left: boolean[]; before: number[]; delta: number[]; deadline: number;
}
export const TURN_MS = config.turnSeconds * 1000;
export const DISCONNECT_MS = config.disconnectSeconds * 1000;
export function rankKey(rating: number): string { return `mp_rank_${Math.min(config.rankCount, Math.max(1, Math.floor(rating / config.rankStep)))}`; }
export function ratingChange(a: number, b: number, score: number): number {
  const expected = 1 / (1 + 10 ** ((b - a) / config.eloScale));
  const raw = config.eloK * (score - expected);
  const rounded = Math.sign(raw) * Math.floor(Math.abs(raw) + .5);
  return score === 1 ? Math.max(1, rounded) : score === 0 ? Math.min(-1, rounded) : rounded;
}
export function searchRange(waitMs: number): number { return Math.min(config.searchMaximum, config.searchStart + Math.floor(Math.max(0, waitMs) / (config.searchStepSeconds * 1000)) * config.searchStep); }
export class Arena {
  constructor(readonly store: Store, readonly now: () => number = Date.now, readonly id: () => string = () => crypto.randomUUID().replaceAll('-', ''), readonly random: () => number = () => crypto.getRandomValues(new Uint32Array(1))[0] / 4294967296) {}
  player(id: string) { const p = this.store.get<Player>('p:' + id); if (!p) throw new Error('AUTH'); return p; }
  save(p: Player) { this.store.put('p:' + p.id, p); }
  room(id: string) { const r = this.store.get<Room>('r:' + id); if (!r) throw new Error('INVITE_EXPIRED'); return r; }
  saveRoom(r: Room) { this.store.put('r:' + r.id, r); }
  register(): Player {
    const count = this.store.get<number>('playerCount') ?? 0;
    if (count >= 10_000) throw new Error('CAPACITY');
    const p = { id: this.id(), rating: config.initialRating, games: 0, room: '', queued: 0, disconnected: 0 };
    this.save(p); this.store.put('playerCount', count + 1); return p;
  }
  connect(id: string) { const p = this.player(id); p.disconnected = 0; this.save(p); }
  disconnect(id: string) { const p = this.player(id); p.disconnected = this.now(); this.cancel(p); this.save(p); }
  cancel(p: Player) { p.queued = 0; this.store.delete('q:' + p.id); this.save(p); }
  private free(p: Player) { if (p.room) throw new Error('IN_MATCH'); }
  create(p: Player, rated: boolean, friendly = true): Room {
    this.free(p); this.cancel(p);
    if (this.store.list<Room>('r:').length >= 200) throw new Error('CAPACITY');
    const r: Room = { id: this.id(), invite: friendly ? this.id() : '', players: [p.id], rated, friendly, created: this.now(), round: 0, seq: 0,
      board: Array(42).fill(0), turn: 0, first: 0, last: [-1, -1], status: 'waiting', winner: -1, reason: '',
      requested: [false, false], left: [false, false], before: [], delta: [0, 0], deadline: this.now() + config.inviteSeconds * 1000 };
    p.room = r.id; this.save(p); this.saveRoom(r);
    if (r.invite) this.store.put('i:' + r.invite, r.id);
    return r;
  }
  preview(invite: string) {
    if (!/^[a-f0-9]{32}$/.test(invite)) throw new Error('INVITE_EXPIRED');
    const id = this.store.get<string>('i:' + invite); if (!id) throw new Error('INVITE_EXPIRED');
    const r = this.room(id); if (r.status !== 'waiting' || r.deadline <= this.now()) throw new Error('INVITE_EXPIRED');
    return r;
  }
  start(r: Room) {
    const a = this.player(r.players[0]), b = this.player(r.players[1]);
    r.before = [a.rating, b.rating];
    r.first = a.rating === b.rating ? (this.random() < .5 ? 0 : 1) : a.rating < b.rating ? 0 : 1;
    r.turn = r.first; r.round++; r.seq = 0; r.board.fill(0); r.last = [-1, -1];
    r.winner = -1; r.reason = ''; r.status = 'playing'; r.requested = [false, false]; r.delta = [0, 0]; r.deadline = this.now() + TURN_MS;
    this.saveRoom(r);
  }
  join(p: Player, r: Room) {
    this.free(p); if (r.players.includes(p.id) || r.status !== 'waiting') throw new Error('IN_MATCH');
    this.cancel(p); r.players.push(p.id); p.room = r.id; this.save(p); this.start(r);
  }
  match() {
    const waiting = this.store.list<Player>('q:').map(p => this.player(p.id)).filter(p => p.queued && !p.disconnected && !p.room).sort((a, b) => a.queued - b.queued);
    const used = new Set<string>();
    for (const a of waiting) {
      if (used.has(a.id)) continue;
      const candidates = waiting.filter(b => b.id !== a.id && !used.has(b.id) && Math.abs(a.rating - b.rating) <= Math.min(searchRange(this.now() - a.queued), searchRange(this.now() - b.queued)))
        .sort((x, y) => Math.abs(a.rating - x.rating) - Math.abs(a.rating - y.rating) || x.queued - y.queued);
      const b = candidates[0]; if (!b) continue;
      const r = this.create(a, true, false); this.join(b, r); used.add(a.id); used.add(b.id);
    }
  }
  finish(r: Room, winner: number, reason: string) {
    if (r.status !== 'playing') return;
    r.status = 'finished'; r.winner = winner; r.reason = reason; r.deadline = this.now() + config.resultSeconds * 1000;
    if (r.rated && reason !== 'server_pause' && reason !== 'both_disconnected') {
      const a = this.player(r.players[0]), b = this.player(r.players[1]);
      let delta = ratingChange(r.before[0], r.before[1], winner < 0 ? .5 : winner === 0 ? 1 : 0);
      // Zero-sum floor at 0. A floor may limit the number of points available to transfer.
      delta = Math.max(config.minimumRating - a.rating, Math.min(b.rating - config.minimumRating, delta));
      a.rating += delta; b.rating -= delta; a.games++; b.games++; r.delta = [delta, -delta]; this.save(a); this.save(b);
    }
    this.saveRoom(r);
  }
  move(r: Room, seat: number, column: number) {
    if (r.status !== 'playing' || r.turn !== seat) throw new Error('NOT_YOUR_TURN');
    if (!Number.isInteger(column) || column < 0 || column > 6) throw new Error('INVALID_MOVE');
    let row = 0; while (row < 6 && r.board[row * 7 + column]) row++;
    if (row === 6) throw new Error('INVALID_MOVE');
    const cell = row * 7 + column, value = seat === 0 ? 1 : -1;
    r.board[cell] = value; r.last[seat] = cell; r.seq++;
    let won = false;
    for (const [dx, dy] of [[1, 0], [0, 1], [1, 1], [1, -1]]) {
      let count = 1;
      for (const direction of [-1, 1]) for (let n = 1; n < 7; n++) {
        const x = column + dx * n * direction, y = row + dy * n * direction;
        if (x < 0 || x > 6 || y < 0 || y > 5 || r.board[y * 7 + x] !== value) break;
        count++;
      }
      if (count >= 4) won = true;
    }
    if (won) this.finish(r, seat, 'four');
    else if (r.seq === 42) this.finish(r, -1, 'draw');
    else { r.turn = 1 - seat; r.deadline = this.now() + TURN_MS; this.saveRoom(r); }
  }
  leave(p: Player) {
    this.cancel(p); if (!p.room) return;
    const r = this.room(p.room), seat = r.players.indexOf(p.id);
    if (r.status === 'playing') this.finish(r, 1 - seat, 'left');
    r.left[seat] = true; p = this.player(p.id); p.room = ''; this.save(p); this.saveRoom(r);
    if (r.players.length === 1 || r.players.every((_, i) => r.left[i])) this.removeRoom(r);
  }
  removeRoom(r: Room) {
    for (const id of r.players) { const p = this.player(id); if (p.room === r.id) { p.room = ''; this.save(p); } }
    this.store.delete('r:' + r.id); if (r.invite) this.store.delete('i:' + r.invite);
  }
  handle(id: string, input: Record<string, unknown>) {
    const p = this.player(id); const type = input.type;
    if (type === 'sync') return;
    if (type === 'queue') { this.free(p); if (!p.queued) p.queued = this.now(); this.save(p); this.store.put('q:' + p.id, p); this.match(); return; }
    if (type === 'cancel') { this.cancel(p); return; }
    if (type === 'create') { this.create(p, input.rated !== false); return; }
    if (type === 'preview') { const r = this.preview(String(input.invite)); return { type: 'preview', invite: r.invite, rated: r.rated, rating: this.player(r.players[0]).rating }; }
    if (type === 'join') { const r = this.preview(String(input.invite)); if (input.rated !== r.rated) throw new Error('SETTING_CHANGED'); this.join(p, r); return; }
    if (type === 'leave') { this.leave(p); return; }
    if (!p.room || input.roomId !== p.room) throw new Error('NOT_IN_MATCH');
    const r = this.room(p.room), seat = r.players.indexOf(id);
    if (type === 'setRated') { if (seat !== 0 || r.status !== 'waiting' || typeof input.rated !== 'boolean') throw new Error('SETTING_LOCKED'); r.rated = input.rated; this.saveRoom(r); return; }
    if (input.round !== r.round) throw new Error('STALE_MOVE');
    if (type === 'move') { if (input.seq !== r.seq) throw new Error('STALE_MOVE'); this.move(r, seat, input.column as number); return; }
    if (type === 'rematch') {
      if (r.status !== 'finished' || r.left.some(Boolean)) throw new Error('OPPONENT_LEFT');
      r.requested[seat] = true; if (r.requested.every(Boolean)) this.start(r); else this.saveRoom(r); return;
    }
    throw new Error('INVALID_REQUEST');
  }
  tick() {
    const now = this.now();
    for (const p of this.store.list<Player>('q:')) if (now - p.queued > config.queueSeconds * 1000) this.cancel(this.player(p.id));
    for (const r of this.store.list<Room>('r:')) {
      if (r.status === 'playing' && now - r.deadline > config.serverPauseSeconds * 1000) { this.finish(r, -1, 'server_pause'); continue; }
      if (r.status === 'playing' && r.players.every(id => { const p = this.player(id); return p.disconnected && now - p.disconnected >= DISCONNECT_MS; })) {
        this.finish(r, -1, 'both_disconnected');
      }
      for (const id of r.players) { const p = this.player(id); if (!r.left[r.players.indexOf(id)] && p.disconnected && now - p.disconnected >= DISCONNECT_MS) this.leave(p); }
      const current = this.store.get<Room>('r:' + r.id); if (!current) continue;
      if (current.deadline <= now) {
        if (current.status === 'playing') this.finish(current, 1 - current.turn, 'timeout'); else this.removeRoom(current);
      }
    }
    this.match();
  }
  snapshot(id: string) {
    const p = this.player(id), r = p.room ? this.room(p.room) : undefined, seat = r?.players.indexOf(id) ?? 0;
    const other = r?.players[1 - seat] ? this.player(r.players[1 - seat]) : undefined;
    return { type: 'state', playerId: p.id, rating: p.rating, rankKey: rankKey(p.rating), status: r?.status ?? (p.queued ? 'queue' : 'idle'),
      roomId: r?.id ?? '', invite: r?.invite ?? '', rated: r?.rated ?? true, round: r?.round ?? 0, seq: r?.seq ?? 0,
      board: r?.board ?? [], me: seat, turn: r?.turn ?? 0, first: r?.first ?? 0, lastOpponentMove: r?.last[1 - seat] ?? -1,
      otherRating: other?.rating ?? 200, otherRankKey: rankKey(other?.rating ?? 200), otherDisconnected: !!other?.disconnected,
      result: r?.status !== 'finished' ? '' : r.winner < 0 ? 'draw' : r.winner === seat ? 'win' : 'loss', reason: r?.reason ?? '',
      delta: r?.delta[seat] ?? 0, rematchRequested: r?.requested[seat] ?? false, opponentRequested: r?.requested[1 - seat] ?? false,
      opponentLeft: r?.left[1 - seat] ?? false, deadline: r?.deadline ?? 0, now: this.now(), searchRange: searchRange(this.now() - p.queued) };
  }
}
