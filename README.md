# Dollar Recognizer (Multi-Stroke) — README

A lightweight, fast gesture-recognition toolkit with built in template storing that supports **multi-stroke** drawing. It implements three classic families of recognizers side-by-side—**$1**, **$N**, and a **Penny Pincher** (feature-based prefilter)—and returns **uniform statistics** so you can compare results, tune thresholds, and pick the best recognizer for your app or device constraints.

---

## Table of Contents
- [Why this project](#why-this-project)
- [Algorithms at a glance](#algorithms-at-a-glance)
- [How recognition works](#how-recognition-works)
- [Statistics reported](#statistics-reported)

---

## Why this project
Gesture recognizers are often presented in isolation. This repo lets you:
- **Compare** $1, $N, and a **feature-based prefilter** (“Penny Pincher”) on the **same inputs**.
- **See the numbers** (distances, angles, confidence) that drive decisions.
- **Tune thresholds** to your hardware, users, and gesture set.
- **Use multi-stroke** gestures without being locked to stroke order or direction.

---

## Algorithms at a glance

| Recognizer | Multi-Stroke | Invariances | Score (intuition) | Speed | Typical Use |
|---|---|---|---|---|---|
| **$1** | ❌ (unistroke) – used here by concatenating or pre-combining strokes when desired | Rotation, scale, translation | `1 - normalized_path_distance` | Very fast | Prototyping; single-stroke or pre-combined paths |
| **$N** | ✅ (true multi-stroke) | Rotation, scale, translation; **stroke order & direction invariance** via permutations/heuristics | `1 - normalized_min_distance_over_permutations` | Fast–Moderate (depends on templates, permutations) | Natural multi-stroke input (e.g., characters, symbols) |
| **Penny Pincher (Prefilter)** | ✅ | Mostly translation/scale via normalization; coarse angle bins | Low-cost feature distance to **prune** template set before $N/$1 | **Very fast** | Mobile/embedded; large template sets; early pruning |

---

## How recognition works

### Shared preprocessing
1. **Resample** each stroke to a fixed number of points (e.g., 64 total; configurable).
2. **Normalize**: translate to centroid, scale to a reference square, optionally rotate to an indicative angle (for $1 / $N).
3. **Multi-stroke handling**:
   - **$1**: (optional) combine strokes into one path (you choose how).
   - **$N**: match across **permutations and directions** with pruning heuristics.

### Matching (per recognizer)
- **$1 recognizer**  
  - Distance metric: average path distance between candidate and template.  
  - Score: $$\= 1 - \frac{\text{distance}}{0.5 \cdot \sqrt{2} \cdot \text{SquareSize}}$$
  - Returns best template and stats (distance, rotation, etc.).

- **$N recognizer**  
  - Explores stroke **order/direction permutations** (with pruning by start-angle/end-angle, stroke count, etc.).  
  - Uses the same (or very similar) normalized distance and scoring as $1 on the best permutation.  
  - Returns best template and stats, plus how many permutations were evaluated.

- **Penny Pincher (feature prefilter)**  
  - Computes a **cheap feature vector** per gesture & template:  
    - stroke count, total/avg stroke length, bounding-box ratio, indicative angle bin(s), start–end vector, simple direction histogram, corner count (optional), and centroid offset stats.  
  - Computes a **feature distance** (e.g., weighted L2) and **keeps top-K** templates if `feature_distance ≤ feature_threshold`.  
  - Passes survivors to $N (or $1) for exact matching.  
  - Returns its own stats (feature_distance, kept/topK, reject_reason if any).

---
