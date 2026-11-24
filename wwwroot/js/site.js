document.querySelectorAll("[data-example]").forEach((button) => {
  button.addEventListener("click", () => {
    document.querySelector("#cadena").value = button.dataset.example;
    document.querySelector("#validator-form").requestSubmit();
  });
});
