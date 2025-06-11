const form = document.getElementById("resetForm");

form.addEventListener("submit", function (event) {
  const password = document.getElementById("password").value;
  const confirmPassword = document.getElementById("confirmPassword").value;

  if (password !== confirmPassword) {
    event.preventDefault();
    alert("Пароли не совпадают. Пожалуйста, повторите ввод.");
  }
});
