mergeInto(LibraryManager.library, {
  StageGameSetLanguage: function (pointer) {
    var language = UTF8ToString(pointer) === 'en' ? 'en' : 'ko';
    try { localStorage.setItem('stagegame.language', language); } catch (error) { /* Optional loading-screen preference only. */ }
    document.documentElement.lang = language;
    if (window.stageGameLocalize) window.stageGameLocalize(language);
  }
});
