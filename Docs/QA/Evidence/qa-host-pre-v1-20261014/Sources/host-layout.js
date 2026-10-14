// CSS-pixel adapter boundary. No Toss SDK is connected by this file.
// A future adapter must call setInsets(SafeAreaInsets.get()) and forward subscribe() events.
(() => {
  const query = new URLSearchParams(location.search);
  const preview = query.get('hostPreview') === '1';
  const root = document.documentElement;
  let supplied = null, controlsBottom = -1, dirty = true, metrics;
  const number = value => Number.isFinite(Number(value)) ? Math.max(0, Number(value)) : 0;
  if (preview) supplied = { top: number(query.get('hostTop')), bottom: number(query.get('hostBottom')), left: number(query.get('hostLeft')), right: number(query.get('hostRight')) };
  const read = () => {
    if (!dirty && metrics.width === innerWidth && metrics.height === innerHeight) return metrics;
    const style = getComputedStyle(root);
    const inset = edge => supplied ? supplied[edge] : number(parseFloat(style.getPropertyValue('--os-' + edge)));
    metrics = { left: inset('left'), top: inset('top'), right: inset('right'), bottom: inset('bottom'), width: innerWidth, height: innerHeight, controlsBottom };
    root.style.setProperty('--content-top', Math.max(metrics.top + 64, controlsBottom >= 0 ? controlsBottom + 8 : 0) + 'px');
    root.style.setProperty('--host-os-top', metrics.top + 'px');
    root.style.setProperty('--host-os-right', metrics.right + 'px');
    dirty = false;
    return metrics;
  };
  window.stageGameHostLayout = {
    read,
    setInsets(insets, actualControlsBottom = -1) {
      supplied = { left: number(insets.left), top: number(insets.top), right: number(insets.right), bottom: number(insets.bottom) };
      controlsBottom = Number.isFinite(actualControlsBottom) && actualControlsBottom >= 0 ? actualControlsBottom : -1;
      dirty = true; read();
    }
  };
  addEventListener('resize', () => { dirty = true; read(); });
  read();
  document.getElementById('host-preview').hidden = !preview;
})();
