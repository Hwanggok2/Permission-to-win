'use strict';
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const sourceDir = process.argv[2], output = process.argv[3];
const results = [];
function run(name, fn) {
  try { fn(); results.push({ name, status: 'PASS' }); console.log('PASS ' + name); }
  catch (error) { results.push({ name, status: 'FAIL', error: error.message }); console.log('FAIL ' + name + ' | ' + error.message); }
}
function environment(query = '', os = {}) {
  const styles = {}, events = {}, preview = {};
  const root = { style: { setProperty(k, v) { styles[k] = v; } } };
  const sandbox = {
    URLSearchParams, location: { search: query }, innerWidth: 390, innerHeight: 844,
    document: { documentElement: root, getElementById(id) { assert.equal(id, 'host-preview'); return preview; } },
    getComputedStyle() { return { getPropertyValue(key) { return String(os[key] ?? 0); } }; },
    addEventListener(name, handler) { events[name] = handler; },
    HEAPF32: new Float32Array(16), LibraryManager: { library: {} }, mergeInto(target, value) { Object.assign(target, value); }
  };
  sandbox.window = sandbox;
  vm.createContext(sandbox);
  vm.runInContext(fs.readFileSync(path.join(sourceDir, 'host-layout.js'), 'utf8'), sandbox);
  vm.runInContext(fs.readFileSync(path.join(sourceDir, 'HostInsets.jslib'), 'utf8'), sandbox);
  return { sandbox, styles, events, preview, api: sandbox.stageGameHostLayout };
}
run('bridge.css_fallback_uses_css_units', () => {
  const e = environment('', { '--os-top': 44, '--os-bottom': 34, '--os-left': 10, '--os-right': 6 });
  assert.equal(e.api.read().top, 44); assert.equal(e.styles['--content-top'], '108px'); assert.equal(e.preview.hidden, true);
});
run('bridge.supplied_insets_replace_fallback_and_actual_controls_union', () => {
  const e = environment('', { '--os-top': 44 });
  e.api.setInsets({ top: 20, right: 6, bottom: 10, left: 4 }, 150);
  assert.equal(e.api.read().top, 20); assert.equal(e.styles['--content-top'], '158px');
  e.api.setInsets({ top: 44, right: 0, bottom: 34, left: 0 }, 90);
  assert.equal(e.styles['--content-top'], '108px');
});
run('bridge.negative_nan_and_infinite_javascript_numbers_sanitize', () => {
  const e = environment();
  e.api.setInsets({ top: NaN, right: Infinity, bottom: -5, left: 'bad' }, Infinity);
  for (const edge of ['top', 'right', 'bottom', 'left']) assert.equal(e.api.read()[edge], 0);
  assert.equal(e.api.read().controlsBottom, -1); assert.equal(e.styles['--content-top'], '64px');
});
run('bridge.preview_is_explicit_and_resize_refreshes_fallback', () => {
  const e = environment('?hostPreview=1&hostTop=44&hostBottom=34');
  assert.equal(e.preview.hidden, false); assert.equal(e.api.read().top, 44);
  e.sandbox.innerWidth = 320; e.sandbox.innerHeight = 568; e.events.resize();
  assert.equal(e.api.read().width, 320); assert.equal(e.api.read().height, 568);
  const os = { '--os-top': 10 }; const f = environment('', os);
  os['--os-top'] = 44; f.events.resize(); assert.equal(f.api.read().top, 44);
});
run('bridge.jslib_pointer_order_and_no_dpr_multiplication', () => {
  const e = environment(); e.sandbox.devicePixelRatio = 3;
  e.api.setInsets({ left: 1, top: 44, right: 3, bottom: 34 }, 130);
  e.sandbox.LibraryManager.library.StageGameReadHostInsets(0, 4, 8, 12, 16, 20, 24);
  assert.deepEqual(Array.from(e.sandbox.HEAPF32.slice(0, 7)), [1, 44, 3, 34, 390, 844, 130]);
});
run('bridge.finite_javascript_values_cannot_overflow_float32', () => {
  const e = environment(); e.api.setInsets({ left: 1e300, top: 1e300, right: 1e300, bottom: 1e300 }, 1e300);
  e.sandbox.LibraryManager.library.StageGameReadHostInsets(0, 4, 8, 12, 16, 20, 24);
  assert.ok(Array.from(e.sandbox.HEAPF32.slice(0, 7)).every(Number.isFinite), 'finite JavaScript inputs overflow HEAPF32');
});
const failed = results.filter(r => r.status === 'FAIL').length;
fs.writeFileSync(output, JSON.stringify({ passed: results.length - failed, failed, results, limits: 'Actual JS/jslib executed in Node VM with explicit DOM/event/memory doubles. No browser, Toss SDK, pixel layout or pointer interaction.' }, null, 2));
process.exitCode = failed ? 1 : 0;
