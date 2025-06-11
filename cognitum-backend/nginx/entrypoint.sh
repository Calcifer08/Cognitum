#!/bin/bash

CERT_DIR="/etc/nginx/certs"

if [ -z "$(ls -A $CERT_DIR)" ]; then
  echo "Папка сертификатов пуста, генерируем новые сертификаты..."
  openssl req -x509 -nodes -days 365 \
    -subj "/C=RU/ST=Local/L=Dev/O=Dev/CN=localhost" \
    -newkey rsa:2048 \
    -keyout $CERT_DIR/server.key \
    -out $CERT_DIR/server.crt
else
  echo "Сертификаты уже существуют, генерация пропущена."
fi

exec "$@"
