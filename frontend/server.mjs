import { createReadStream, existsSync } from 'node:fs';
import { stat } from 'node:fs/promises';
import http from 'node:http';
import path from 'node:path';

const host = '0.0.0.0';
const port = Number(process.env.PORT ?? 8080);
const distDir = path.resolve('/app/dist');
const indexFile = path.join(distDir, 'index.html');

const contentTypes = new Map([
  ['.css', 'text/css; charset=utf-8'],
  ['.html', 'text/html; charset=utf-8'],
  ['.ico', 'image/x-icon'],
  ['.js', 'text/javascript; charset=utf-8'],
  ['.json', 'application/json; charset=utf-8'],
  ['.map', 'application/json; charset=utf-8'],
  ['.png', 'image/png'],
  ['.svg', 'image/svg+xml'],
  ['.txt', 'text/plain; charset=utf-8'],
  ['.woff', 'font/woff'],
  ['.woff2', 'font/woff2'],
]);

function sendFile(filePath, response) {
  const ext = path.extname(filePath).toLowerCase();
  response.writeHead(200, {
    'Content-Type': contentTypes.get(ext) ?? 'application/octet-stream',
  });
  createReadStream(filePath).pipe(response);
}

const server = http.createServer(async (request, response) => {
  const requestPath = new URL(request.url ?? '/', `http://${request.headers.host ?? 'localhost'}`).pathname;
  const sanitizedPath = path.normalize(requestPath).replace(/^(\.\.[/\\])+/, '');
  const targetPath = path.join(distDir, sanitizedPath === '/' ? 'index.html' : sanitizedPath);

  try {
    const fileStats = await stat(targetPath);
    if (fileStats.isFile()) {
      sendFile(targetPath, response);
      return;
    }
  } catch {
    // Fall through to SPA fallback.
  }

  if (!existsSync(indexFile)) {
    response.writeHead(500, { 'Content-Type': 'text/plain; charset=utf-8' });
    response.end('Frontend bundle is missing.');
    return;
  }

  sendFile(indexFile, response);
});

server.listen(port, host, () => {
  console.log(`Frontend listening on ${host}:${port}`);
});
