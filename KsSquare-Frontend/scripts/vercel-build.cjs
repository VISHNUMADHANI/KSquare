const fs = require('node:fs');
const raw = process.env.BACKEND_ORIGIN;
if (!raw) throw new Error('Set BACKEND_ORIGIN to your deployed backend HTTPS origin.');
const origin = new URL(raw);
if (origin.protocol !== 'https:' || origin.username || origin.password || origin.pathname !== '/' || origin.search || origin.hash) {
  throw new Error('BACKEND_ORIGIN must be an HTTPS origin without credentials, path, or query.');
}
const root = '.vercel/output';
fs.mkdirSync(`${root}/static`, { recursive: true });
fs.cpSync('dist/ks-square/browser', `${root}/static`, { recursive: true });
fs.writeFileSync(`${root}/config.json`, JSON.stringify({
  version: 3,
  routes: [
    { src: '/api/(.*)', dest: `${origin.origin}/api/$1` },
    { handle: 'filesystem' },
    { src: '/(.*)', dest: '/index.html' }
  ]
}, null, 2));