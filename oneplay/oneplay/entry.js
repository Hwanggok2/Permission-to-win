// Included only by PackageOnePlay.py; the ordinary Pages/Toss entry never loads this SDK.
const onePlayAssets = new URL('.', document.currentScript.src);
window.stageGamePlatformBoot = (async () => {
  const [{ createSDK }, { createOnePlayHost }, config] = await Promise.all([
    import('https://h5sdk.onestore.net/lib/v1.1.0/onestore-h5-sdk.min.js'),
    import(new URL('host.mjs', onePlayAssets).href),
    fetch(new URL('config.json', onePlayAssets), { cache: 'no-store' }).then(r => {
      if (!r.ok) throw new Error('ONEplay configuration unavailable');
      return r.json();
    })
  ]);
  const host = createOnePlayHost(createSDK(), config, {
    send: (target, value) => window.stageGameInstance?.SendMessage(target, 'OnOnePlayEvent', JSON.stringify(value)),
    applyEnvironment(info) {
      window.stageGameHostLayout?.setNativeInsets(info.safeArea || {});
    }
  });
  window.stageGameOnePlay = host;
  document.addEventListener('visibilitychange', () => host.setVisible(document.visibilityState !== 'hidden'));
  host.setVisible(document.visibilityState !== 'hidden');
  await host.initialize();
})();
