const sourceEditor = document.querySelector("#cadena");
const sourceLineNumbers = document.querySelector("#source-line-numbers");
const validatorForm = document.querySelector("#validator-form");
const fileInput = document.querySelector("#program-file");
const loadButton = document.querySelector("#load-program");
const editButton = document.querySelector("#edit-program");
const programStatus = document.querySelector("#program-status");
const downloadHelp = document.querySelector("#download-help");

function updateSourceLineNumbers() {
  if (!sourceEditor || !sourceLineNumbers) return;
  const lineCount = sourceEditor.value.replace(/\r\n/g, "\n").replace(/\r/g, "\n").split("\n").length;
  sourceLineNumbers.textContent = Array.from({ length: lineCount }, (_, index) => index + 1).join("\n");
  sourceLineNumbers.scrollTop = sourceEditor.scrollTop;
}

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
    updateSourceLineNumbers();
    markProgramAsEdited("Ejemplo cargado; validando...");
    validatorForm.requestSubmit();
  });
});

sourceEditor?.addEventListener("input", () => {
  updateSourceLineNumbers();
  markProgramAsEdited();
});
sourceEditor?.addEventListener("scroll", updateSourceLineNumbers);

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
  updateSourceLineNumbers();
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

updateSourceLineNumbers();
