#!/usr/bin/env python3
"""Gráficos das simulações de domínio; não são capturas do jogo."""
import csv
from pathlib import Path
import matplotlib
matplotlib.use("Agg")
import matplotlib.pyplot as plt

ROOT = Path(__file__).resolve().parents[1]
DIRECTORY = ROOT / "docs" / "balance"
VERSION = (ROOT / "VERSION").read_text().strip()

def read(name):
    with (DIRECTORY / name).open(encoding="utf-8") as source:
        return list(csv.DictReader(source))

rows = read(f"simulation-{VERSION}.csv")
long_term = read(f"long-term-xp-{VERSION}.csv")
fig, axes = plt.subplots(1, 2, figsize=(12, 4.8), constrained_layout=True)
palette = {"balanced": "#1e6f5c", "attack": "#e59a00", "survival": "#5576c1"}
labels = {"balanced": "Equilíbrio", "attack": "Ataque", "survival": "Sobrevivência"}
for strategy, color in palette.items():
    for seed in ("12345", "54321", "98765"):
        clear = [r for r in rows if r["strategy"] == strategy and r["seed"] == seed and r["result"] == "clear"]
        axes[0].plot([int(r["phase"]) for r in clear], [float(r["simulated_minutes"]) / 60 for r in clear],
                     color=color, alpha=.7, label=labels[strategy] if seed == "12345" else None)
axes[0].set(title="Campanha: 3 estratégias × 3 sementes", xlabel="Fase vencida", ylabel="Horas simuladas")
axes[0].legend()
axes[1].plot([int(r["level"]) for r in long_term], [float(r["projected_active_hours"]) for r in long_term], color="#1e6f5c", linewidth=2.5)
axes[1].set(title="XP individual: cenário favorável de farm", xlabel="Nível da broca", ylabel="Horas ativas projetadas")
for axis in axes:
    axis.grid(alpha=.2)
    axis.set_xlim(0, 60)
    axis.set_ylim(bottom=0)
fig.suptitle(f"Dig & Wonder {VERSION} • ensaios matemáticos, sem jogadores reais", fontsize=13, fontweight="bold")
fig.savefig(DIRECTORY / f"progression-{VERSION}.png", dpi=150)
print(DIRECTORY / f"progression-{VERSION}.png")
