(function () {
  const Analytics = {
    record(mode, result) {
      const cls = window.OZGE.getClassState();
      const analytics = { ...cls.weeklyAnalytics };
      const accuracy = analytics.accuracyByMode || {};
      accuracy[mode] = Math.round(((accuracy[mode] || 0) + result.score) / 2);
      analytics.accuracyByMode = accuracy;
      analytics.mostMissed = updateMissed(analytics.mostMissed, result.missed || []);
      analytics.speakingScores = [...(analytics.speakingScores || []), { time: Date.now(), score: result.score }].slice(-20);
      window.OZGE.updateClass(cls.id, () => ({ weeklyAnalytics: analytics }));
    }
  };

  function updateMissed(list, words) {
    const counts = Object.fromEntries(list.map((item) => [item.word, item.count]));
    words.forEach((word) => {
      counts[word] = (counts[word] || 0) + 1;
    });
    return Object.entries(counts).map(([word, count]) => ({ word, count })).sort((a, b) => b.count - a.count).slice(0, 10);
  }

  window.OZGE = window.OZGE || {};
  window.OZGE.Analytics = Analytics;
})();
