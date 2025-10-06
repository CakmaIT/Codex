(function () {
  async function extractText(file) {
    if (file.type === "application/pdf" || file.name.endsWith(".pdf")) {
      const buffer = await file.arrayBuffer();
      const ascii = new TextDecoder("utf-8", { fatal: false }).decode(buffer);
      const matches = ascii.match(/\(([^\)]+)\)/g);
      if (matches && matches.length) {
        return matches.map((m) => m.replace(/[()]/g, "")).join(" ");
      }
      return simplePdfExtract(ascii);
    }
    if (file.type.startsWith("image/")) {
      return await fakeOcrFromImage(file);
    }
    return await file.text();
  }

  function simplePdfExtract(text) {
    return text
      .replace(/\r|\n/g, " ")
      .replace(/[^A-Za-z0-9\s]/g, " ")
      .split(/\s+/)
      .filter(Boolean)
      .join(" ");
  }

  async function fakeOcrFromImage(file) {
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d");
    const img = document.createElement("img");
    return new Promise((resolve) => {
      img.onload = () => {
        canvas.width = img.width;
        canvas.height = img.height;
        ctx.drawImage(img, 0, 0);
        const sample = ctx.getImageData(0, 0, Math.min(200, img.width), Math.min(200, img.height));
        const brightness = averageBrightness(sample.data);
        resolve(brightness > 128 ? "bright image" : "dark image");
      };
      img.onerror = () => resolve("image" + file.name);
      img.src = URL.createObjectURL(file);
    });
  }

  function averageBrightness(data) {
    let total = 0;
    for (let i = 0; i < data.length; i += 4) {
      total += data[i] * 0.299 + data[i + 1] * 0.587 + data[i + 2] * 0.114;
    }
    return total / (data.length / 4);
  }

  window.OZGE = window.OZGE || {};
  window.OZGE.OCR = { extractText };
})();
