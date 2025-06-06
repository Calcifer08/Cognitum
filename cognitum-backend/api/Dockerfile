FROM node:18

WORKDIR /app

COPY package*.json ./

RUN npm install --production

COPY . .

EXPOSE ${APP_PORT}

CMD ["node", "main.js"]
