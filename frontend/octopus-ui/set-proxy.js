const fs = require('fs');
const target = process.env.API_PROXY_TARGET || 'http://localhost:5000';
const config = {
  '/api': {
    target,
    secure: false,
    changeOrigin: true
  }
};
fs.writeFileSync('proxy.conf.json', JSON.stringify(config, null, 2) + '\n');
console.log(`Proxy target set to ${target}`);
