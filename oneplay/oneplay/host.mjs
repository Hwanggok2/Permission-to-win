const SDK_TIMEOUT = 600000;
const delay = ms => new Promise(resolve => setTimeout(resolve, ms));
async function bounded(task, ms) {
  let timer;
  try { return await Promise.race([task, new Promise((_, reject) => {
    timer = setTimeout(() => reject(new Error('timeout')), ms);
  })]); } finally { clearTimeout(timer); }
}

// SDK and transport are injected so cancellation/lifecycle can be tested without real ad impressions.
export function createOnePlayHost(sdk, config, { send, applyEnvironment = () => {}, fetcher = fetch, sleep = delay } = {}) {
  let initialized = false, started = false, paused = false, hidden = false, muted = false, exited = false;
  let target = '', attached = false, active = null, quarantined = false, pendingTicket = null;
  const supported = type => initialized && sdk.ads?.isSupported(type) === true;
  const connected = () => started && !exited && supported('interstitial') && !!config.interstitialPlacementId;
  function emit(value) { if (attached && target) send(target, value); }
  function state() {
    emit({ kind: 'state', ready: connected(), paused: paused || hidden || !started || exited, muted });
  }
  function flush() {
    if (!attached || !target || paused || hidden || !started || exited || !active?.result) return;
    const completed = active;
    active = null;
    if (!completed.cancelled) emit({ kind: 'ad', id: completed.id, result: completed.result });
  }
  function cancel() {
    pendingTicket = null; // Navigation cancels this match's local hint; never carry it into another match.
    if (!active) return;
    active.cancelled = true;
    active.abort.abort();
    // No native SDK cancellation API: retain the lock until the outstanding show resolves.
    if (active.result) active = null;
  }
  sdk.on('pause', () => { paused = true; state(); });
  sdk.on('resume', event => {
    if (typeof event?.ringerSilent === 'boolean') muted = event.ringerSilent;
    paused = false; state(); flush();
  });
  sdk.on('exit', () => { exited = true; cancel(); emit({ kind: 'exit' }); state(); });

  async function api(path, body, operation) {
    const origin = new URL(config.rewardApiOrigin);
    if (origin.protocol !== 'https:') throw new Error('HTTPS reward service required');
    const response = await fetcher(new URL(path, origin), {
      method: 'POST', headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body), signal: operation.abort.signal, credentials: 'omit', cache: 'no-store'
    });
    if (!response.ok) throw new Error('Reward verification unavailable');
    return response.json();
  }
  async function rewardTicket(operation) {
    const ticket = await bounded(api('/v1/hints', {}, operation), 8000);
    if (!/^[0-9a-f-]{36}$/.test(ticket.requestId) || !/^[0-9a-f]{64}$/.test(ticket.claimToken))
      throw new Error('Invalid reward ticket');
    return ticket;
  }
  async function verified(ticket, operation) {
    // Local waiting policy, not a claim about the provider's callback delivery SLA.
    for (let attempt = 0; attempt < 15 && !operation.cancelled; attempt++) {
      const result = await bounded(api('/v1/hints/status', ticket, operation), 2000);
      if (result.status === 'verified' && result.requestId === ticket.requestId) return true;
      if (result.status !== 'pending') { pendingTicket = null; return false; }
      await sleep(1000);
    }
    return false;
  }
  async function show(type, id) {
    const placementId = type === 'rewarded' ? config.rewardedPlacementId : config.interstitialPlacementId;
    if (!started || paused || hidden || exited || active || quarantined || !placementId || !supported(type) ||
        (type === 'rewarded' && !config.rewardApiOrigin)) {
      emit({ kind: 'ad', id, result: 'Unavailable' }); return;
    }
    const operation = { id, cancelled: false, result: '', abort: new AbortController() };
    active = operation;
    const recovering = type === 'rewarded' && !!pendingTicket;
    let result = 'Failed', nativeStarted = false, nativeSettled = false;
    try {
      // Rechecking a delayed SSV for the same match must not make the player watch another ad.
      if (recovering) {
        result = await verified(pendingTicket, operation) ? 'Completed' : 'Failed';
        if (result === 'Completed') pendingTicket = null;
        return;
      }
      if (!await bounded(sdk.ads.isReadyAsync(type, placementId), 8000)) {
        result = 'Unavailable'; return;
      }
      const ticket = type === 'rewarded' ? await rewardTicket(operation) : null;
      if (operation.cancelled || exited || paused || hidden) { result = 'Cancelled'; return; }
      nativeStarted = true;
      const options = { placementId, timeoutMs: SDK_TIMEOUT };
      const answer = await bounded(type === 'rewarded'
        ? sdk.ads.showRewardedAsync({ ...options, requestId: ticket.requestId })
        : sdk.ads.showInterstitialAsync(options), SDK_TIMEOUT + 5000);
      nativeSettled = true;
      if (answer?.reason === 'timeout') quarantined = true;
      if (operation.cancelled) return;
      if (type === 'interstitial') result = answer?.status === 'completed' ? 'Completed' : 'Failed';
      else if (answer?.status === 'dismissed') result = 'Cancelled';
      else if (answer?.status === 'rewarded' && answer.requestId === ticket.requestId) {
        pendingTicket = ticket;
        result = await verified(ticket, operation) ? 'Completed' : 'Failed';
        if (result === 'Completed') pendingTicket = null;
      }
    } catch (error) {
      if (!nativeStarted && !recovering) result = 'Unavailable';
      // A timed-out native request can still show later. Do not issue another ad this page session.
      if (nativeStarted && !nativeSettled && (error?.reason === 'timeout' || error?.message === 'timeout')) quarantined = true;
    } finally {
      operation.abort.abort();
      operation.result = result;
      if (operation.cancelled) active = null;
      else flush();
      if (!quarantined && !exited && !operation.cancelled) preload(type);
    }
  }
  function preload(type) {
    if (!supported(type)) return;
    const placementId = type === 'rewarded' ? config.rewardedPlacementId : config.interstitialPlacementId;
    if (!placementId || (type === 'rewarded' && !config.rewardApiOrigin)) return;
    try {
      const task = type === 'rewarded' ? sdk.ads.loadRewarded({ placementId }) : sdk.ads.loadInterstitial({ placementId });
      Promise.resolve(task).catch(() => {});
    } catch {}
  }
  return {
    async initialize() {
      const info = await bounded(sdk.initializeAsync(), 15000);
      if (!info || info.err) throw new Error('ONEplay platform initialization failed');
      initialized = true; muted = info.ringerSilent === true; applyEnvironment(info);
      sdk.setLoadingProgress(0);
    },
    // Unity Start may run before or after createUnityInstance resolves. Do not call back
    // synchronously while C# is still installing its service after Bind returns.
    bind(name) { target = name; queueMicrotask(state); },
    async attach() {
      attached = true; state();
      sdk.setLoadingProgress(100);
      await bounded(sdk.startGameAsync(), 15000);
      started = true; state(); flush(); preload('interstitial'); preload('rewarded');
    },
    progress(value) { if (initialized) sdk.setLoadingProgress(Math.round(Math.max(0, Math.min(1, value)) * 100)); },
    setVisible(visible) { hidden = !visible; state(); flush(); },
    show, cancel
  };
}
