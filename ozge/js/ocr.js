(function () {
  async function extractTextFromPDF(file) {
    const buffer = await file.arrayBuffer();
    const textDecoder = new TextDecoder();
    const hint = textDecoder.decode(buffer.slice(0, 4096));
    const matches = hint.match(/[A-Za-z0-9 ,.;:'"\n\r-]{4,}/g) || [];
    return matches.join(" ").replace(/\s+/g, " ").trim();
  }

  async function extractTextFromImage(file) {
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");
    const bitmap = await createImageBitmap(file);
    canvas.width = bitmap.width;
    canvas.height = bitmap.height;
    ctx.drawImage(bitmap, 0, 0);
    const imageData = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const pixels = imageData.data;
    let contrast = 0;
    for (let i = 0; i < pixels.length; i += 4) {
      const avg = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
      contrast += avg > 128 ? 1 : -1;
    }
    if (Math.abs(contrast) < pixels.length / 1000) {
      return "";
    }
    return "(Image text approximation unavailable offline)";
  }

  async function runOCR(file) {
    const ext = (file.name.split('.').pop() || '').toLowerCase();
    if (ext === "pdf") {
      return extractTextFromPDF(file);
    }
    return extractTextFromImage(file);
  }

  window.OCR = {
    run: runOCR
  };
})();
