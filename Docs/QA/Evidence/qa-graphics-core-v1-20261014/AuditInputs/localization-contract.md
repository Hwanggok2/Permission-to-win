# 한국어·영어 문자열 계약

2026-10-14 총괄 승인 및 `Client/LOCALIZATION-REQUEST.json`의 소비자 수락 확인. 기존 한국어 전용 표를 확장했다. 데이터 담당은 Client/를 수정하지 않는다.

- Excel: 기존 `ExportType, SearchKey, Content_Kor, /Description`을 보존하고 마지막에 `Content_Eng`(string/Client) 추가.
- JSON: 기존 wrapper와 `schemaVersion:1`, 기존 key/ko를 보존하고 각 항목에 `en` 추가. 예: `{"schemaVersion":1,"strings":[{"key":"game_title","ko":"StageGame","en":"StageGame"}]}`.
- 모든 USE 행에 ko/en 필수. 승인된 문구 변경은 전후 값과 사유를 기록한다. 언어 설정은 `ko`, `en`, 기본 `ko`; 잘못된 저장 언어는 ko로 복귀. 누락 번역은 exporter 오류로 처리.
- 포맷은 C# `string.Format`에 맞춘 `{0}`, `{1}` 등 숫자 인자만 허용. `{{`/`}}`는 리터럴 중괄호. 양쪽 언어의 인덱스 집합 일치·0부터 연속·괄호 문법을 검사. 정렬/형식 지정자·이름 인자는 이번 계약에서 제외.
- `Client/LOCALIZATION-REQUEST.json`의 70개 요청을 반영했다(기존 키 5개, 신규 키 65개). 기존 58개를 보존·번역해 총 123개 ko/en을 제공한다. 설정 언어명은 양쪽 언어에서 `한국어`, `English`로 표시한다. formatArgCount는 요청 검증용이며 JSON에는 내보내지 않는다.
- 후속 단계 선택/그림 도움말 요청 14개를 `Strings!A128:E141`에 추가했다. 기존 120개 보존 + 방향별 패배 안내 3개 승인 수정 + 신규 14개 = 현재 총 137개다. `stage_map_chapter`, `stage_cleared_progress`는 인자 2개, `stage_lock_requirement`는 1개이며 나머지 신규 키는 0개다. 요청 문구를 그대로 반영했다.
- 총괄의 실제 브라우저 검사에서 불투명 결과 팝업 뒤 금색 줄을 보라는 안내를 확인해, `result_loss_horizontal/vertical/diagonal`의 둘째 줄만 ko/en에서 삭제했다. 변경 셀은 C101:C103·E101:E103이며 나머지 기존 값과 전체 서식·크기·고정창·입력 유효성을 보존했다. [승인 전후 문구](C:/Github/StageGame/GameData/Validation/approved-string-revisions.json)는 Client 최초 요청의 해당 3키를 대체하는 검증 기준이다.
- `stage_tier_1/2/3`의 입문/Basics, 전술/Tactics, 도전/Challenge는 각각 1–10/11–20/21–30 스테이지의 챕터 표시명이다. 사람 플레이로 검증된 난이도 등급을 뜻하지 않는다.
- stages.json, ads.json과 스테이지/광고 데이터는 보존한다. Client 소비/복사와 실제 언어 UI·저장은 클라이언트 담당 범위다.

상태: Excel → JSON 반복 생성 바이트 동일 및 `--check` 성공, Python 36개 테스트 통과. `Generated/strings.json` SHA-256: `57e392cf68464898829946f17030e4ae3008aab86370452bacf6dbe56d68a291`. [14개 추가·보존·해시 증거](C:/Github/StageGame/GameData/Validation/stage-map-strings-results.json). 클라이언트가 세 JSON을 함께 sync할 수 있다. UI 전환·로컬 언어 저장·WebGL 문구 생성·실기 확인 완료는 클라이언트 증거를 따른다.
