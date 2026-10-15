// Pure adapter/ABI checks. This is not browser or iOS execution evidence.
const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const path = require('node:path');
const root = path.resolve(__dirname, '..');
const adapter = fs.readFileSync(path.join(root, 'Assets/WebGLTemplates/StageGamePortrait/host-layout.js'), 'utf8');
const bridge = fs.readFileSync(path.join(root, 'Assets/StageGame/Plugins/WebGL/HostInsets.jslib'), 'utf8');
const checks = [];
function context(width, height, query = '', viewport = true) {
  const styles = {}, player = {style:{}}, preview = {}, events = {}, visibleEvents = {};
  const visible = {width, height, offsetLeft:0, offsetTop:0, addEventListener:(event, fn) => visibleEvents[event] = fn};
  const sandbox = {URLSearchParams, location:{search:query}, innerWidth:width, innerHeight:height,
    document:{documentElement:{style:{setProperty:(key, value) => styles[key] = value}},
      getElementById:id => id === 'player' ? player : preview},
    getComputedStyle:() => ({getPropertyValue:() => '0px'}),
    addEventListener:(event, fn) => events[event] = fn,
    visualViewport:viewport ? visible : null};
  sandbox.window = sandbox;
  vm.createContext(sandbox); vm.runInContext(adapter, sandbox);
  return {sandbox, styles, player, visible, events, visibleEvents, layout:sandbox.stageGameHostLayout};
}
function check(name, test) { test(); checks.push({name, passed:true}); }
for (const [width, height] of [[390,664],[390,844],[454,768],[1080,1920]]) {
  check(`Plain viewport ${width}x${height} has no fictional host inset`, () => {
    const c = context(width,height); const m = c.layout.read();
    assert.equal(m.width,width);assert.equal(m.height,height);assert.equal(m.hostActive,false);
    assert.equal(c.styles['--content-top'],'0px');assert.equal(c.player.style.height,height+'px');
  });
}
check('Visual viewport overrides layout viewport, including toolbar resize and offsets', () => {
  const c=context(390,844);c.visible.height=664;c.visible.offsetTop=20;c.visible.offsetLeft=3;
  c.visibleEvents.resize();assert.equal(c.layout.read().height,664);
  assert.equal(c.player.style.top,'20px');assert.equal(c.player.style.left,'3px');
  c.visible.offsetTop=0;c.visibleEvents.scroll();assert.equal(c.player.style.top,'0px');
  assert.equal(c.styles['--content-top'],'0px');
});
check('Inner dimensions remain a fallback without visualViewport', () => {
  const c=context(454,768,'',false);assert.equal(c.layout.read().height,768);
  c.sandbox.innerHeight=700;c.events.resize();assert.equal(c.player.style.height,'700px');
});
check('Explicit SDK host retains its controls allowance and actual controls bounds', () => {
  const c=context(390,844);c.layout.setInsets({top:59,bottom:34,left:0,right:0});
  assert.equal(c.layout.read().hostActive,true);assert.equal(c.styles['--content-top'],'123px');
  c.layout.setInsets({top:59,bottom:34,left:0,right:0},160);
  assert.equal(c.styles['--content-top'],'168px');assert.equal(c.layout.read().bottom,34);
});
check('Host preview is explicit', () => {
  const c=context(390,844,'?hostPreview=1&hostTop=59&hostBottom=34');
  assert.equal(c.layout.read().hostActive,true);assert.equal(c.styles['--content-top'],'123px');
});
check('Invalid inputs cannot inject nonfinite numbers into Unity', () => {
  const c=context(390,664);c.layout.setInsets({top:NaN,bottom:Infinity,left:-4,right:1e100},NaN);
  for(const edge of ['top','bottom','left','right'])assert.equal(c.layout.read()[edge],0);
  assert.equal(c.layout.read().controlsBottom,-1);
});
check('Eight-float WebGL ABI preserves viewport and host activation', () => {
  const c=context(390,664);c.sandbox.HEAPF32=new Float32Array(16);
  c.sandbox.LibraryManager={library:{}};c.sandbox.mergeInto=Object.assign;
  vm.runInContext(bridge,c.sandbox);
  const read=()=>{c.sandbox.LibraryManager.library.StageGameReadHostInsets(0,4,8,12,16,20,24,28);return Array.from(c.sandbox.HEAPF32.slice(0,8));};
  assert.deepEqual(read(),[0,0,0,0,390,664,-1,0]);
  c.layout.setInsets({left:1,top:59,right:2,bottom:34},160);
  assert.deepEqual(read(),[1,59,2,34,390,664,160,1]);
  vm.runInContext('window.stageGameHostLayout = null',c.sandbox);
  assert.deepEqual(read(),[0,0,0,0,390,664,-1,0]);
});
const output=path.join(root,'Evidence/web-audio-fsb-20261016/viewport-adapter-tests.json');
fs.writeFileSync(output,JSON.stringify({kind:'Node VM unit checks; no browser execution',checks},null,2));
console.log(`${checks.length}/${checks.length} adapter checks passed`);
