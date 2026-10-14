# MVP 확정 계약 v1

확정: 2026-10-14 총괄. 기획 권고 `Docs/Design/DECISION.md`와 구현 사전 조사를 반영.

## 게임

- 내부 작업명 StageGame. 상점용 이름은 권리 확인 후 확정한다.
- 7열 × 6행 중력식 4목. 열을 눌러 가장 낮은 빈 칸에 돌을 놓는다. 가로/세로/대각선으로 4개 이상 연결하면 승리. 합법 수 없는 가득 찬 판은 무승부.
- 플레이어=1, AI=-1, 빈 칸=0. 열 인덱스 0~6. 턴 교대, 가득 찬 열 불가. 승리/무승부 후 추가 착수 금지.
- 오프라인 AI 싱글플레이, 30개 초기 스테이지(3챕터 × 10). 첫 구현은 1판→결과→다음 스테이지 루프까지.
- 자유 오목/렌주/물리 알까기/온라인 대전/계정/랭킹/결제는 MVP 범위 밖.
- 중간 국면은 시작측부터 합법적으로 재생 가능한 열 순서로 표현한다. 재생 중 이미 종료되는 배치 금지. 불가능/떠 있는 돌 배열 금지.
- AI는 깊이/노드 예산/실수 비율로 난이도 변화. 즉시 승리/즉시 패배 방어 우선, 임의 난수로 치팅하지 않는다. 깊이/시간은 실측 후 조정.
- 판당 45~120초는 목표이고 측정 전 보장하지 않는다. 손실 원인 표시/전술 국면은 차별화 가설.

## 프로젝트와 UI

- Unity 6000.3.15f1, 프로젝트 루트 `Client/`. 설치된 Android 및 WebGL 모듈 활용.
- 세로 화면 720×1280 기준, 작은 화면/안전영역 대응. 한국어 텍스트와 자체 단순 도형 아트. 폰트 재배포 권리 검증.
- 씬 `Client/Assets/StageGame/Scenes/Main.unity`. 공통 CloseButton/PrimaryButton, 로비, 보드, 결과, 설정·도움말 모달을 저장된 프리팹으로 제작한다.
- 최초 생성 에디터 도구 허용. 이미 생성된 사용자 프리팹을 재생성/덮어쓰지 않는다. 런타임 UI 트리 조립 금지.

## 데이터 교환

- Excel 원본/메타행 관례는 `GameData/FORMAT.md`를 따른다. 데이터 담당은 원본→검증→JSON exporter를 제공한다.
- 생성 위치 `GameData/Generated/stages.json`, `ads.json`, `strings.json`.
- 클라이언트 담당이 읽고 자신의 `Assets/StageGame/Resources/GameData/`에 복사/임포트한다. 데이터 담당이 Client/에 쓰지 않는다.
- JSON은 Unity JsonUtility 호환 wrapper object 사용. 필드 이름은 아래처럼 고정. 문자열 UTF-8.

```json
{"schemaVersion":1,"stages":[{"id":1,"chapter":1,"titleKey":"stage_001","aiDepth":1,"aiNodeBudget":1000,"aiRandomPermille":250,"starter":1,"openingMoves":""}]}
```

- `id`: 1~30 고유 정수; `chapter`: 1~3; `titleKey`: strings에 존재; `aiDepth`: 1~6; `aiNodeBudget`: 100~30000; `aiRandomPermille`: 0~1000; `starter`: 1 또는 -1; `openingMoves`: 쉼표로 구분된 0~6 열 인덱스 또는 빈 문자열.
- runtime validation 실패 시 명확한 오류와 안전한 로비 복귀. 잘못된 데이터를 정상 데이터처럼 무시하지 않는다.

```json
{"schemaVersion":1,"interstitialGraceMatches":3,"interstitialEveryMatches":2,"interstitialCooldownSeconds":90,"bannerLobby":true,"bannerResult":true,"rewardedHintsPerMatch":1}
```

```json
{"schemaVersion":1,"strings":[{"key":"stage_001","ko":"첫 수"}]}
```

## 광고와 진행

- 첫 3판 전면광고 제외. 이후 최소 2판 및 90초를 모두 충족하는 대국 종료 경계에서만 시험. 결과 행동 버튼 활성화 전 광고 기회를 처리해 다음 판 중 늦게 표시되는 일이 없어야 한다.
- 배너: 로비/결과 예약 공간만. 대국/일시정지/앱 재진입에서는 전면광고 금지.
- 리워드: 자발적 선택 힌트 1회/판. 미준비/취소/실패면 무보상. 성공 1회만 지급. 무제한 무료 재도전을 광고 시청으로 막지 않는다.
- 실제 광고 SDK/계정 ID가 없으면 로컬 테스트 어댑터를 명시하며 수익 연동 완료로 보고하지 않는다.
- 첫 프로토타입은 광고 준비 안 됨(no fill)을 기본으로 하고 Editor에서만 모의 완료/실패/중복 테스트 가능.
- 진행도 최고 해금 스테이지를 로컬 저장. 게임 데이터와 저장 버전 분리. 동일 결과/보상 중복 처리 방지.

## 이번 구현 완료 증거

- 실제 저장된 Scene/Prefab과 플레이 가능한 규칙/AI/진행 코드.
- 규칙/AI/데이터/광고 상태 테스트 결과. Unity 컴파일 및 가능하면 EditMode 테스트 증거.
- Android/WebGL 산출물은 실제 빌드 성공 때만 완료 표기. 광고 승인/스토어 승인/실기기 검증은 각각 별도 상태.
