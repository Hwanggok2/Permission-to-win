mergeInto(LibraryManager.library, {
  StageGameReadHostInsets: function (left, top, right, bottom, width, height, controlsBottom) {
    var value = window.stageGameHostLayout ? window.stageGameHostLayout.read() : { left: 0, top: 0, right: 0, bottom: 0, width: window.innerWidth, height: window.innerHeight, controlsBottom: -1 };
    HEAPF32[left >> 2] = value.left;
    HEAPF32[top >> 2] = value.top;
    HEAPF32[right >> 2] = value.right;
    HEAPF32[bottom >> 2] = value.bottom;
    HEAPF32[width >> 2] = value.width;
    HEAPF32[height >> 2] = value.height;
    HEAPF32[controlsBottom >> 2] = value.controlsBottom;
  }
});
