const sourceEditor = document.querySelector("#cadena");
const validatorForm = document.querySelector("#validator-form");
const fileInput = document.querySelector("#program-file");
const loadButton = document.querySelector("#load-program");
const editButton = document.querySelector("#edit-program");
const programStatus = document.querySelector("#program-status");
const downloadHelp = document.querySelector("#download-help");

function markProgramAsEdited(message = "Cambios sin validar") {
  document.querySelectorAll("[data-source-copy]").forEach((input) => {
    input.value = sourceEditor.value;
  });
  document.querySelectorAll("[data-save-button]").forEach((button) => {
    button.disabled = true;
    button.type = "button";
  });
  if (downloadHelp) downloadHelp.textContent = "Verifica nuevamente el programa para habilitar ambas descargas.";
  if (programStatus) {
    programStatus.textContent = message;
    programStatus.classList.add("dirty");
  }
}

document.querySelectorAll("[data-example]").forEach((button) => {
  button.addEventListener("click", () => {
    sourceEditor.value = button.dataset.example;
    markProgramAsEdited("Ejemplo cargado; validando...");
    validatorForm.requestSubmit();
  });
});

sourceEditor?.addEventListener("input", () => markProgramAsEdited());

loadButton?.addEventListener("click", () => fileInput.click());

fileInput?.addEventListener("change", async () => {
  const file = fileInput.files?.[0];
  if (!file) return;
  if (!file.name.toLowerCase().endsWith(".txt")) {
    fileInput.value = "";
    if (programStatus) {
      programStatus.textContent = "Selecciona únicamente un archivo .txt";
      programStatus.classList.add("dirty");
    }
    return;
  }

  sourceEditor.value = await file.text();
  markProgramAsEdited(`${file.name} cargado; verificando...`);
  validatorForm.requestSubmit();
});

editButton?.addEventListener("click", () => {
  sourceEditor.focus();
  sourceEditor.setSelectionRange(sourceEditor.value.length, sourceEditor.value.length);
  if (programStatus) {
    programStatus.textContent = "Modo edición activo";
    programStatus.classList.add("dirty");
  }
});
