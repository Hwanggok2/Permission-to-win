# v10 임시 개발 표시명 인계

2026-10-15 총괄 지시에 따라 개발 화면 표시명을 **4목 / Four in a Row**로 변경했다. 최종 상점 브랜드 확정이나 권리 확보를 뜻하지 않는다. Excel 원본과 데이터 생성·검증은 완료했으며 Client 반영·브라우저 확인은 별도 단계다. 이번 인계에서는 **웹·Android 빌드를 실행하지 않는다**.

## 변경한 표시명

| 키 | Excel 행 | ko | en |
| --- | --- | --- | --- |
| game_title | 35 | 4목 | Four in a Row |
| client_game_title | 63 | 4목 | Four in a Row |
| game_brand | 64 | 4목 | Four in a Row |
| web_canvas_label | 127 | 4목 게임 | Four in a Row game |

기존 UI 제목 별칭 `client_game_title`의 영어는 이미 같아 보존했다. 변경 셀은 Strings!C35/E35/C63/C64/E64/C127/E127의 **7개**다. 237개 키와 ko/en 구조를 유지하며 추가·삭제는 없다. 나머지 233개 문자열, 부제·첫 원문·계급별 승패 대사·단계 설명을 보존했다. 30개 Stage 전체 필드와 광고 JSON은 직전 v9와 바이트가 같다. 이름 이외의 v10 UI 변경은 Client 범위다.

## Client 소스 반영 위치

- `Generated/`의 세 JSON을 함께 `Client/Assets/StageGame/Resources/GameData/`로 동기화한다. 이번에 내용이 달라진 파일은 strings.json이다.
- `RankGameApp.cs`의 로비·대국 제목은 `game_title`을 읽는다. `RankUiBuilder.cs`가 저장하는 제목과 기존 `LocalizationBuilder.cs`/`AppRoot.prefab`의 `game_brand`, `client_game_title` 바인딩도 확인한다. 저장된 프리팹은 기존 비파괴 편집 경로로 갱신한다.
- `LocalizationBuilder.ExportWebStrings()`로 `Assets/WebGLTemplates/StageGamePortrait/ui-text.js`를 갱신할 수 있다. 이 메서드는 문자열 소스 생성이며 WebGL 빌드를 호출하지 않는다. `game_title`은 브라우저 탭 제목과 로딩 제목, `web_canvas_label`은 캔버스 접근성 이름에 쓰인다.
- `LocalizationTests.cs`의 `game_title` 한국어·영어 기대값도 새 값으로 맞춘다. 테스트 실행과 실제 UI 확인 결과는 Client가 별도로 남긴다.
- 내부 프로젝트명 `StageGame`, 네임스페이스·저장 키·앱 식별자 변경은 이번 데이터 변경에 포함하지 않았다. 웹 템플릿의 최초 `PRODUCT_NAME` 값은 ProjectSettings에서 오므로 사용자 화면에 노출되는 시점이 있는지는 Client가 소스 기준으로 확인한다.

위 경로는 읽기 전용으로 조사했으며 데이터 담당은 Client 파일을 수정하거나 Unity/브라우저를 실행하지 않았다. 기존 빌드 산출물은 이번 데이터 변경으로 갱신되지 않는다.

## 검증과 동기화 해시

Artifact Tool로 기존 서식을 유지하며 7개 셀만 수정했다. 변경 범위 전후 렌더를 확인했고 수식 오류 검색은 0건이다. 저장된 Excel의 나머지 값·타입·서식·크기·고정창·입력 유효성은 직전 스냅샷과 일치한다. `--check`, 반복 export 바이트 일치, **Python 44개 테스트 통과**를 확인했다. 실제 게임 화면·폰 플레이·빌드는 이 검증에 포함하지 않는다.

| 파일 | SHA-256 |
| --- | --- |
| GameData_MVP.xlsx | `f2def3d974982b9361040c3b3e242e9fe3cfcbe72737212b782355b2b56401a1` |
| Generated/strings.json | `9519b8ea2dfa94e45ac1273226ffb0a95634779d3de9d02d6817f17c9e6f74f1` |
| Generated/stages.json | `6f76d1575f71fd0240cf7b5774bd2a3fed41fb1828bd0cb85464f34e72122e7e` (동일) |
| Generated/ads.json | `d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a` (동일) |

[변경 전후 값](C:/Github/StageGame/GameData/Validation/title-v10-changes.json), [검증 결과](C:/Github/StageGame/GameData/Validation/title-v10-results.json). 이전 문구 승인 이력 뒤에 title-v10 변경을 적용하도록 `verify_localization.py`를 갱신했다. 별도 QA 스냅샷에는 기존 승인 명세들과 `Validation/title-v10-changes.json`도 포함한다. 서사·Stage 1 대사 예외는 [STORY-CONTRACT.md](C:/Github/StageGame/GameData/STORY-CONTRACT.md)를 따른다.
