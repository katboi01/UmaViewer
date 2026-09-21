// Hands bytes from the WebGL heap to the browser as a file download. WebGL has no real
// filesystem, so File.WriteAllBytes only reaches the sandboxed MEMFS; this triggers the
// browser's own save-file flow instead.
mergeInto(LibraryManager.library, {
  UmaSaveFile: function (namePtr, dataPtr, length) {
    var name = UTF8ToString(namePtr);
    // Copy out of the heap: the object URL outlives this call and the heap can move.
    var bytes = HEAPU8.slice(dataPtr, dataPtr + length);
    var blob = new Blob([bytes], { type: 'application/octet-stream' });
    var url = URL.createObjectURL(blob);
    var a = document.createElement('a');
    a.href = url;
    a.download = name;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    setTimeout(function () { URL.revokeObjectURL(url); }, 10000);
  },
});
