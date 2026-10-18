import assert from 'node:assert/strict';
const endpoint=process.argv[2]??'ws://127.0.0.1:8788/v1/play';
const remote=process.argv.includes('--public');
const publicEndpoint='wss://permission-to-win-multiplayer-dev.hsj2014528.workers.dev/v1/play';
if(remote?endpoint!==publicEndpoint:!endpoint.startsWith('ws://127.0.0.1:'))throw new Error('Use localhost or the explicit approved public test endpoint with --public');
const peers=[];
const sleep=ms=>new Promise(r=>setTimeout(r,ms));
async function peer(token=''){
 const p={ws:new WebSocket(endpoint),token,state:null,messages:[]};
 peers.push(p);
 p.ws.onmessage=e=>{const m=JSON.parse(e.data);p.messages.push(m);if(m.type==='auth')p.token=m.token;if(m.type==='state')p.state=m;};
 p.send=(type,extra={})=>p.ws.send(JSON.stringify({type,roomId:p.state?.roomId,round:p.state?.round,seq:p.state?.seq,...extra}));
 await new Promise((resolve,reject)=>{p.ws.onopen=resolve;p.ws.onerror=()=>reject(new Error('WebSocket connection failed'));});
 p.ws.send(JSON.stringify({type:'auth',token}));await until(()=>p.state,'authenticate');return p;
}
async function until(fn,label){for(let i=0;i<250;i++){if(fn())return;await sleep(20);}throw new Error('Timeout: '+label);}
const a=await peer(),b=await peer();let tests=0;
try{
 assert.equal(a.state.rating,200);assert.equal(b.state.rating,200);tests++;
 a.send('create',{rated:false});await until(()=>a.state.status==='waiting','create');const invite=a.state.invite;
 b.send('preview',{invite});await until(()=>b.messages.some(m=>m.type==='preview'&&!m.rated),'preview unranked');tests++;
 a.send('setRated',{rated:true});await until(()=>a.state.rated,'toggle');
 b.send('join',{invite,rated:false});await until(()=>b.messages.some(m=>m.code==='SETTING_CHANGED'),'changed setting rejected');tests++;
 b.send('join',{invite,rated:true});await until(()=>a.state.status==='playing'&&b.state.status==='playing','join');tests++;
 const first=a.state.first===a.state.me?a:b,second=first===a?b:a;
 const before=a.state.seq;second.send('move',{column:0});await until(()=>second.messages.some(m=>m.code==='NOT_YOUR_TURN'),'turn validation');assert.equal(a.state.seq,before);tests++;
 for(const column of [0,1,0,1,0,1,0]){
   const current=a.state.turn===a.state.me?a:b,seq=a.state.seq;current.send('move',{column});await until(()=>a.state.seq===seq+1&&b.state.seq===seq+1,'move replication');
 }
 assert.equal(first.state.result,'win');assert.equal(first.state.rating,216);assert.equal(second.state.rating,184);tests++;
 first.send('rematch');await until(()=>second.state.opponentRequested,'rematch notice');assert.equal(second.state.status,'finished');tests++;
 second.send('rematch');await until(()=>a.state.round===2&&b.state.round===2,'mutual rematch');assert.equal(second.state.first,second.state.me);tests++;
 const resumeToken=b.token;b.ws.close();await sleep(150);const resumed=await peer(resumeToken);assert.equal(resumed.state.playerId,b.state.playerId);assert.equal(resumed.state.round,2);tests++;
 resumed.send('leave');await until(()=>a.state.opponentLeft&&resumed.state.status==='idle','departure notice');a.send('leave');await until(()=>a.state.status==='idle','cleanup');tests++;
 a.send('queue');resumed.send('queue');await until(()=>a.state.status==='playing'&&resumed.state.status==='playing','random matchmaking');assert.equal(a.state.rated,true);tests++;
 a.send('leave');await until(()=>resumed.state.opponentLeft,'ranked departure');resumed.send('leave');await until(()=>resumed.state.status==='idle','cleanup ranked');resumed.ws.close();
 console.log(JSON.stringify({passed:tests,transport:remote?'public WSS / Cloudflare Durable Object SQLite':'real WebSocket / local Durable Object SQLite',remoteDeployment:remote}));
}finally{for(const p of peers)p.ws.close();}
