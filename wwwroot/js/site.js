const sourceEditor = document.querySelector("#cadena");
const validatorForm = document.querySelector("#validator-form");
const fileInput = document.querySelector("#program-file");
const loadButton = document.querySelector("#load-program");
const editButton = document.querySelector("#edit-program");
const programStatus = document.querySelector("#program-status");
const downloadSource = document.querySelector("#download-source");
const downloadButton = document.querySelector("#download-tokens");
const downloadHelp = document.querySelector("#download-help");

function markProgramAsEdited(message = "Cambios sin validar") {
  if (downloadSource) downloadSource.value = sourceEditor.value;
  if (downloadButton) {
    downloadButton.disabled = true;
    downloadButton.type = "button";
  }
  if (downloadHelp) downloadHelp.textContent = "Guarda y valida nuevamente el programa para habilitar la descarga.";
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
  markProgramAsEdited(`${file.name} cargado; falta guardar y validar`);
  sourceEditor.focus();
});

editButton?.addEventListener("click", () => {
  sourceEditor.focus();
  sourceEditor.setSelectionRange(sourceEditor.value.length, sourceEditor.value.length);
  if (programStatus) {
    programStatus.textContent = "Modo edición activo";
    programStatus.classList.add("dirty");
  }
});
