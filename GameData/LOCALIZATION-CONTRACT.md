# 한국어·영어 문자열 계약

현재 단계별 서사 참조와 418개 문자열 구성은 2026-10-15 사용자 승인 [TOURNAMENT-CONTRACT.md](TOURNAMENT-CONTRACT.md)를 따른다. ko/en 저장·전환과 포맷 인자 규칙은 유지하며, 아래 이전 집계·해시는 당시 검증 이력이다.

2026-10-15 임시 개발 표시명 4목 / Four in a Row 반영. 현재 **237개 ko/en**, 10계급×3단계다. 최신 제목 키·해시는 [TITLE-V10-CONTRACT.md](C:/Github/StageGame/GameData/TITLE-V10-CONTRACT.md), 대사 키·Stage 1 예외는 [STORY-CONTRACT.md](C:/Github/StageGame/GameData/STORY-CONTRACT.md), 확인 패널·토스트 키는 [V9-UI-CONTRACT.md](C:/Github/StageGame/GameData/V9-UI-CONTRACT.md)를 따른다. 데이터 담당은 Client/를 수정하지 않는다.

- Excel: 기존 `ExportType, SearchKey, Content_Kor, /Description`을 보존하고 마지막에 `Content_Eng`(string/Client) 추가.
- JSON: 기존 wrapper와 `schemaVersion:1`, `key/ko/en`을 유지한다. 예: `{"schemaVersion":1,"strings":[{"key":"game_title","ko":"4목","en":"Four in a Row"}]}`. 임시 개발 표시명이며 상점명 확정이나 권리 확보 선언이 아니다. `game_brand`, `client_game_title`도 같은 표기, `web_canvas_label`은 4목 게임 / Four in a Row game이다.
- 모든 USE 행에 ko/en 필수. 승인된 문구 변경은 전후 값과 사유를 기록한다. 언어 설정은 `ko`, `en`, 기본 `ko`; 잘못된 저장 언어는 ko로 복귀. 누락 번역은 exporter 오류로 처리.
- 포맷은 C# `string.Format`에 맞춘 `{0}`, `{1}` 등 숫자 인자만 허용. `{{`/`}}`는 리터럴 중괄호. 양쪽 언어의 인덱스 집합 일치·0부터 연속·괄호 문법을 검사. 정렬/형식 지정자·이름 인자는 이번 계약에서 제외.
- v9 이전 228개 중 191개 보존·37개 수정 + 확인 패널/서사 신규 9개 = 237개다. 단계 설명 30개는 보존하고 기존 AI 대사 30개를 프로 소문을 듣고 대결을 신청하는 서사로 교체했다. 일반 대사는 최대 ko 23자/en 54자, 지정 첫 시작 대사는 원문 그대로 ko 30자/en 61자다. 말풍선 1–2줄 표시를 허용하며 이전 한 줄/22·41자 목표보다 사용자 원문을 우선한다.
- `story_premise`와 `story_first_intro/win/lose` 4개를 추가했다. Stage 1의 3개 대사 필드만 전용 키를 참조하고 훈련병 2/3은 기본 계급 대사를 쓴다. `aiWinKey`는 AI 승리/플레이어 패배, `aiLoseKey`는 플레이어 승리/AI 패배다. 첫 지정 한국어 3개는 테스트로 정확히 검사한다.
- 플레이어는 프로 출신 훈련병이며 계급명+1/2/3 제목은 도전자의 계급을 나타낸다. `rank_label`과 `rank_progress`, 부제를 같은 서사로 수정했다. 별은 설계상 진행 표시이며 사람에게 검증된 난이도 등급을 뜻하지 않는다. 실제 군인 진급이나 성과 주장을 추가하지 않는다.
- 나가기/다시하기 확인 문구 4개와 `ui_cancel`을 추가했다. `retry/ui_retry/stage_replay`는 다시하기/Restart, `confirm_leave`는 나가기/Leave다. 진행/종료 대국 모두 전용 본문을 사용한다. 차례 토스트는 기존 `ui_player_turn`(내 차례/Your turn)을 재사용하며 인자는 없다.
- 작업명·흑돌/백돌 범례와 선택→미리 보기→놓기 안내를 반영했다. 민트/산호와 열 클릭 즉시 착수 안내는 생성물에 없다. 기존 방향별 패배 문구에서 삭제했던 금색 줄 안내도 복원하지 않았다. 기존 챕터 키와 포맷 인자 개수는 유지하고 새 화면은 계급 UI 키를 쓴다.
- 최초 `Client/LOCALIZATION-REQUEST.json`의 70개 요청은 패배 문구·계급 UI·[v9](C:/Github/StageGame/GameData/Validation/v9-ui-changes.json)·[서사](C:/Github/StageGame/GameData/Validation/story-changes.json)·[임시 표시명](C:/Github/StageGame/GameData/Validation/title-v10-changes.json)의 승인 변경을 순서대로 적용해 대조한다. 추가 문구도 Excel/JSON과 일치하는지 검사한다. 변경 명세는 검증용이며 콘텐츠 원본은 Excel이다. 설정 언어명 `한국어`, `English`와 기존 포맷 계약을 유지한다.
- stages.json의 기존 8개 필드·ID·시작판·AI 값은 보존했다. 이번에는 Stage 1의 대사 참조 3개만 변경했고 다른 29개 Stage는 전체 필드가 같다. ads.json은 바이트가 동일하다. 실제 저장 복원·언어 UI·말풍선 표시는 Client/총괄 브라우저 검증 범위며 실물 폰 시험은 보류한다.

상태: 최종 Excel → JSON 반복 생성 바이트 동일 및 `--check` 성공, Python **44개 테스트 통과**. `Generated/strings.json` SHA-256: `4fa6c54394fd59846131794852ea025d4fa7dcb6725ab2a1dfb7435f1fa50de4`. [최종 통합 보존·해시·문구 길이 증거](C:/Github/StageGame/GameData/Validation/story-results.json). 클라이언트가 세 JSON을 함께 sync할 수 있다.
