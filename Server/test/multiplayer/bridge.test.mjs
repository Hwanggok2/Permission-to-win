import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import vm from 'node:vm';
const source=fs.readFileSync(new URL('../../../Client/Assets/StageGame/Plugins/WebGL/MultiplayerBridge.jslib',import.meta.url),'utf8');
function bridge(navigator={}){
 const messages=[],context={LibraryManager:{library:{}},UTF8ToString:s=>s,navigator,SendMessage:(...args)=>messages.push(args),setInterval:()=>1,clearInterval:()=>{},mergeInto:Object.assign};
 vm.createContext(context);vm.runInContext(source,context);context.PTW=context.LibraryManager.library.$PTW;return {context,lib:context.LibraryManager.library,messages};
}
test('native web share receives only invite URL and localized title',async()=>{let shared;const b=bridge({share:async x=>{shared=x;}});b.lib.PTWShare('https://game/#invite=abc','멀티 배틀','RankAppRoot',0);await Promise.resolve();assert.equal(shared.url,'https://game/#invite=abc');assert.equal(shared.title,'멀티 배틀');assert.equal(b.messages[0][2],'shared');});
test('copy button bypasses share and reports successful clipboard copy',async()=>{let copied;const b=bridge({clipboard:{writeText:async x=>{copied=x;}}});b.lib.PTWShare('https://game/#invite=abc','title','RankAppRoot',1);await Promise.resolve();assert.equal(copied,'https://game/#invite=abc');assert.equal(b.messages[0][2],'copied');});
test('share cancellation does not send or copy anything',async()=>{const b=bridge({share:async()=>{throw {name:'AbortError'};}});b.lib.PTWShare('url','title','target',0);await Promise.resolve();assert.equal(b.messages.length,0);});
test('clipboard denial reports manual-copy fallback instead of false success',async()=>{const b=bridge({clipboard:{writeText:async()=>{throw Error('Denied');}}});b.lib.PTWShare('url','title','target',1);await Promise.resolve();assert.equal(b.messages[0][2],'failed');});
test('replaced or closed sockets cannot deliver stale game messages',()=>{const b=bridge();b.context.WebSocket=class{constructor(){this.readyState=1;}close(){this.closed=true;}send(x){this.sent=x;}};b.lib.PTWConnect('ws://local','RankAppRoot');const old=b.context.PTW.socket;b.lib.PTWClose();old.onmessage({data:'old'});assert.equal(b.messages.length,0);b.lib.PTWConnect('ws://local','RankAppRoot');const fresh=b.context.PTW.socket;fresh.onopen();fresh.onmessage({data:'new'});assert.deepEqual(b.messages.map(x=>x[1]),['OnNetworkOpen','OnNetworkData']);b.lib.PTWClose();});
