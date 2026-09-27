// Keep browser requests on the frontend origin for local use and client previews.
module.exports = {
  '/api': {
    target: 'http://127.0.0.1:5000',
    changeOrigin: true,
    configure(proxy) {
      proxy.on('proxyRes', (response, request) => {
        if (request.headers['x-forwarded-proto'] === 'https' && response.headers['set-cookie']) {
          response.headers['set-cookie'] = response.headers['set-cookie'].map(cookie =>
            /;\s*secure(?:;|$)/i.test(cookie) ? cookie : `${cookie}; Secure`);
        }
      });
    }
  }
};
