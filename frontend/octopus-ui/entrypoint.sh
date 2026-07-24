#!/bin/sh
# Substitute API_UPSTREAM env var into nginx config before starting
export API_UPSTREAM="${API_UPSTREAM:-http://api:5000}"
envsubst '${API_UPSTREAM}' < /etc/nginx/conf.d/default.conf > /etc/nginx/conf.d/default.conf.tmp
mv /etc/nginx/conf.d/default.conf.tmp /etc/nginx/conf.d/default.conf
exec nginx -g 'daemon off;'
