"""Read-only bounded proof of a forced AI win; false/timeout is NOT a solvability proof."""
import argparse
from functools import lru_cache
import json
from pathlib import Path
import time

LINES = []
for x in range(7):
    for y in range(6):
        for dx, dy in ((1, 0), (0, 1), (1, 1), (-1, 1)):
            points = [(x+n*dx, y+n*dy) for n in range(4)]
            if all(0 <= a < 7 and 0 <= b < 6 for a, b in points):
                LINES.append(tuple(b*7+a for a, b in points))
ORDER = (3, 2, 4, 1, 5, 0, 6)

def winner(board):
    for line in LINES:
        side = board[line[0]]
        if side and all(board[i] == side for i in line):
            return side
    return 0

def children(board, side):
    for col in ORDER:
        for row in range(6):
            index = row*7+col
            if board[index] == 0:
                candidate = list(board)
                candidate[index] = side
                yield col, tuple(candidate)
                break

def opening(stage):
    board = (0,)*42
    side = stage['starter']
    for col in map(int, stage['openingMoves'].split(',')) if stage['openingMoves'] else []:
        options = dict(children(board, side))
        assert col in options and winner(board) == 0
        board = options[col]
        side = -side
    assert winner(board) == 0
    return board, side

def audit(stage, depth, limit_seconds=8):
    start = time.monotonic()
    nodes = 0
    @lru_cache(maxsize=250000)
    def forced(board, side, left):
        nonlocal nodes
        nodes += 1
        if nodes % 256 == 0 and (time.monotonic()-start > limit_seconds or nodes > 250000):
            raise TimeoutError('bounded search budget exceeded')
        won = winner(board)
        if won:
            return won == -1
        if left == 0:
            return False
        moves = list(children(board, side))
        if not moves:
            return False
        # Terminal wins first; each branch remains an independently enumerated legal move.
        moves.sort(key=lambda item: winner(item[1]) == side, reverse=True)
        if side == -1:
            return any(forced(child, 1, left-1) for _, child in moves)
        return all(forced(child, -1, left-1) for _, child in moves)
    board, side = opening(stage)
    try:
        proven = forced(board, side, depth)
        status = 'FORCED_AI_WIN_PROVEN_WITHIN_BOUND' if proven else 'NOT_PROVEN_WITHIN_BOUND'
        certificate = None
        if proven:
            def proof(current, turn, left):
                if winner(current):
                    return {'winner': winner(current)}
                edges = {}
                for col, child in children(current, turn):
                    if turn == 1 or forced(child, 1, left-1):
                        edges[str(col)] = proof(child, -turn, left-1)
                        if turn == -1:
                            break
                return {'turn': turn, 'moves': edges}
            certificate = proof(board, side, depth)
            def verify(current, turn, tree):
                if 'winner' in tree:
                    assert winner(current) == tree['winner'] == -1
                    return
                legal = dict(children(current, turn))
                edges = {int(k): v for k, v in tree['moves'].items()}
                assert set(edges).issubset(legal)
                assert set(edges) == set(legal) if turn == 1 else len(edges) == 1
                for col, subtree in edges.items():
                    verify(legal[col], -turn, subtree)
            verify(board, side, certificate)
        return {'stage': stage['id'], 'depth': depth, 'status': status, 'nodes': nodes,
                'seconds': round(time.monotonic()-start, 3), 'certificate': certificate}
    except TimeoutError:
        return {'stage': stage['id'], 'depth': depth, 'status': 'SEARCH_LIMIT_NOT_A_VERDICT',
                'nodes': nodes, 'seconds': round(time.monotonic()-start, 3)}

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('stages', type=Path)
    parser.add_argument('output', type=Path)
    parser.add_argument('--ids', default='10,12,20,22,24,26')
    args = parser.parse_args()
    selected = {int(i) for i in args.ids.split(',')}
    data = json.loads(args.stages.read_text(encoding='utf-8-sig'))
    results = []
    for stage in data['stages']:
        if stage['id'] not in selected:
            continue
        for depth in (2, 4, 6, 8):
            result = audit(stage, depth)
            results.append(result)
            print(json.dumps({k: v for k, v in result.items() if k != 'certificate'}), flush=True)
            args.output.write_text(json.dumps({'method': 'Independent whole-board windows; all player moves and an AI winning strategy; terminal proof certificates verified. Not tied to the actual fixed-seed AI choices.', 'results': results}, ensure_ascii=False, indent=2), encoding='utf-8')
            if result['status'] == 'FORCED_AI_WIN_PROVEN_WITHIN_BOUND':
                break

if __name__ == '__main__':
    main()
