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
for (const [width, height] of [[320,568],[390,664],[390,844],[454,768]]) {
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
const desktop = [];
for (const [width,height,expectedWidth] of [[1280,720,405],[1920,1080,480],[2556,1338,480],[800,1200,480],[1080,1920,480]]) {
  check(`Desktop ${width}x${height} uses a centered portrait canvas`, () => {
    const c=context(width,height),m=c.layout.read();
    assert.equal(m.width,expectedWidth);assert.equal(m.height,height);
    assert.equal(m.x,(width-expectedWidth)/2);assert.equal(m.y,0);
    assert.equal(c.player.style.width,expectedWidth+'px');assert.equal(c.player.style.left,m.x+'px');
    assert.equal(c.layout.read(),m,'Unchanged reads must not trigger another layout');
    desktop.push({viewport:{width,height},player:{width:m.width,height:m.height,x:m.x,y:m.y}});
  });
}
check('Resize from desktop to mobile and back keeps the same adapter and viewport-local coordinates', () => {
  const c=context(1280,720);
  Object.assign(c.visible,{width:390,height:844});c.visibleEvents.resize();
  assert.equal(c.layout.read().width,390);assert.equal(c.player.style.left,'0px');
  Object.assign(c.visible,{width:1920,height:1080,offsetLeft:3,offsetTop:20});c.visibleEvents.resize();
  assert.equal(c.layout.read().width,480);assert.equal(c.player.style.left,'723px');assert.equal(c.player.style.top,'20px');
});
check('Desktop side insets are intersected with the canvas; host top/bottom remain local', () => {
  const c=context(1280,720);c.layout.setInsets({left:450,right:440,top:59,bottom:34},160);
  const m=c.layout.read();assert.equal(m.left,12.5);assert.equal(m.right,2.5);
  assert.equal(m.top,59);assert.equal(m.bottom,34);assert.equal(m.controlsBottom,160);
  assert.equal(c.styles['--content-top'],'168px');
  c.layout.setInsets({left:20,right:20,top:59,bottom:34});
  assert.equal(c.layout.read().left,0);assert.equal(c.layout.read().right,0);
});
check('Desktop bridge sends actual canvas dimensions rather than outer window dimensions', () => {
  const c=context(1280,720);c.sandbox.HEAPF32=new Float32Array(8);
  c.sandbox.LibraryManager={library:{}};c.sandbox.mergeInto=Object.assign;vm.runInContext(bridge,c.sandbox);
  c.layout.setInsets({left:450,right:440,top:59,bottom:34},160);
  c.sandbox.LibraryManager.library.StageGameReadHostInsets(0,4,8,12,16,20,24,28);
  assert.deepEqual(Array.from(c.sandbox.HEAPF32),[12.5,59,2.5,34,405,720,160,1]);
});
check('Rotated mobile preserves a 320px play area and restores full portrait width', () => {
  const c=context(390,844,'?hostPreview=1&hostTop=59&hostBottom=34');
  Object.assign(c.visible,{width:844,height:390});c.visibleEvents.resize();
  assert.equal(c.layout.read().width,320);assert.equal(c.layout.read().height,390);
  assert.equal(c.layout.read().x,262);assert.equal(c.layout.read().top,59);assert.equal(c.layout.read().bottom,34);
  Object.assign(c.visible,{width:390,height:844});c.visibleEvents.resize();
  assert.equal(c.layout.read().width,390);assert.equal(c.layout.read().x,0);
});
const output=path.resolve(process.argv[2]||path.join(root,'Evidence/desktop-centered-20261016/viewport-adapter-tests.json'));
fs.writeFileSync(output,JSON.stringify({kind:'Node VM unit checks; no browser execution',checks,desktop},null,2));
console.log(`${checks.length}/${checks.length} adapter checks passed`);
