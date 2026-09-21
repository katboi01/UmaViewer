#!/usr/bin/env python3
# Serve the WebGL build over HTTP. Adds the gzip content header for .gz files and the wasm
# media type, so both decompression-fallback (.unityweb) and raw-gzip (.gz) builds work.
# File System Access API needs a secure context; localhost qualifies.
#
#   python3 tools/serve-web.py [port]   # default 8000, serves Build/Web
import os, sys, http.server, socketserver

ROOT = os.path.join(os.path.dirname(os.path.dirname(os.path.abspath(__file__))), "Build", "Web")

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *a, **kw):
        super().__init__(*a, directory=ROOT, **kw)

    # .unityweb (decompression-fallback) and .gz builds are gzip-compressed; advertising the
    # encoding lets the browser decompress natively instead of the loader's JS fallback.
    def end_headers(self):
        if self.path.endswith(".gz") or self.path.endswith(".unityweb"):
            self.send_header("Content-Encoding", "gzip")
        # Never let the browser reuse a stale build during local dev.
        self.send_header("Cache-Control", "no-store, no-cache, must-revalidate")
        super().end_headers()

    def guess_type(self, path):
        if ".wasm" in path:
            return "application/wasm"
        return super().guess_type(path)

def main():
    if not os.path.isdir(ROOT):
        sys.exit(f"No build at {ROOT}. Run tools/build-web.sh first.")
    port = int(sys.argv[1]) if len(sys.argv) > 1 else 8000
    with socketserver.TCPServer(("", port), Handler) as httpd:
        print(f"Serving {ROOT} at http://localhost:{port}")
        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            pass

if __name__ == "__main__":
    main()
