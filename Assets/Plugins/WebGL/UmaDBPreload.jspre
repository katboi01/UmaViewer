// Before Unity boots, fault the meta and master DBs from the picked folder into MEMFS. The
// database controller opens them by path in Awake, and Unity's Start/Awake order is undefined,
// so an addRunDependency in preRun holds main() until the files exist. Bundles load later and
// use the on-demand jslib mount instead.
Module.preRun = Module.preRun || [];
Module.preRun.push(function () {
  var fs = (typeof window !== 'undefined') ? window.__umaFS : null;
  if (!fs || !fs.root) { console.warn('[UmaFS] no folder picked; databases not preloaded'); return; }

  function mkdirp(path) {
    var parts = path.split('/').filter(function (p) { return p.length; });
    var cur = '';
    for (var k = 0; k < parts.length; k++) { cur += '/' + parts[k]; try { FS.mkdir(cur); } catch (e) {} }
  }

  addRunDependency('uma-db');
  (async function () {
    var files = ['meta', 'master/master.mdb'];
    for (var i = 0; i < files.length; i++) {
      var rel = files[i];
      try {
        var parts = rel.split('/');
        var dir = fs.root;
        for (var j = 0; j < parts.length - 1; j++) dir = await dir.getDirectoryHandle(parts[j]);
        var handle = await dir.getFileHandle(parts[parts.length - 1]);
        var bytes = new Uint8Array(await (await handle.getFile()).arrayBuffer());
        var full = (fs.rootPrefix || '/uma/') + rel;
        mkdirp(full.substring(0, full.lastIndexOf('/')));
        FS.writeFile(full, bytes);
        console.log('[UmaFS] preloaded ' + full + ' (' + bytes.length + ' bytes)');
      } catch (e) {
        console.warn('[UmaFS] could not preload ' + rel, e);
      }
    }
    removeRunDependency('uma-db');
  })();
});
