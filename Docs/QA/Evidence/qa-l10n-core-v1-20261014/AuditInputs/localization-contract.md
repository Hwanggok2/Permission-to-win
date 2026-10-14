# 한국어·영어 문자열 계약

2026-10-14 총괄 승인 및 `Client/LOCALIZATION-REQUEST.json`의 소비자 수락 확인. 기존 한국어 전용 표를 확장했다. 데이터 담당은 Client/를 수정하지 않는다.

- Excel: 기존 `ExportType, SearchKey, Content_Kor, /Description`을 보존하고 마지막에 `Content_Eng`(string/Client) 추가.
- JSON: 기존 wrapper와 `schemaVersion:1`, 기존 key/ko를 보존하고 각 항목에 `en` 추가. 예: `{"schemaVersion":1,"strings":[{"key":"game_title","ko":"StageGame","en":"StageGame"}]}`.
- 모든 USE 행에 ko/en 필수. 기존 키의 한국어는 그대로 보존. 언어 설정은 `ko`, `en`, 기본 `ko`; 잘못된 저장 언어는 ko로 복귀. 누락 번역은 exporter 오류로 처리.
- 포맷은 C# `string.Format`에 맞춘 `{0}`, `{1}` 등 숫자 인자만 허용. `{{`/`}}`는 리터럴 중괄호. 양쪽 언어의 인덱스 집합 일치·0부터 연속·괄호 문법을 검사. 정렬/형식 지정자·이름 인자는 이번 계약에서 제외.
- `Client/LOCALIZATION-REQUEST.json`의 70개 요청을 반영했다(기존 키 5개, 신규 키 65개). 기존 58개를 보존·번역해 총 123개 ko/en을 제공한다. 설정 언어명은 양쪽 언어에서 `한국어`, `English`로 표시한다. formatArgCount는 요청 검증용이며 JSON에는 내보내지 않는다.
- stages.json, ads.json과 기존 한국어/스테이지/광고 데이터는 보존한다. Client 소비/복사와 실제 언어 UI·저장은 클라이언트 담당 범위다.

상태: Excel → JSON 생성 및 `--check` 성공, Python 36개 테스트 통과. `Generated/strings.json` SHA-256: `ed5fd27463724f4789b2f5e96922316240f73bf73ebc9d663410f0758e0883c2`. 클라이언트가 세 JSON을 함께 sync할 수 있다. UI 전환·로컬 언어 저장·WebGL 문구 생성·실기 확인 완료는 클라이언트 증거를 따른다.
