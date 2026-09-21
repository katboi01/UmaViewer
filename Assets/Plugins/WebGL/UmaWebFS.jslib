// Bridges the managed WebFileMount to the folder the user picked in the template
// (window.__umaFS, a File System Access API directory handle). Reads are async, so the
// managed side starts a mount and polls until the bytes land in Emscripten's MEMFS at the
// same path the synchronous C#/sqlite/AssetBundle code later opens.
mergeInto(LibraryManager.library, {
  $UmaFSState: { reqs: {}, nextId: 1 },

  UmaFS_IsMounted: function (pathPtr) {
    var path = UTF8ToString(pathPtr);
    try { return FS.analyzePath(path).exists ? 1 : 0; } catch (e) { return 0; }
  },

  UmaFS_BeginMount__deps: ['$UmaFSState'],
  UmaFS_BeginMount: function (pathPtr) {
    var path = UTF8ToString(pathPtr);
    var id = UmaFSState.nextId++;
    UmaFSState.reqs[id] = 0;
    (async function () {
      try {
        var fs = window.__umaFS;
        if (!fs || !fs.root) { UmaFSState.reqs[id] = 2; return; }
        var prefix = fs.rootPrefix || '/uma/';
        var rel = path.indexOf(prefix) === 0 ? path.substring(prefix.length) : path.replace(/^\/+/, '');
        var parts = rel.split('/').filter(function (p) { return p.length; });
        if (!parts.length) { UmaFSState.reqs[id] = 2; return; }

        var dir = fs.root;
        for (var i = 0; i < parts.length - 1; i++) {
          dir = await dir.getDirectoryHandle(parts[i]);
        }
        var handle = await dir.getFileHandle(parts[parts.length - 1]);
        var file = await handle.getFile();
        var bytes = new Uint8Array(await file.arrayBuffer());

        var mem = path.split('/').filter(function (p) { return p.length; });
        var cur = '';
        for (var k = 0; k < mem.length - 1; k++) {
          cur += '/' + mem[k];
          try { FS.mkdir(cur); } catch (e) {}
        }
        FS.writeFile(path, bytes);
        UmaFSState.reqs[id] = 1;
      } catch (e) {
        if (e && e.name === 'NotFoundError') { UmaFSState.reqs[id] = 2; }
        else { console.error('[UmaFS] mount failed: ' + path, e); UmaFSState.reqs[id] = 3; }
      }
    })();
    return id;
  },

  UmaFS_PollMount__deps: ['$UmaFSState'],
  UmaFS_PollMount: function (id) {
    var s = UmaFSState.reqs[id];
    if (s === undefined) return 3;
    if (s !== 0) delete UmaFSState.reqs[id];
    return s;
  },

  // The app's "Change DataPath" button routes here; the template picks a new folder, stores the
  // handle, and reloads.
  UmaFS_ChangeFolder: function () {
    if (window.umaPickAndReload) window.umaPickAndReload();
  },
});
