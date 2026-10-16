// CSS-pixel adapter boundary. No Toss SDK is connected by this file.
// A future adapter must call setInsets(SafeAreaInsets.get()) and forward subscribe() events.
(() => {
  const query = new URLSearchParams(location.search);
  const preview = query.get('hostPreview') === '1';
  const root = document.documentElement;
  const player = document.getElementById('player');
  let supplied = null, controlsBottom = -1, dirty = true, metrics;
  const float32 = value => Number.isFinite(value) && Number.isFinite(Math.fround(value));
  const number = value => float32(Number(value)) ? Math.max(0, Number(value)) : 0;
  if (preview) supplied = { top: number(query.get('hostTop')), bottom: number(query.get('hostBottom')), left: number(query.get('hostLeft')), right: number(query.get('hostRight')) };
  const read = () => {
    // The browser's bars are outside the visual viewport, not extra game safe-area insets.
    const visible = window.visualViewport;
    const width = number(visible?.width) || innerWidth;
    const height = number(visible?.height) || innerHeight;
    const x = number(visible?.offsetLeft), y = number(visible?.offsetTop);
    // Large windows keep a phone-sized play area, including tall desktop windows.
    // Ordinary portrait phones stay full width; landscape also fits the available height.
    const gameWidth = Math.min(width, 480, width > height ? Math.max(320, height * 9 / 16) : width);
    const sideGap = (width - gameWidth) / 2;
    if (!dirty && metrics.width === gameWidth && metrics.height === height && metrics.x === x + sideGap && metrics.y === y) return metrics;
    if (player) {
      player.style.width = gameWidth + 'px'; player.style.height = height + 'px';
      player.style.left = (x + sideGap) + 'px'; player.style.top = y + 'px';
    }
    const style = getComputedStyle(root);
    const inset = edge => supplied ? supplied[edge] : number(parseFloat(style.getPropertyValue('--os-' + edge)));
    const hostActive = supplied !== null;
    // The bridge reports canvas-local CSS dimensions/insets, never the surrounding page width.
    metrics = { left: Math.max(0, inset('left') - sideGap), top: inset('top'), right: Math.max(0, inset('right') - sideGap), bottom: inset('bottom'), width: gameWidth, height, x: x + sideGap, y, controlsBottom, hostActive };
    root.style.setProperty('--content-top', Math.max(metrics.top + (hostActive ? 64 : 0), controlsBottom >= 0 ? controlsBottom + 8 : 0) + 'px');
    root.style.setProperty('--host-os-top', metrics.top + 'px');
    root.style.setProperty('--host-os-right', metrics.right + 'px');
    dirty = false;
    return metrics;
  };
  window.stageGameHostLayout = {
    read,
    setInsets(insets, actualControlsBottom = -1) {
      supplied = { left: number(insets.left), top: number(insets.top), right: number(insets.right), bottom: number(insets.bottom) };
      controlsBottom = float32(actualControlsBottom) && actualControlsBottom >= 0 ? actualControlsBottom : -1;
      dirty = true; read();
    }
  };
  addEventListener('resize', () => { dirty = true; read(); });
  window.visualViewport?.addEventListener('resize', () => { dirty = true; read(); });
  window.visualViewport?.addEventListener('scroll', () => { dirty = true; read(); });
  read();
  document.getElementById('host-preview').hidden = !preview;
})();
