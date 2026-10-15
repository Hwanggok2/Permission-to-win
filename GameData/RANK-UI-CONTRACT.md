# 계급 UI 데이터 계약

현재 계약(2026-10-15 후속 승인)은 [TOURNAMENT-CONTRACT.md](TOURNAMENT-CONTRACT.md)다. 부대 24 + 대회 6단계, 캐릭터 19명, 문자열 418개를 사용한다. `RankKey/DescriptionKey/Ai*Key`는 단계별 서사 키이며 `CharacterId`를 추가했다. 아래는 이전 계급 개편 기록으로, 배치·참조 키·문자열 개수의 현재 기준으로 쓰지 않는다.

현재 계약은 2026-10-15 사용자 승인 기준으로 18개 계급 표시·반 별 단위 최대 8개·269개 문자열이다. 확인 패널 키는 [V9-UI-CONTRACT.md](C:/Github/StageGame/GameData/V9-UI-CONTRACT.md), Stage 1 지정 대사는 [STORY-CONTRACT.md](C:/Github/StageGame/GameData/STORY-CONTRACT.md)를 유지한다. 아래 과거 생성 해시는 이전 상태의 기록이다. 계급은 도전자의 표시이며 플레이어는 훈련병으로 유지한다.

임시 표시명은 `4목 / Four in a Row`이며 상점명 확정이나 권리 확인을 뜻하지 않는다. 2026-10-15에는 사용자가 Client 세션의 GameData 원본 수정까지 승인했다.

## Stage 추가 필드

JSON wrapper와 schemaVersion 1, ID 1–30·titleKey·chapter·openingMoves·starter를 유지한다. AI 설정은 난이도 곡선에 맞게 변경한다. Excel Stages L:Q의 UI 필드는 다음과 같다.

| Excel | JSON | 형식/의미 |
| --- | --- | --- |
| RankKey | rankKey | 문자열 참조. `rank_01`–`rank_18`, 처음 6계급은 1단계씩, 이후 12계급은 2단계씩 |
| DescriptionKey | descriptionKey | `stage_desc_001`–`stage_desc_030`, 해당 시작판에 맞는 한 줄 |
| DifficultyStars | difficultyStars | float 1–8, 0.5 단위. ID 1–2는 1, 3–4는 1.5, …, 29–30은 8 |
| AiIntroKey | aiIntroKey | `ai_rank_01_intro`–`ai_rank_18_intro`, 시작 때 AI 대사 |
| AiWinKey | aiWinKey | `ai_rank_01_win`–`ai_rank_18_win`, **AI 승리 / 플레이어 패배** 대사 |
| AiLoseKey | aiLoseKey | `ai_rank_01_lose`–`ai_rank_18_lose`, **플레이어 승리 / AI 패배** 대사 |

2026-10-15 사용자 승인 계급 표시 순서: 훈련병, 이등병, 일병, 상병, 병장, 하사, 중사, 상사, 원사, 준위, 소위, 중위, 대위, 소령, 중령, 대령, 준장, 사단장. 게임 진행을 위한 계급·직책 명칭이며 실제 진급 경로를 뜻하지 않는다. `stage_001`–`stage_030` 키를 유지하고 같은 계급의 두 단계만 표시값에 1·2를 붙인다. 기존 `chapter`는 저장/구버전 호환을 위한 3×10 값으로 남긴다. AI 깊이·노드 예산은 감소하지 않고 무작위 선택 비율은 증가하지 않도록 조정했다. 별은 기획한 난이도이며 사람 플레이로 검증된 등급은 아니다.

## 신규 UI 키

아래 키를 Excel Strings에서 ko/en으로 제공한다. 숫자 포맷은 기존 `{0}` 방식 그대로다.

- 인자 없음: `undo`(되돌리기), `place`(놓기), `retry`(재도전), `select_column`(놓을 열을 고른 뒤 '놓기'를 눌러요), `multiplayer_start`(대국 시작), `coming_soon`(준비중), `nextstage`(다음 단계), `first_match_title`, `first_match_body`, `stage_list_title`, `stage_detail_title`, `undo_unavailable`, `undo_done`, `multiplayer_title`, `multiplayer_unavailable`.
- 인자 1개: `stage_label`({0}=전체 단계 번호), `rank_label`({0}=번역된 계급명), `difficulty_stars`({0}=1–8, 반 개 허용), `selected_column_accessibility`({0}=사용자 표시 열 번호 1–7).
- 인자 2개: `rank_progress`({0}=현재 계급 번호 1–18, {1}=전체 계급 수 18).
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
