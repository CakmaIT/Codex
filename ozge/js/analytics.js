(function () {
  function calculate(cls) {
    const modeCounts = {};
    cls.lessonLog.forEach((entry) => {
      const mode = entry.mode || "HOME";
      if (!modeCounts[mode]) {
        modeCounts[mode] = { correct: 0, total: 0 };
      }
      modeCounts[mode].correct += entry.correct || 0;
      modeCounts[mode].total += entry.total || 0;
    });

    const behaviorTrend = cls.behaviorLog.slice(-50).map((item) => ({
      time: item.timestamp,
      value: item.action === "positive" ? 5 : item.action === "penalty" ? -10 : 0
    }));

    const difficultWords = {};
    cls.lessonLog.forEach((entry) => {
      (entry.missedWords || []).forEach((word) => {
        difficultWords[word] = (difficultWords[word] || 0) + 1;
      });
    });

    return {
      modeCounts,
      behaviorTrend,
      difficultWords: Object.entries(difficultWords)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 10)
    };
  }

  function render(container, cls) {
    container.innerHTML = `
      <section class="analytics-grid">
        <article class="analytics-card" id="chart-modes">
          <h3>Accuracy by Mode</h3>
          <canvas class="chart" data-chart="mode"></canvas>
        </article>
        <article class="analytics-card" id="chart-words">
          <h3>Most Missed Words</h3>
          <ul class="word-frequency"></ul>
        </article>
        <article class="analytics-card" id="chart-behavior">
          <h3>Behavior Over Time</h3>
          <canvas class="chart" data-chart="behavior"></canvas>
        </article>
        <article class="analytics-card">
          <h3>Export</h3>
          <button id="analytics-export-json" class="ghost">Download JSON</button>
          <button id="analytics-export-csv" class="ghost">Download CSV</button>
          <button id="analytics-print" class="primary">Print/PDF</button>
        </article>
      </section>
    `;

    const data = calculate(cls);
    drawModeChart(container.querySelector('[data-chart="mode"]'), data.modeCounts);
    drawBehaviorChart(container.querySelector('[data-chart="behavior"]'), data.behaviorTrend);
    renderWordList(container.querySelector('.word-frequency'), data.difficultWords);

    container.querySelector('#analytics-export-json').addEventListener('click', () => exportJSON(cls, data));
    container.querySelector('#analytics-export-csv').addEventListener('click', () => exportCSV(cls, data));
    container.querySelector('#analytics-print').addEventListener('click', () => window.print());
  }

  function drawModeChart(canvas, modeCounts) {
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    const modes = Object.keys(modeCounts);
    if (!modes.length) {
      ctx.fillText('No data yet', 10, 30);
      return;
    }
    const width = canvas.width || canvas.offsetWidth;
    const height = canvas.height || canvas.offsetHeight;
    const barWidth = width / (modes.length * 1.5);
    modes.forEach((mode, index) => {
      const stats = modeCounts[mode];
      const ratio = stats.total ? stats.correct / stats.total : 0;
      const barHeight = ratio * (height - 20);
      ctx.fillStyle = '#3c50ff';
      ctx.fillRect(20 + index * barWidth * 1.5, height - barHeight - 10, barWidth, barHeight);
      ctx.fillStyle = '#0f1a2a';
      ctx.fillText(`${mode} ${(ratio * 100).toFixed(0)}%`, 20 + index * barWidth * 1.5, height - 2);
    });
  }

  function drawBehaviorChart(canvas, trend) {
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    if (!trend.length) {
      ctx.fillText('No behavior entries yet', 10, 30);
      return;
    }
    const width = canvas.width || canvas.offsetWidth;
    const height = canvas.height || canvas.offsetHeight;
    ctx.beginPath();
    ctx.moveTo(0, height / 2);
    trend.forEach((item, index) => {
      const x = (index / Math.max(1, trend.length - 1)) * width;
      const y = height / 2 - item.value * 2;
      ctx.lineTo(x, y);
    });
    ctx.strokeStyle = '#ffba3b';
    ctx.lineWidth = 3;
    ctx.stroke();
  }

  function renderWordList(list, entries) {
    if (!list) return;
    list.innerHTML = entries.map(([word, count]) => `<li>${word} <strong>${count}</strong></li>`).join('');
  }

  function exportJSON(cls, data) {
    const blob = new Blob([JSON.stringify({ classId: cls.id, generatedAt: new Date().toISOString(), data }, null, 2)], { type: 'application/json' });
    downloadBlob(blob, `${cls.id}-analytics.json`);
  }

  function exportCSV(cls, data) {
    const rows = ['mode,accuracy'];
    Object.entries(data.modeCounts).forEach(([mode, stats]) => {
      const ratio = stats.total ? stats.correct / stats.total : 0;
      rows.push(`${mode},${(ratio * 100).toFixed(0)}`);
    });
    const blob = new Blob([rows.join('\n')], { type: 'text/csv' });
    downloadBlob(blob, `${cls.id}-analytics.csv`);
  }

  function downloadBlob(blob, filename) {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    a.click();
    URL.revokeObjectURL(url);
  }

  window.Analytics = {
    render,
    calculate
  };
})();
