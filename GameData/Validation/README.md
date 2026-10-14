# 6개 국면 교체 검증

2026-10-14. 최종 동기화 상태는 [CHECKPOINT.md](C:/Github/StageGame/GameData/Validation/CHECKPOINT.md)를 따른다. 현재 `Generated/stages.json`에는 10/12/20/22/24/26의 새 수순이 반영되어 있다.

## 근거 파일

| 파일 | 내용 |
| --- | --- |
| [opening-repairs.json](C:/Github/StageGame/GameData/Validation/opening-repairs.json) | 6개 ID별 이전/새 수순과 후보 선택 기록 |
| [bounded-outcomes.json](C:/Github/StageGame/GameData/Validation/bounded-outcomes.json) | 이전의 제한 깊이 패배와 수정 후 깊이 8 승리 전략 판정, 현재 원본/JSON 해시 |
| [verify_repairs.py](C:/Github/StageGame/GameData/Validation/verify_repairs.py) | 원본 Excel과 이전 수순을 읽어 두 결과를 재계산. Excel과 runtime JSON은 변경하지 않음 |
| [repair-preservation.json](C:/Github/StageGame/GameData/Validation/repair-preservation.json) | Excel의 변경 셀 6개, 스타일 차이 0개, JSON 변경 ID/필드, 광고·문자열 보존 |
| [before-early.png](C:/Github/StageGame/GameData/Validation/before-early.png) / [after-early.png](C:/Github/StageGame/GameData/Validation/after-early.png) | Stages G13:K16의 변경 전후 렌더 |
| [before-later.png](C:/Github/StageGame/GameData/Validation/before-later.png) / [after-later.png](C:/Github/StageGame/GameData/Validation/after-later.png) | Stages G24:K30의 변경 전후 렌더 |

최초 QA 증거는 `Docs/QA/Evidence/qa-core-v2-20261014/stage-paths.json`과 `run.log`다. 12번의 실제 C# AI 탐색 실패만으로 불가능을 선언하지 않았으며, 독립적인 판정으로 원국면의 문제를 확인했다.

## 분석 의미와 재현

`audit_openings.py`는 7비트 열 간격 bitboard와 전수 minimax를 사용한다. 휴리스틱 점수·플레이아웃 통계·랜덤 승률로 판정하지 않는다. 즉승이 없으면 상대의 단일 즉승 열을 막아야 하며, 서로 다른 두 즉승 열은 한 수로 모두 막을 수 없다는 규칙으로만 가지를 줄인다. 깊이 0의 값은 0(미해결/무승부)이고, 미해결 가지를 패배로 바꾸지 않는다.

- `1/bounded_win`: 현재 차례가 상대의 모든 응답에 대해 지정 깊이 안에 이기는 전략이 있음.
- `-1/bounded_loss`: 어떤 합법 수를 골라도 최적 상대 대응에 지정 깊이 안에 패배.
- `0/unknown_or_draw`: 제한 깊이에서 확정하지 못했거나 무승부. 안전·해결 가능 판정이 아님.

원국면 결과는 ID 순으로 깊이 8/6/6/6/18/22에서 -1이며 새 6개는 깊이 8에서 모두 +1이다. 결과는 **현재 차례 기준**이고, 이 여섯 국면은 모두 플레이어 차례다. 작은 깊이의 가지를 줄이지 않은 전체 보드 검사와 bitboard 결과 일치, 모든 30국면의 즉승 열 일치, 수정 6개의 결과를 Python 테스트에 추가했다. 총 28개 통과.

```powershell
$stagePython = 'C:\Users\JW\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
& $stagePython C:\Github\StageGame\GameData\Validation\verify_repairs.py
& $stagePython C:\Github\StageGame\GameData\audit_openings.py --depth 8
& $stagePython -m unittest discover -s C:\Github\StageGame\GameData -p test_export_data.py -v
& $stagePython C:\Github\StageGame\GameData\export_data.py --check
```

`verify_repairs.py`는 동일 폴더의 `bounded-outcomes.json`만 갱신한다. `.inspection`의 탐색 스크립트 없이 재현된다. 원본 수순은 새 파일로 복사한 외부 게임 자료가 아니라 이 프로젝트의 이전 콘텐츠이며, 전후 비교를 위해 JSON에 남겼다.

후속 [core-v4 경로](C:/Github/StageGame/Docs/QA/Evidence/qa-core-v4-20261014/stage-paths.json)에서 새 생성물의 30/30 승리 예시를 확인했다. 해당 탐색은 실제 C# AI·시드·노드 예산을 사용하지만 런타임 0.65초 제한을 재현하지 않는다. 제한 깊이 전략과 AI 예시 존재는 사람이 쉽게 찾거나 재미있다는 증거가 아니다. 전체 퍼즐 인증, 난이도 완성, 기기 성능 또는 출시 gate 전체 통과로 확대 해석하지 않는다.

## QA-004 실패 복구

[verify_export_failure.py](C:/Github/StageGame/GameData/Validation/verify_export_failure.py)로 처리 가능한 교체 실패와 복구 실패를 주입한다. [export-failure-results.json](C:/Github/StageGame/GameData/Validation/export-failure-results.json)에 세 검사 결과, exporter 해시와 전후 원본/JSON 동일성을 기록했다. 1/2/3번째 교체 실패의 기존 묶음 보존, 처음 생성할 때의 파일 부재 복원, 복구 실패의 표시·백업 보존과 `--check`/재시도 거부를 통과했다.

프로세스 강제 종료나 전원 손실까지 전체 묶음의 원자성을 보장하지 않는다. `.export-incomplete`가 있으면 이전 백업 묶음을 복원·대조하고 표시를 정리한 뒤 다시 export 및 `--check`를 수행해야 한다. 평상시 출력 내용은 수정 전과 동일하다.
