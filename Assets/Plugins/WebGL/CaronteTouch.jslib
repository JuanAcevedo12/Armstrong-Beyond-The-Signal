mergeInto(LibraryManager.library, {
 CaronteIsMobileTouch: function() {
  var n = navigator;
  return /Android|iPhone|iPad|iPod|Mobile/i.test(n.userAgent || '') ||
   ((n.maxTouchPoints || 0) > 0 && (window.matchMedia('(pointer: coarse)').matches || /Macintosh/.test(n.userAgent || ''))) ? 1 : 0;
 }
});