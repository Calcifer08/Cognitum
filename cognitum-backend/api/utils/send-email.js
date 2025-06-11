import nodemailer from "nodemailer";
import env from "../config/env.js";

const transporter = nodemailer.createTransport({
  host: env.SMTP_HOST,
  port: env.SMTP_PORT,
  secure: false,
  auth: {
    user: env.SMTP_USER,
    pass: env.SMTP_PASS,
  },
});

export const sendPasswordResetEmail = async (to, link) => {
  const mailOptions = {
    from: env.SMTP_USER,
    to,
    subject: "Сброс пароля",
    html: `
      <p>Вы запросили сброс пароля.</p>
      <p>Нажмите на ссылку ниже, чтобы сбросить пароль:</p>
      <a href="${link}">${link}</a>
      <p>Если вы не запрашивали сброс, проигнорируйте это письмо.</p>
    `,
  };

  await transporter.sendMail(mailOptions);
};
