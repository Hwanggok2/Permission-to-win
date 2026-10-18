import assert from 'node:assert/strict';
const endpoint=process.argv[2]??'ws://127.0.0.1:8788/v1/play';
const remote=process.argv.includes('--public');
if(remote?endpoint!=='wss://permission-to-win-multiplayer-dev.hsj2014528.workers.dev/v1/play':!endpoint.startsWith('ws://127.0.0.1:'))throw Error('Use the explicit approved test endpoint');
const url=endpoint.replace(/^ws/,'http').replace('/v1/play','/v1/profile');
const origin=remote?'https://hwanggok2.github.io':'http://127.0.0.1:8080';
const peers=[],sleep=ms=>new Promise(r=>setTimeout(r,ms));
async function until(fn){for(let i=0;i<250;i++){if(fn())return;await sleep(20);}throw Error('Timed out');}
async function peer(){
 const p={ws:new WebSocket(endpoint),state:null,closed:false,token:''};peers.push(p);
 p.ws.onclose=()=>{p.closed=true;};p.ws.onmessage=e=>{const s=JSON.parse(e.data);if(s.type==='auth')p.token=s.token;if(s.type==='state')p.state=s;};
 await new Promise((resolve,reject)=>{p.ws.onopen=resolve;p.ws.onerror=()=>reject(Error('Socket failed'));});
 p.ws.send(JSON.stringify({type:'auth'}));await until(()=>p.state);
 p.send=(type,data={})=>p.ws.send(JSON.stringify({type,roomId:p.state.roomId,round:p.state.round,seq:p.state.seq,...data}));return p;
}
async function profile(p){const r=await fetch(url,{headers:{Origin:origin,...(p?{Authorization:'Bearer '+p.token}:{})}});assert.equal(r.status,200);assert.equal(r.headers.get('Cache-Control'),'no-store');assert.equal(r.headers.get('Access-Control-Allow-Origin'),origin);return r.json();}
try{
 assert.equal((await profile()).rating,200);
 assert.equal((await fetch(url,{method:'OPTIONS',headers:{Origin:origin,'Access-Control-Request-Headers':'authorization'}})).status,204);
 assert.equal((await fetch(url,{headers:{Origin:'https://invalid.example'}})).status,403);
 assert.equal((await fetch(url,{headers:{Origin:origin,Authorization:'Bearer '+'0'.repeat(64)}})).status,401);
 const a=await peer(),b=await peer();
 a.send('create',{rated:true});await until(()=>a.state.status==='waiting');b.send('join',{invite:a.state.invite,rated:true});await until(()=>a.state.status==='playing'&&b.state.status==='playing');
 for(const p of [a,b]){const r=await profile(p);assert.equal(r.playerId,p.state.playerId);assert.equal(r.rating,200);assert.equal(p.closed,false);}
 for(const column of [0,1,0,1,0,1,0]){const current=a.state.turn===a.state.me?a:b,seq=a.state.seq;current.send('move',{column});await until(()=>a.state.seq===seq+1&&b.state.seq===seq+1);}
 assert.deepEqual([a.state.rating,b.state.rating].sort(),[184,216]);
 for(const p of [a,b]){assert.equal((await profile(p)).rating,p.state.rating);assert.equal(p.closed,false);}
 a.send('leave');b.send('leave');await until(()=>a.state.status==='idle'&&b.state.status==='idle');
 console.log(JSON.stringify({passed:7,anonymous:200,cors:true,invalidIdentityRejected:true,activeSocketsPreserved:true,settledRatings:[184,216],remote}));
}finally{for(const p of peers)p.ws.close();}
