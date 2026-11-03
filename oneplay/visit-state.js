// A synchronous visit marker supplements Unity's asynchronous WebGL save flush.
(() => {
  window.stageGameWasVisited = () => {
    try {
      const key = 'stagegame.visited.v1:' + location.pathname.replace(/\/[^/]*$/, '/');
      const previous = localStorage.getItem(key) === '1';
      localStorage.setItem(key, '1');
      return previous;
    } catch (error) { return false; } // PlayerPrefs still handles ordinary storage.
  };
  // A closed page restored from the back/forward cache enters through the normal launch flow.
  addEventListener('pageshow', event => { if (event.persisted) location.reload(); });
})();
