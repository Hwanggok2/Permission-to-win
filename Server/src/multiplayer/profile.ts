import type { Store, Player } from './arena';
import config from '../../../GameData/Generated/multiplayer.json' with { type: 'json' };

// Read-only: no registration, reconnect, matchmaking, socket replacement or storage writes.
export async function readProfile(request: Request, store: Store): Promise<Response> {
  if (request.method !== 'GET') return new Response('Method not allowed', { status: 405 });
  const authorization = request.headers.get('Authorization');
  if (!authorization) return Response.json({ type: 'profile', rating: config.initialRating });
  const match = /^Bearer ([a-f0-9]{64})$/.exec(authorization);
  if (!match) return new Response('Unauthorized', { status: 401 });
  const digest = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(match[1]));
  const hash = Array.from(new Uint8Array(digest), b => b.toString(16).padStart(2, '0')).join('');
  const id = store.get<string>('token:' + hash), player = id ? store.get<Player>('p:' + id) : undefined;
  if (!player) return new Response('Unauthorized', { status: 401 });
  return Response.json({ type: 'profile', playerId: player.id, rating: player.rating });
}
