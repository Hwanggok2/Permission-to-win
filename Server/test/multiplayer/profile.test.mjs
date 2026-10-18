import test from 'node:test';
import assert from 'node:assert/strict';
import { readProfile } from '../../src/multiplayer/profile.ts';

test('anonymous rating preview has no account or storage writes',async()=>{
 const response=await readProfile(new Request('https://game/v1/profile'),{get(){throw Error('unexpected access');}});
 assert.deepEqual(await response.json(),{type:'profile',rating:200});
});
test('profile only reads the authenticated rating without changing an active player',async()=>{
 const token='a'.repeat(64),hash=Buffer.from(await crypto.subtle.digest('SHA-256',new TextEncoder().encode(token))).toString('hex');
 const player={id:'p1',rating:416,room:'active',queued:12,disconnected:0};const before=JSON.stringify(player);
 const reads=[];const store={get(key){reads.push(key);return key==='token:'+hash?'p1':key==='p:p1'?player:undefined;},put(){throw Error('write');},delete(){throw Error('delete');}};
 const result=await readProfile(new Request('https://game/v1/profile',{headers:{Authorization:'Bearer '+token}}),store);
 assert.deepEqual(await result.json(),{type:'profile',playerId:'p1',rating:416});assert.equal(JSON.stringify(player),before);assert.equal(reads.length,2);
});
test('bad or unknown identities cannot read a profile and wrong methods do not register',async()=>{
 for(const auth of ['Bearer bad','Basic abc','Bearer '+'b'.repeat(64)]){
  const result=await readProfile(new Request('https://game/v1/profile',{headers:{Authorization:auth}}),{get(){return undefined;}});
  assert.equal(result.status,401);
 }
 assert.equal((await readProfile(new Request('https://game/v1/profile',{method:'POST'}),{})).status,405);
});
