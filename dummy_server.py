from http.server import HTTPServer, BaseHTTPRequestHandler
import time

class SimpleHandler(BaseHTTPRequestHandler):
    def do_POST(self):
        time.sleep(0.5)
        self.send_response(200)
        self.send_header('Content-type', 'application/json')
        self.end_headers()
        self.wfile.write(b'{"permalink": "http://example.com", "sha256": "123", "scan_id": "123"}')

httpd = HTTPServer(('127.0.0.1', 8080), SimpleHandler)
httpd.serve_forever()
