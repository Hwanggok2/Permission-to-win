# v9 확인 패널·차례 토스트 문자열

**서사까지 통합한 최신 생성물은 237개**이며 [STORY-CONTRACT.md](C:/Github/StageGame/GameData/STORY-CONTRACT.md)의 최종 해시를 사용한다. 아래 확인 패널/토스트 키·문구는 그대로 유지하고, 아래 233개 해시는 서사 추가 직전 검증 기록이다.

2026-10-14 총괄 요청. 상태: **Excel/JSON 작성·보존 검사·반복 생성·42개 테스트 완료, Client 동기화 가능**. 기존 계급·시작판·AI Stage 필드는 그대로다. Client 연결은 클라이언트 담당이다.

| 키 | 한국어 | 영어 | 처리 |
| --- | --- | --- | --- |
| confirm_exit_title | 나가시겠습니까? | Leave this game? | 신규 |
| confirm_exit_body | 이 대국을 나가고 로비로 돌아가요. | Leave this game and return to the lobby. | 신규, 진행/종료 대국 공통 |
| confirm_retry_title | 다시하시겠습니까? | Restart this game? | 신규 |
| confirm_retry_body | 이 대국을 처음부터 다시 시작해요. | Start this game again from the beginning. | 신규, 진행/종료 대국 공통 |
| ui_cancel | 취소 | Cancel | 신규, 두 패널 공통 |
| confirm_leave | 나가기 | Leave | 기존 행동 라벨 수정 |
| retry | 다시하기 | Restart | 기존 행동 라벨 수정 |
| ui_retry | 다시하기 | Restart | 기존 행동 라벨 수정 |
| stage_replay | 다시하기 | Restart | 기존 행동 라벨 수정 |
| ui_player_turn | 내 차례 | Your turn | 기존 키 그대로 재사용, 차례 토스트용 |

모두 포맷 인자 0개다. `client_ui_retry` 키는 현재 원본에 없으므로 새 별칭을 만들지 않고 `retry` 또는 `ui_retry`를 사용한다. `confirm_title/body`는 예전 일시정지 안내용으로 보존하며 v9 나가기/다시하기 패널은 위 전용 키를 쓴다.

로비로/다시하기 버튼은 진행 중과 결과 화면 모두 해당 확인 패널을 열고, 나가기/다시하기 확정에서만 실행한다. 취소는 현재 화면으로 돌아간다. 실제 동작·크게 잠깐 표시 후 사라지는 차례 토스트는 Client 구현/브라우저 검증 범위다. 실물 폰 시험은 사용자 요청으로 보류한다. 얼굴 삭제와 계급 모자 그래픽에 추가 문자열은 없다.

## 확인 패널 생성물 (서사 추가 직전 233개)

이전 228개 중 **224개 보존·4개 승인 수정 + 신규 5개 = 233개 ko/en**이다. 기존 대사·규칙·계급·별·30개 시작판·AI 필드와 광고는 모두 보존했다. 기존 Excel 값은 Strings C/E의 41·108·136·214행만 바뀌고 A233:E237이 추가됐다. 전체 기존 서식·크기·고정창·입력 유효성 보존과 변경 범위 렌더를 확인했다. 신규/수정 문구 및 토스트의 포맷 인자는 모두 0개다.

| 파일 | SHA-256 |
| --- | --- |
| GameData_MVP.xlsx | `e44e73aef15cbc5ae2e65ec74e9c63c42549ca020be2ca6e633e68205ffa1c6c` |
| Generated/strings.json | `1259006d809a1b66d3d585a68ad070f94789bcc4b2a51022bb92096a292809c9` |
| Generated/stages.json | `6e66c8b0abe2dcc3b5297bf08a98953929b7e17b31e740569910255f7601fc26` (동일) |
| Generated/ads.json | `d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a` (동일) |

반복 export 바이트 동일, `--check`, Python **42개 테스트 통과**. [변경 명세](C:/Github/StageGame/GameData/Validation/v9-ui-changes.json), [최종 보존·검증 결과](C:/Github/StageGame/GameData/Validation/v9-ui-results.json). `verify_localization.py`는 이전 승인 변경과 이번 변경을 순서대로 대조한다. 세 JSON을 함께 동기화한다. 실물 폰 시험은 수행하지 않았다.
