# 데이터 재동기화 준비 완료

## 최신: v9 확인 패널+프로 출신 훈련병 서사 통합 완료

2026-10-14. **228개 중 191개 보존·37개 수정 + 신규 9개 = 237개 ko/en**. 확인 패널·다시하기·차례 토스트를 유지하고 계급별 대사 30개, 도전자 계급 표시를 변경했다. 첫 대국 지정 대사 3개와 요약을 추가하고 Stage 1의 대사 참조 3개만 전용 키로 변경했다. [최종 필드·키·대사·해시](C:/Github/StageGame/GameData/STORY-CONTRACT.md), [통합 검증](C:/Github/StageGame/GameData/Validation/story-results.json).

사용자 지정 첫 한국어 대사 원문 검사, 1단계 예외 범위/대사 역할 검사 포함 **44개 테스트 통과**, 반복 export 동일, `--check` 및 변경 범위 렌더 완료. 기존 30개 설명·시작판·AI·규칙·ID·광고·Excel 서식은 보존했다. **Client 재동기화 가능**. 플레이어는 훈련병으로 유지하며 더 높은 계급의 도전자가 등장하는 서사다. 실제 UI는 브라우저 검증 대상으로 인계하며 실물 폰 시험은 보류한다.

## 이전: v9 확인 패널·다시하기 문구 완료

2026-10-14. **기존 224개 보존·4개 라벨 수정 + 신규 5개 = 233개 ko/en**. 나가기/다시하기 제목·본문, 취소 키를 추가하고 `confirm_leave` 및 다시하기 라벨을 맞췄다. 차례 토스트는 기존 `ui_player_turn`(내 차례 / Your turn) 재사용이다. [Client 연결용 키·문구·해시](C:/Github/StageGame/GameData/V9-UI-CONTRACT.md), [검증 결과](C:/Github/StageGame/GameData/Validation/v9-ui-results.json).

시작판·AI·대사·규칙·계급/별·전체 stages/ads JSON 바이트와 기존 Excel 서식을 보존했다. 반복 export 동일, `--check`, **42개 테스트 통과**, 변경 범위 렌더 확인 완료. 문자열 SHA-256 `1259006d809a1b66d3d585a68ad070f94789bcc4b2a51022bb92096a292809c9`. **Client 재동기화 가능**. 실제 패널/토스트 동작은 Client/총괄 브라우저 검증 범위이며 실물 폰 시험은 사용자 요청으로 보류한다.

## 이전: 한 수 진급 계급 UI 데이터 완료

2026-10-14. **10계급×3단계, 별 1–5, 228개 ko/en**. 기존 137개 중 90개 보존·47개 승인 수정 + 신규 91개다. 기존 30개의 ID/titleKey/chapter/openingMoves/AI 값과 Stages A1:K34를 그대로 유지하고 L:Q에 표시용 6개 필드를 추가했다. 광고 전체, 기존 셀 서식·크기·고정창·입력 유효성을 보존했다. 설명·AI 대사 60개를 한 문장으로 간결화해 최대 ko 22자 / en 41자다. 민트/산호 및 열 클릭 즉시 착수 문구는 생성 문자열에 없다.

최종 반복 export 바이트 동일, `--check`, **42개 Python 테스트**와 변경 범위 렌더 검증 완료. [필드·키·전체 해시 및 인계](C:/Github/StageGame/GameData/RANK-UI-CONTRACT.md), [검증 결과](C:/Github/StageGame/GameData/Validation/rank-ui-results.json). stages SHA-256 `6e66c8b0abe2dcc3b5297bf08a98953929b7e17b31e740569910255f7601fc26`, strings SHA-256 `e191ce6773c2e608831f4f38bbb1b995bdd2caec3c156693cdbe71e50645828c`. **Client 재동기화 가능**. QA 스냅샷에는 새 테스트 의존성 `GameData/Validation/rank-ui-changes.json`을 포함한다. UI 실행·저장 복원·사람 난이도 검증은 이 데이터 완료 범위에 포함하지 않는다.

## 이전: 단계 선택·그림 도움말 14개 추가 및 패배 안내 3개 수정

2026-10-14. **기존 120개 보존 + 방향별 패배 안내 3개 승인 수정 + 신규 14개 = 총 137개**, schemaVersion 1이다. 총괄의 실제 브라우저 검사로 승인된 `result_loss_horizontal/vertical/diagonal`의 둘째 줄(금색 줄 안내)만 ko/en에서 삭제했다. Excel 변경 셀은 C101:C103·E101:E103이며, 나머지 기존 값과 전체 서식·행/열 크기·고정창·입력 유효성을 보존했다. 스테이지/광고 시트와 JSON 바이트도 그대로다. 반복 export 동일, `--check` 및 Python **36개 테스트 통과**. 추가 범위 B128:E141 및 수정 범위 B101:E103의 렌더를 확인했다. Client 재동기화 가능.

`Generated/strings.json` SHA-256: `57e392cf68464898829946f17030e4ae3008aab86370452bacf6dbe56d68a291`. 원본과 전체 JSON 해시·신규 문구·포맷 인자 수는 [stage-map-strings-results.json](C:/Github/StageGame/GameData/Validation/stage-map-strings-results.json)에 기록했다. 입문/전술/도전은 1–10/11–20/21–30 챕터 표시명이다. 사람 난이도나 실제 클라이언트 UI 검증 완료를 뜻하지 않는다.

## 이전: 한국어·영어 지원

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
