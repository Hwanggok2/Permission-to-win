# 계급 UI 데이터 계약

후속 v9의 확인 패널·다시하기 라벨·차례 토스트 키는 [V9-UI-CONTRACT.md](C:/Github/StageGame/GameData/V9-UI-CONTRACT.md), **현재 237개 문자열·서사·Stage 1 대사 예외와 최종 해시**는 [STORY-CONTRACT.md](C:/Github/StageGame/GameData/STORY-CONTRACT.md)를 따른다. 아래 생성 결과와 해시는 계급 개편 직후 228개 시점의 기록이다. 계급은 도전자의 계급이며 플레이어는 훈련병으로 유지한다.

2026-10-14 총괄 요청. 상태: **Excel 작성·JSON 생성·보존 검사·반복 생성·42개 테스트 완료, Client 동기화 가능**. Client/ 복사와 UI 연결은 클라이언트 담당이다. 작업명은 한국어 `한 수 진급`, 영어 `Rank by Rank`; 상점명 확정이나 권리 확인을 뜻하지 않는다.

## Stage 추가 필드

JSON wrapper와 schemaVersion 1, 기존 8개 Stage 필드·ID 1–30·titleKey·chapter·openingMoves·AI 설정을 유지한다. Excel Stages 기존 A:K 뒤 L:Q에 다음 6개를 추가한다.

| Excel | JSON | 형식/의미 |
| --- | --- | --- |
| RankKey | rankKey | 문자열 참조. `rank_01`–`rank_10`, 3단계씩 같은 계급 |
| DescriptionKey | descriptionKey | `stage_desc_001`–`stage_desc_030`, 해당 시작판에 맞는 한 줄 |
| DifficultyStars | difficultyStars | 정수 1–5. ID 1–6은 1, 7–12는 2, 13–18은 3, 19–24는 4, 25–30은 5 |
| AiIntroKey | aiIntroKey | `ai_rank_01_intro`–`ai_rank_10_intro`, 시작 때 AI 대사 |
| AiWinKey | aiWinKey | `ai_rank_01_win`–`ai_rank_10_win`, **AI 승리 / 플레이어 패배** 대사 |
| AiLoseKey | aiLoseKey | `ai_rank_01_lose`–`ai_rank_10_lose`, **플레이어 승리 / AI 패배** 대사 |

계급 순서: 훈련병, 이등병, 일병, 상병, 병장, 하사, 중사, 상사, 소위, 중위. 기존 `stage_001`–`stage_030`의 표시값을 `훈련병 1`, `훈련병 2`, `훈련병 3`, …, `중위 3`으로 수정한다. 기존 `chapter`는 저장/구버전 호환을 위한 3×10 값으로 남기고 새 계급 표시에는 `rankKey`를 쓴다. 계급과 별은 설계상 진행 표시이며 사람에게 검증된 난이도 등급이 아니다.

## 신규 UI 키

아래 키를 Excel Strings에서 ko/en으로 제공한다. 숫자 포맷은 기존 `{0}` 방식 그대로다.

- 인자 없음: `undo`(되돌리기), `place`(놓기), `retry`(재도전), `select_column`(놓을 열을 고른 뒤 '놓기'를 눌러요), `multiplayer_start`(대국 시작), `coming_soon`(준비중), `nextstage`(다음 단계), `first_match_title`, `first_match_body`, `stage_list_title`, `stage_detail_title`, `undo_unavailable`, `undo_done`, `multiplayer_title`, `multiplayer_unavailable`.
- 인자 1개: `stage_label`({0}=전체 단계 번호), `rank_label`({0}=번역된 계급명), `difficulty_stars`({0}=1–5), `selected_column_accessibility`({0}=사용자 표시 열 번호 1–7).
- 인자 2개: `rank_progress`({0}=현재 계급 번호 1–10, {1}=전체 계급 수 10).
- 인자 4개: `stage_card_accessibility`({0}=전체 단계 번호, {1}=번역된 titleKey 표시값, {2}=별 개수, {3}=번역된 완료/현재/잠금 상태).
- 잠금 안내는 기존 `stage_lock_requirement`({0}=먼저 완료할 전체 단계 번호), `stage_locked`, `stage_locked_detail`, 상태는 `stage_state_*`를 계속 쓴다.

