FROM nginx:stable

RUN apt update && apt install -y openssl

# Если вы используете собственные сертификаты — поместите их в ./nginx/certs/
RUN mkdir -p /etc/nginx/certs && \
    if [ ! -f /etc/nginx/certs/server.key ]; then \
      echo "Сертификаты не найдены, создаём..." && \
      openssl req -x509 -nodes -days 365 \
        -subj "/C=RU/ST=Local/L=Dev/O=Dev/CN=localhost" \
        -newkey rsa:2048 \
        -keyout /etc/nginx/certs/server.key \
        -out /etc/nginx/certs/server.crt; \
    fi
