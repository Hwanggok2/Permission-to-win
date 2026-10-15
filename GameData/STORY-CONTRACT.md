# v9 대결 서사 데이터 계약

후속 사용자 승인(2026-10-15)의 현재 서사는 [TOURNAMENT-CONTRACT.md](TOURNAMENT-CONTRACT.md)를 따른다. 부대 대국 24단계 뒤 병사 대회 64강~결승 6단계이며 인물별 이름·대사를 사용한다. 이 문서는 v9 이력이다. 첫 대국 지정 문구 3개는 현재에도 그대로 보존한다.

2026-10-14 총괄 승인. 상태: **v9 확인 패널+서사 통합 완료, Excel/JSON·보존·재현성·44개 테스트 검증 완료, Client 동기화 가능**. 플레이어는 사회에서 이 게임의 프로였고, 입대 후 훈련병으로 지낸다. 프로라는 소문을 들은 동기·선임들이 대결을 신청한다. 단계의 계급은 **도전자 계급**이며 승리로 플레이어의 실제 군인 계급이 오르는 설정이 아니다.

## Client 연결

| 키 | 한국어 | 영어 | 연결 |
| --- | --- | --- | --- |
| story_premise | 사회에서는 4목 프로, 오늘부터는 훈련병. | A four-in-a-row pro in civilian life, a recruit today. | 짧은 서사 요약, 별도 긴 모달 없음 |
| story_first_intro | 이 게임의 프로라고 했지? 그런데 내가 이길 것 같아. | You said you were a pro at this game? I think I can beat you. | Stage 1 aiIntroKey |
| story_first_win | 프로라는 말, 거짓말이었나? | Was that pro story just a bluff? | Stage 1 aiWinKey: AI 승리 / 플레이어 패배 |
| story_first_lose | 프로는 역시 다르네. 다음엔 내가 이긴다. | A pro really is different. Next time, I win. | Stage 1 aiLoseKey: 플레이어 승리 / AI 패배 |

기존 `ai_rank_01_intro/win/lose`는 훈련병 2·3단계가 사용한다. Stage 2–30은 기존 계급 대사 키를 유지한다. Stage 1의 3개 대사 참조만 예외이며 exporter가 해당 ID와 역할을 명시 검증한다. `rankKey`, `descriptionKey`, `difficultyStars`, 모든 기존 8개 Stage 필드·시작판·AI 설정은 그대로다.

계급별 `ai_rank_XX_intro/win/lose` 30개 문구를 전부 교체한다. 흐름은 훈련병 동기의 호기심 → 이등병의 프로 소문 → 일병의 빈틈 시험 → 상병의 전략 → 병장의 부대 강자 자신감 → 하사의 차분한 대결 → 중사의 몇 수 앞 → 상사의 경험 → 소위의 도전 → 중위의 최종 승부다. AI가 이기면 장난스러운 도발, 플레이어가 이기면 인정과 다음 승부를 기약한다.

`game_subtitle`, `rank_label`({0}=도전자 계급명), `rank_progress`({0}=도전자 계급 번호, {1}=전체 계급 수)를 도전자 흐름으로 수정한다. 첫 지정 한국어 대사는 축약하지 않는다. 대사는 값에 강제 줄바꿈을 넣지 않고 UI 말풍선에서 1–2줄로 표시한다. 이전 ko 22자/en 41자 목표는 이 대사에 적용하지 않으며 일반 대사는 가급적 ko 35자/en 65자 이내다. 단계 설명 30개는 보존한다.

확인 패널·다시하기 라벨·차례 토스트는 [V9-UI-CONTRACT.md](C:/Github/StageGame/GameData/V9-UI-CONTRACT.md)를 따른다. 실물 폰 시험은 보류하며 실제 대사 역할·말풍선 잘림은 Client/총괄 브라우저 검증 범위다.

## 최종 데이터와 검증

v9 시작 전 228개 중 **191개 보존·37개 수정 + 신규 9개 = 총 237개 ko/en**이다. 서사만 보면 직전 233개 중 200개 보존·33개 수정·4개 추가다. 일반 계급 대사 30개의 최대 길이는 ko 23자 / en 54자, 지정 첫 시작 대사는 ko 30자 / en 61자다. 첫 지정 한국어 3개를 정확히 보존하는 회귀 검사를 추가했다. 설명 30개, 시작판·규칙·AI 설정·광고·ID·해금/완료 저장 의미는 그대로다.

Stage 1의 `aiIntroKey/aiWinKey/aiLoseKey`만 바꾸고 다른 29개 Stage 전체와 나머지 Stage 1 필드를 비교해 보존했다. 기존 Excel 서식·크기·고정창·입력 유효성도 보존했다. 변경 범위 렌더, 반복 export 바이트 동일, `--check`, **Python 44개 테스트 통과**. 런타임 브라우저 실행과 실제 저장 복원은 데이터 담당 검증에 포함하지 않는다.

| 파일 | SHA-256 |
| --- | --- |
| GameData_MVP.xlsx | `92330d2a7f963f37fcc4e0a1c524888da94fef2e190c2aecd6409e0360befa11` |
| Generated/stages.json | `6f76d1575f71fd0240cf7b5774bd2a3fed41fb1828bd0cb85464f34e72122e7e` |
| Generated/strings.json | `4fa6c54394fd59846131794852ea025d4fa7dcb6725ab2a1dfb7435f1fa50de4` |
| Generated/ads.json | `d19f181faf52db28287bbed1b42291770dc3bf83d35e5cca8af4a6dcce9e194a` (동일) |

[전체 승인 대사 목록·전후 값·Stage 참조 변경](C:/Github/StageGame/GameData/Validation/story-changes.json), [v9+서사 통합 검증](C:/Github/StageGame/GameData/Validation/story-results.json). 콘텐츠 원본은 Excel이며 위 JSON은 검증 기록이다. Client는 생성물 세 파일을 함께 동기화한다. `verify_localization.py`를 별도 스냅샷에서 실행할 때는 기존 승인 명세와 `v9-ui-changes.json`, `story-changes.json`도 같은 Validation 경로에 포함한다.