`game_title`, `game_brand`, `web_canvas_label`은 새 작업명을 반영한다. `match_legend`는 나=흑돌/AI=백돌이다. 기존 `help_rules`, `client_help_rules`, `help_drop_title`, `help_drop_caption`, `match_instruction`, `client_ui_player_turn`을 **열 선택 → 놓일 위치 미리 보기 → 놓기 확정** 흐름에 맞춘다. 민트/산호·즉시 열 클릭 착수 안내를 남기지 않는다. 기존 포맷 인자 개수는 유지한다.

상세 패널의 기존 `client_ui_start`는 **시작하기 / Start**다. `multiplayer_start`는 **대국 시작 / Start game**으로 유지한다. `client_ui_player_turn`과 `select_column`의 중복 노출 여부는 Client UI에서 결정한다.

첫 실행 자동 1단계, 큰 2열 카드, 오른쪽 상세 패널, 되돌리기 실제 동작은 Client 구현/검증 범위다. `multiplayer_*`는 요청된 준비중 표시용이며 PvP·네트워크 대국을 추가하지 않는다. 중력식 7×6 4목 규칙과 기존 진행 저장 ID/해금/완료 의미를 유지한다.

## 생성 결과와 인계 (계급 개편 직후 228개 시점)

기존 137개 중 90개 보존·47개 승인 수정 + 신규 91개 = **228개 ko/en**이다. 신규는 계급명 10개·시작판 설명 30개·AI 대사 30개·UI 21개다. 기존 문자열 키와 포맷 인자 계약은 유지했다. 기존 Stages A1:K34, 광고 전체, 기존 셀의 서식·행/열 크기·고정창·검증 규칙 보존을 확인했다. 새 열은 L:Q, 새 문자열은 Strings A142:E232다. 모든 변경 범위를 렌더로 확인했다.

작은 화면 검토 요청에 따라 설명/AI 대사 60개를 한 문장으로 간결화했다. 최종 최대 길이는 한국어 22자, 영어 41자다. 아래 해시는 간결화 후의 최종값이다. 영문 계급명은 유지하며 실제 카드 렌더 후 필요하면 별도 승인으로 조정한다.

| 파일 | SHA-256 |
| --- | --- |
| GameData_MVP.xlsx | `b60229d2fbb81b4cd17b14805d45861d23168cd640b85b1a2d3e5c55f520cd15` |
| Generated/stages.json | `6e66c8b0abe2dcc3b5297bf08a98953929b7e17b31e740569910255f7601fc26` |
| Generated/strings.json | `e191ce6773c2e608831f4f38bbb1b995bdd2caec3c156693cdbe71e50645828c` |
| Generated/ads.json | `d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a` (동일) |

[검증 결과](C:/Github/StageGame/GameData/Validation/rank-ui-results.json), [변경 전후 문구·기존 Stage 필드 증거](C:/Github/StageGame/GameData/Validation/rank-ui-changes.json). 생성물 세 파일을 함께 복사한다. AI 승리/패배 대사 참조를 서로 바꾸거나 다른 계급 대사를 지정하면 exporter가 거부한다. 새 참조의 누락, 여러 줄/숫자 인자, 별 범위·6단계 경계 오류도 검사한다.

QA 스냅샷으로 Python 테스트를 실행할 때 **GameData/Validation/rank-ui-changes.json**을 동일 상대 경로로 포함해야 한다. 기존 audit_openings.py와 opening-repairs.json 의존성도 유지한다. 실제 Unity 저장 복원·UI 표시·첫 실행·되돌리기·AI 대사 출력과 Android/WebGL 실행은 Client/QA에서 별도 확인한다.
