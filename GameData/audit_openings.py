"""Independent, heuristic-free bounded outcome audit. 0 means unknown or draw, never proven safe."""
import argparse
from functools import lru_cache
import json

from export_data import SOURCE, read_source, validate

ORDER = (3, 2, 4, 1, 5, 0, 6)


def four(bits):
    # Seven-bit column stride includes one sentinel bit above the six playable cells.
    for stride in (1, 6, 7, 8):
        adjacent = bits & (bits >> stride)
        if adjacent & (adjacent >> (2 * stride)):
            return True
    return False


@lru_cache(maxsize=1_000_000)
def solve(own, enemy, heights, depth):
    """1: side to move can force a win within depth; -1: forced loss; 0: unresolved/draw."""
    if depth == 0:
        return 0
    legal = [col for col in ORDER if heights[col] < 6]
    if not legal:
        return 0
    for col in legal:
        if four(own | (1 << (col * 7 + heights[col]))):
            return 1
    if depth == 1:
        return 0
    threats = [col for col in legal if four(enemy | (1 << (col * 7 + heights[col])))]
    if len(threats) > 1:
        return -1
    # With no immediate own win, a single opponent winning column must be blocked.
    if threats:
        legal = threats
    best = -1
    for col in legal:
        bit = 1 << (col * 7 + heights[col])
        next_heights = list(heights)
        next_heights[col] += 1
        score = -solve(enemy, own | bit, tuple(next_heights), depth - 1)
        if score == 1:
            return 1
        best = max(best, score)
    return best


def position(stage):
    stones, heights, side = {1: 0, -1: 0}, [0] * 7, stage["Starter"]
    for col in map(int, filter(None, (stage["OpeningMoves"] or "").split(","))):
        stones[side] |= 1 << (col * 7 + heights[col])
        heights[col] += 1
        side = -side
    return stones[side], stones[-side], tuple(heights)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--stages", type=int, nargs="+", default=[10, 12, 20, 22, 24, 26])
    parser.add_argument("--depth", type=int, choices=range(1, 27), default=10)
    args = parser.parse_args()
    tables = read_source(SOURCE)
    validate(tables)
    if not set(args.stages) <= set(range(1, 31)):
        parser.error("stage IDs must be 1..30")
    for stage in tables["Stages"]:
        if stage["Id"] not in args.stages:
            continue
        score = solve(*position(stage), args.depth)
        print(json.dumps({"id": stage["Id"], "depth": args.depth, "boundedOutcome": score,
                          "meaning": {1: "bounded_win", -1: "bounded_loss", 0: "unknown_or_draw"}[score]}))
        solve.cache_clear()


if __name__ == "__main__":
    main()
