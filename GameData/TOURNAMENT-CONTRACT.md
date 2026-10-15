# 부대 대국 24단계 + 병사 대회 6단계

2026-10-15 사용자 선택 및 Client/GameData 적용·Unity 검증 범위 승인. 콘텐츠 원본은 `GameData_MVP.xlsx`이며 JSON은 exporter로만 생성한다. 기존 계급 순서 계약의 진행 배치는 이 문서로 대체한다.

## 단계와 캐릭터

| 단계 | 상대 | 이야기 |
| --- | --- | --- |
| 01~12 | C01~C12 각 1회 | 프로 출신 입대자의 소문과 부대 대국 |
| 13~24 | C13~C18 각 2회 | 중대장·작전참모·대대장·연대장·부사단장·사단장 |
| 25 | C02 이병 문시온 | 64강 |
| 26 | C03 일병 오준호 | 32강 |
| 27 | C04 상병 정한결 | 16강 |
| 28 | C05 병장 남기석 | 8강 |
| 29 | C19 일병 조은찬 | 4강 |
| 30 | C01 이병 강민재 | 결승 |

대회 상대는 병사만 참가한다. C01은 훈련병 시절과 같은 인물이며 대회에서는 이병으로 표시한다. 대국 승리가 군 진급을 뜻하지 않는다. 사단장과의 첫 승리인 23단계는 재대국을, 두 번째 승리인 24단계는 대회를 예고한다. 30승리 때 우승 문구를 표시하고 다음 단계 버튼은 숨긴다. 이전 30단계 완료 저장값을 삭제하거나 우승 결과를 자동 재생하지 않는다.

## Excel·런타임 연결

- Stages R열 `CharacterId`: `string/Client`, JSON `characterId`. C01~C19와 위 배치를 exporter가 검사한다.
- L열 `RankKey`: `story_opponent_001`~`story_opponent_030`. 계급·군종 또는 보직·이름, 대회 라운드를 포함한 표시 문자열이다.
- M열 `DescriptionKey`: `story_stage_desc_001`~`story_stage_desc_030`.
- O~Q열: `story_002_intro/win/lose`~`story_030_intro/win/lose`. `win`은 AI 승리, `lose`는 AI 패배다.
- 1단계만 기존 `story_first_intro/win/lose`와 지정 문구를 그대로 사용한다.
- `action_to_round_64`: 24승리 후 다음 버튼. `story_champion`: 30승리 결과 제목.
- 기존 269개 Strings ko/en을 보존하고 149개를 추가해 총 418개다. 사용자 화면 문구를 코드에 새로 하드코딩하지 않는다.

Stage ID·titleKey·chapter·별점·광고 설정은 유지한다. 2026-10-15 추가 피드백으로 초반 시작판과 AI 수치를 완화했으며 현재 수치는 [난이도 계약](DIFFICULTY-CONTRACT.md)을 따른다. 별은 1~8, 반 개 단위이며 같은 값으로 두 단계씩 진행한다. `schemaVersion:1`과 저장 키·해금 의미를 유지한다. 새 데이터의 `characterId` 소비자 코드를 함께 배포한다.

각 인물의 intro/thinking/defeat Sprite는 저장된 RankTheme 참조로 연결한다. AI 승리·무승부에는 defeat 그림을 쓰지 않고 재도전 시 intro로 복원한다. C06 하사는 아래 방향 V 표식으로 고친 cap-v2를 사용한다. 전 인물 설정·대사는 [CHARACTERS.md](../Docs/Design/CHARACTERS.md), 적용 검증은 [PIXEL-INTEGRATION.md](../Docs/Design/PIXEL-INTEGRATION.md)를 따른다.

## 검증 증거

`Client/Evidence/pixel-art-20261015/workbook-validation.json`은 기존 269개 문자열·30개 수순/AI 설정과 서식 보존 비교다. Python 자동 검사 45개와 exporter `--check`를 실행한다. Unity 테스트·화면 결과는 같은 Evidence 경로에 별도로 기록하며, 에디터 검증을 실기기·웹/Android 빌드 검증으로 간주하지 않는다.
