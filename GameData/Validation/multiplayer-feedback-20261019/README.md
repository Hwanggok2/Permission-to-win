# 멀티 결과·레이팅·착수 확인 문자열

확인일: 2026-10-19. 상태: Excel 원본·생성 JSON·Client Resources 문자열 동기화와 데이터 검증 완료. 이 기록은 Unity 화면·프리팹·웹 빌드·멀티 실제 대전 검증을 포함하지 않는다.

`Strings!A507:E513`에 `mp_outcome_win`, `mp_outcome_loss`, `mp_outcome_draw`, `mp_outcome_cancelled`, `mp_rating_result`, `mp_rating_unchanged`, `mp_move_pending` 7개를 추가했다. 기존 502개를 보존해 총 509개다. 레이팅 결과의 인자 `{0}`/`{1}`과 중간 공백 두 개, 착수 확인 문구의 말줄임표를 요청 그대로 보존했다.

- 기존 모든 셀의 값·타입·서식·행/열 크기·고정창·시트 구성·유효성 대조: 변화 0개.
- 기존 502개 문자열의 변경·삭제: 0개. 신규 7개 한영 문구 정확히 일치.
- `stages.json`, `ads.json`, `multiplayer.json` SHA-256 불변.
- `GameData/Generated/strings.json`과 `Client/Assets/StageGame/Resources/GameData/strings.json` 바이트 일치.
- 신규 행의 한영 문구와 인자 표시 렌더를 직접 확인했다.
- exporter `--check`와 자동 검사 47개 통과. 반복 생성·행 순서 변경 시 동일 출력 검사 포함.

[변경 문구](changes.json), [원본 보존 대조](preservation.json), [생성물 대조](output-verification.json), [자동 검사 로그](data-tests.txt), [새 문구 렌더](after.png).

`before.xlsx`, `after.xlsx`와 의존성 연결은 로컬 보조물로 버전 관리에서 제외한다. `GameData_MVP.xlsx`가 유일한 편집 원본이다.
