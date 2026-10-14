# 데이터 재동기화 준비 완료

## 최신: 한국어·영어 지원

2026-10-14. 총괄 승인 및 Client 공유 요청을 반영해 Strings에 Content_Eng를 추가했다. ko/en 123개, schemaVersion 1, 기존 key/ko 유지. 신규 UI 키 65개이며 Client 요청 70개 전체의 문구·인자 수를 검증했다. `stages.json`과 `ads.json`은 이전 바이트 그대로다. exporter/`--check` 및 Python 36개 테스트가 통과했다. [계약·문자열 해시](C:/Github/StageGame/GameData/LOCALIZATION-CONTRACT.md), [보존 증거](C:/Github/StageGame/GameData/Validation/localization-results.json).

## 이전: 국면 6개 교체

2026-10-14. **Excel 수정 및 Generated 재export 완료. 클라이언트 동기화 가능.**

- 변경은 10/12/20/22/24/26의 `openingMoves`만이다. 다른 24개 국면·AI 값·제목·광고·문자열은 동일하다.
- 원본 Excel의 변경 셀은 `Stages!I14,I16,I24,I26,I28,I30` 6개. 시트·서식·행/열 크기·고정창·입력 유효성은 보존 비교를 통과했다.
- exporter와 `--check` 성공. 새 6개는 독립 bitboard 제한 깊이 8 전수 탐색에서 모두 `bounded_win`이다.
- 이전 6개는 최적 대응에서 8/6/6/6/18/22 ply 이내 패배로 확인했다. 최초 즉시 패배만 검사하면 놓치는 국면이었다.
- `Generated/stages.json` SHA-256: `2d04b124a42f0f3798ad942e9aacab97ee34a25f05b380144cd70f75c3904992`.
- 새 수순과 이전 수순: `opening-repairs.json`. 범위 보존 검사: `repair-preservation.json`. 변경 전후 Excel 렌더: `before-*.png`, `after-*.png`.

Python **28개 테스트 통과**. 후속 QA core-v4에서 이 데이터 해시로 **30/30 실제 C# AI 승리 예시**를 확인했다. 자동 결과를 실제 기기·사람 플레이 또는 완성된 밸런스로 표시하지 않는다.

**QA-004 수정 완료, 독립 QA 재검증 가능.** `export_data.py`는 교체 오류 시 이전 3파일을 복구하고, 복구 실패 시 `.export-incomplete`와 백업을 남겨 `--check`/재export를 거부한다. 기존 데이터 바이트는 동일하다. 재현은 `verify_export_failure.py`, 결과는 `export-failure-results.json`이다. 강제 프로세스 중단에 대한 묶음 원자성은 보장하지 않는다.

QA 스냅샷 테스트 의존성이 늘었다. `Tests/audit-data.py`에서 기존 3개 파일 외에 `GameData/audit_openings.py`와 `GameData/Validation/opening-repairs.json`을 **같은 상대 구조로 Sources에 포함하고 Sources를 Python import 경로에 추가**해야 28개 테스트 전체를 실행할 수 있다. 이 세션은 Tests/를 수정하지 않았다.

| Stage | 새 openingMoves |
| --- | --- |
| 10 | `3,3,4,1,3,1,1,5,3,0,4,0,1,2` |
| 12 | `6,1,6,6,5,3,4,5,1,3,3,2,0,6,6,2` |
| 20 | `2,5,5,3,6,5,3,0,5,6,1,4,3,2,6,2,0,1` |
| 22 | `2,1,5,0,0,2,0,6,3,3,6,6,6,3,2,1,5` |
| 24 | `0,6,5,0,3,6,5,1,1,1,2,4,6,1,4,4` |
| 26 | `0,4,6,3,3,5,0,4,4,3,2,3,3,0,4,1,0,2,2,4` |
