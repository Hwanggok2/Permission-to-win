# StageGame 클라이언트 구현 기록

최종 체크포인트: **2026-10-14, 호스트 상단 예약·자체 도형 UI·작은 라이선스 메뉴 및 본문30 적용. Unity 전체49/49 + 경계8/8 + 최종 화면1/1 통과(고유50개). 최종 양 플랫폼 개발 빌드 성공(오류/경고0), 총괄 브라우저 본문30 최종 재시험 통과, 소스 동결**. Unity **6000.3.15f1**. 프로젝트 루트는 `C:\Github\StageGame\Client`.

## 완료 결과

7×6 중력식 4목의 오프라인 AI 스테이지 초기 버전이다. **30개 스테이지, 한국어/영어 설정과 즉시 전환·저장·복원, 저장된 씬·프리팹, Unity 전체49/49·경계8/8·최종 화면1/1 검사, Android 개발 APK, 일반 WebGL 개발 빌드**를 확보했다. IAP와 서버는 계획 상태다.

| 항목 | 확인 결과 | 증거 |
|---|---|---|
| Unity 컴파일·회귀 | 전체49/49 + 최종 경계8/8 + 본문 확대 후 화면1/1. 고유50개 통과, 단일50개 실행 아님 | `Evidence/unity-tests.xml`, `host-ui-boundary-tests.xml`, `host-ui-final-render.xml` |
| Android 개발 APK | 성공, BuildReport 오류/경고 각각 0, APK 35,559,979 bytes | `Builds/Android/StageGame-development.apk`, `Evidence/android-build-summary.txt`, `Logs/android-host-ui-final.log` |
| 일반 WebGL 개발 빌드 | 성공, BuildReport 오류/경고 각각 0 | `Builds/WebGL/index.html`, `Evidence/webgl-build-summary.txt`, `Logs/webgl-host-ui-final.log` |
| 최신 한영 PlayMode 화면 | 실제 저장 씬 실행·카메라 렌더 720×1280, 영어 전환과 재실행 후 해금/언어 복원 | `Evidence/ko-lobby.png`, `ko-settings.png`, `ko-match.png`, `en-lobby.png`, `en-settings.png`, `en-match.png`, `en-restored.png` |
| 추가 화면·UI 감사 | 영어 도움말·일시정지·결과, 버튼 영역 및 색 대비 계산 | `Evidence/en-help.png`, `en-confirm.png`, `en-result.png`, `ui-ux-audit.json` |
| 독립 코어 QA | 29묶음 통과/0실패. Unity API 대체 harness이며 실제 기기 검증 아님 | `Docs/QA/Evidence/qa-l10n-core-v1-20261014/run.log` |
| 최종 호스트 브라우저 | 총괄 실행 PASS. 본문24 기능 검사와 최종 본문30 스크롤/닫기/resize 재시험을 분리 기록 | `Evidence/host-ui-verification.json`; 총괄 `Docs/Management/Evidence/host-ui-browser-20261014/README.md` |
| 이전 독립 브라우저 QA-008 | 새로고침 없이 390→320→390, 설정 크기·닫기 입력·언어 유지 통과. error/warn 없음 | `Evidence/layout-reflow-check.json`; 총괄 `Docs/Management/Evidence/localization-browser-20261014/final-*` |
| 이전 빌드 브라우저 기준선 | 모바일 폭 잘림 수정 및 9수 승리→2/30 해금→새로고침 복원 통과. 한영 변경 전 결과 | `Evidence/responsive-check.json`, `webgl-browser-match.jpg`, `verification.json`의 `rootIndependentRetest` |
| 최종 데이터 동기화 | 3개 JSON이 GameData/Generated와 SHA-256 일치 | `Evidence/verification.json` |
| 배포 고지 포함 | APK ZIP의 Notices 항목 및 WebGL StreamingAssets 확인 | `Evidence/apk-notices.json`, `Assets/StreamingAssets/Notices/` |

**실기기 Android 설치·플레이, 앱인토스 SDK/.ait 패키징, 실제 광고 송출, 릴리스 서명, 스토어 제출/승인은 미검증·미완료다.** 현재 산출물은 개발 빌드이며 출시 승인이나 수익 연동 완료를 뜻하지 않는다.

## 실행

- Unity Hub에서 `Client`를 Unity 6000.3.15f1로 열고 `Assets/StageGame/Scenes/Main.unity`를 Play한다.
- Android APK는 ARM64, 패키지 `com.stagegame.prototype`, 버전 `0.1.0`/1, min SDK 25, target SDK 36으로 생성했다. 앱 식별자와 서명은 출시 전에 확정해야 한다.
- WebGL은 `file://` 대신 HTTP 서버로 실행한다. 예: `python -m http.server 8765 --bind 127.0.0.1 --directory C:\Github\StageGame\Client\Builds\WebGL` 후 `http://127.0.0.1:8765/` 접속.
- `StageGamePortrait` 템플릿의 캔버스와 배경은 viewport 전체를 채운다. Unity 내부 720×960 콘텐츠만 OS/호스트 안전영역 안에 맞춘다. 모바일 브라우저 실기기 성능은 측정하지 않았다.

## 한국어·영어 구현과 이전 UI 검사

- `LOCALIZATION-REQUEST.json`으로 필요한 70개 UI 문자열을 먼저 공유했고, 총괄의 중계와 `GameData/LOCALIZATION-CONTRACT.md`로 계약을 확정했다. 기존 key/ko 58개를 보존하고 새 UI 키 65개를 추가한 **123개 ko/en**, 기존 `schemaVersion:1`을 사용한다. stages/ads는 변경하지 않았다. Excel 생성물을 Sync 도구로 복사했으며 JSON을 수동 편집하지 않았다.
- 설정의 저장된 한국어/English 버튼으로 화면 문구를 즉시 갱신한다. 선택 표식은 ✓이며 언어명은 양쪽 언어에서 모국어로 유지한다. `ProgressData.language`를 저장 v1에 추가했고, 구형 저장·누락·알 수 없는 값은 해금/소리 설정을 보존하며 ko로 복귀한다. 영어 선택 후 씬 재실행에서 영어와 2/30 해금을 함께 복원하는 PlayMode 회귀를 통과했다.
- 정적 문구는 AppRoot의 직렬화된 `LocalizedLabel[]`, 동적 제목·턴·수·결과·힌트·오류는 동일 `StringTable` 키를 사용한다. 저장된 모든 Text는 키 바인딩 또는 명시적 동적 필드에 포함되는지 검사한다. 숫자 형식은 코드에서 준비하고 문자열은 `{0}`, `{1}`만 사용한다. 법적 원문·개발 로그·문자열 데이터 자체를 읽지 못했을 때 최소 복구 안내만 예외다.
- WebGL 로딩/오류/캔버스 설명도 빌드 시 Excel 출력에서 `ui-text.js`로 생성한다. 언어 브리지는 실제 선택을 다음 로딩 화면에도 전달하며 게임 내 설정의 기준은 PlayerPrefs다.
- 설정의 SDK/수익/Made with Unity/폰트 설명을 제거하고 라이선스 전문 메뉴를 유지했다. NoFillAds에서는 로비·결과 배너와 결과의 광고 상태 문구를 숨긴다. 실패·취소·저장 안내는 짧게 표시한다. 민트/산호 및 돌의 나/You·AI 문자 표식을 유지했다.
- 기존 프리팹을 `LoadPrefabContents`로 제한 수정했다. 언어 버튼도 공통 PrimaryButton의 저장된 중첩 인스턴스이며 공통 CloseButton을 유지한다. 런타임에서 UI 오브젝트를 만들지 않는다. 호스트 개선 이전 기준 100×100 및 320px 보드 열 39.05px의 제약은 아래 새 구성에서 수정했다. 현재 일반 버튼/닫기는 110단위, 보드 열은 720/7단위이며 최종 CSS 크기는 호스트 절의 지원 범위에 따른다.
- 긴 영어 제목 검사에서 `Review connections and defense`의 잘림을 발견했다. 로비 제목 폭 580/글자 32, 대국 제목 글자 28로 조정한 뒤 두 언어의 30개 제목을 모두 검사했다. 설정·로비·결과·일시정지 버튼 겹침과 영어 본문 높이 검사를 통과했다.
- 불투명 색상 기준 대비는 본문/패널 13.20:1, 보조문구/패널 6.18:1, 짙은 글자/민트 12.01:1, 흰 글자/보조버튼 10.09:1이다. 비활성/장식 요소는 제외한 계산이며 전체 접근성 인증이나 사람 사용성 시험이 아니다.

## 호스트 영역·자체 그래픽·라이선스 메뉴

- 계약 v4에 따라 WebGL 콘텐츠 상단은 **OS 상단 inset + 64 CSS px** 아래에 둔다. 64는 StageGame 설계 여유이며 공식 Toss 버튼 높이나 실기기 측정값이 아니다. 총괄이 확인한 [공식 Safe Area 문서](https://developers-apps-in-toss.toss.im/documentation/common/screen/safe-area), 확인일 2026-10-14. 실제 Toss SDK는 아직 연결하지 않았다.
- HTML 캔버스/배경은 전체 viewport를 유지하고 CSS에서 inset을 빼지 않는다. `HostLayout.ContentArea`는 각 변의 Unity OS safe inset과 브리지 CSS inset 중 큰 값을 취한 후 상단 64를 한 번 추가한다. CSS→Unity 변환은 각 축의 실제 Screen/viewport 비율을 사용한다. 선택적 실제 호스트 컨트롤 바닥 좌표가 있으면 `max(OS top+64, controlsBottom+8)`을 쓴다. Android 단독 앱에는 추가64를 적용하지 않는다.
- 입력 경계는 `window.stageGameHostLayout.setInsets({left,top,right,bottom}, actualControlsBottom=-1)`이다. 단위는 CSS px, OS inset은 각 viewport 가장자리로부터 거리이며 controlsBottom은 viewport 상단부터의 거리다. future adapter가 `SafeAreaInsets.get()`과 `subscribe()` 결과를 전달한다. OS inset 대신 이미 예약한 전체 높이를 넣거나 외부 wrapper가 viewport를 다시 줄이면 안 된다. `host-layout.js`가 CSS env를 기본값으로 사용하며 명시적 값은 이를 대체한다.
- 모의 막대는 `?hostPreview=1&hostTop=59&hostBottom=34`에서만 표시한다. 일반 WebGL은 막대가 숨겨져 있어도64를 예약한다. 모의36px 버튼은 실제 Toss UI의 치수나 작동을 재현한 것이 아니며 포인터를 가로채지 않는다. 실제 SDK/호스트 확인과 분리한다.
- 논리 콘텐츠를 720×960으로 줄이고 일반 버튼·닫기를 최소110×110, 보드7열을720/7 폭으로 저장했다. **320×568 / OS top59·bottom34·left0·right0 / 추가64**의 가용 영역은320×411이며 일반 버튼47.094px, 열 입력44.036px다. 390×844의 동일 inset에서는 각각59.583px/55.714px다. 모든 모달·화면의 활성 그래픽 상하 경계와 각 버튼의44px를 렌더 좌표로 검사했다. 사용 가능한 폭308px 미만 또는 높이410.667px 미만이면 보드44px를 충족하지 못할 수 있어 검증 지원 범위 밖이다. 가로/실기기 사용성은 미검증이다.
- `SoftUiBuilder.Apply`는 기존 저장 프리팹의 배치·스프라이트 참조만 수정하며 런타임 UI 트리를 생성하지 않는다. `Art/SoftUI/RoundRect96.png`(9-slice border26)와 `SoftDisc128.png`는 코드로 그린 원본 도형이다. 둥근 패널·버튼·보드와 민트/산호 돌, 기존 문자 표식을 통일했다. 이전 `Disc.png`를 보존하고 기존 새 자산이 있으면 픽셀을 덮어쓰지 않는다. 생성 코드·파일 해시·참조·출처는 `Evidence/soft-ui-assets.json`에 기록했다.
- 라이선스 메뉴는 설정 하단의26단위 보조 텍스트이며 배경만 투명한535×110 Button이다. 클릭 영역은 줄이지 않았다. 총괄 브라우저 가독성 피드백에 따라 본문만24→30단위(최소 지원 화면에서12.844 CSS px)로 키웠다. 기존8개 고지 파일과 묶음 전문을 유지하고 ScrollRect 마지막 문단까지 렌더했다. `Evidence/host-en-320-notices-bottom.png`는 렌더 타깃 변경 후 스크롤 끝을 다시 맞춘 최종 이미지다.
- 새 Unity 전체 회귀 **49/49**에서 한국어 제목의 높이 부족을 수정한 뒤 통과했다. QA-009의 NaN/Infinity·JS float32 초과 입력은0으로 정규화하고64 예약은 유지한다. 이 마지막 변경 뒤 관련 Unity 검사 **8/8**을 실행했다. 두 실행의 **고유 테스트50개**가 통과했으며 단일50개 실행으로 기록하지 않는다. XML은 `Evidence/unity-tests.xml`, `host-ui-boundary-tests.xml`; 로그는 `Logs/host-ui-tests.log`, `host-ui-boundary-tests.log`다. 최종 본문30 변경 후 호스트 렌더·스크롤 검사1/1을 추가 실행해28개 캡처를 갱신했다(`Evidence/host-ui-final-render.xml`, `Logs/host-ui-final-render.log`). 변경 없는 코어·데이터는 마지막 경계 수정 후 반복하지 않았다.
- `Evidence/host-{ko|en}-{320|390}-{lobby|settings|match|help|confirm|result|notices-bottom}.png` 28개는 저장 씬을 실행한 실제 Unity 카메라 렌더다. 390→320→390 재배치, 두 언어의30개 제목, 버튼 겹침·크기·상단 예약·라이선스 스크롤·중첩 닫기 프리팹을 검사했다. 브라우저 검증은 총괄이 실행했다. 본문24 빌드의 기능 검사는 전체 캔버스/상단123 영역, 390→320→390 설정 크기·영어 유지, 라벨 밖(52,440) 라이선스 클릭, 보드 가장자리 입력·AI 응수, 쉼·도움말을 통과했다. 본문30 최종 빌드를 다시 로드한 뒤320×568에서 진입·본문 확대·55회 드래그 후 마지막 uGUI 문단·닫기 및390 복귀를 통과했다. 관찰 콘솔은 `[]`이고 재로드 후 한국어·2/30 진행 유지, 기본 실행의 모의 표시 숨김도 보고됐다. 첫 기능 검사와 최종 본문 검사는 서로 다른 빌드의 범위이며 새 스테이지 승리를 재검증했다고 기록하지 않는다.
- 출처는 총괄 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`의2026-10-14 보고와 [최종 브라우저 기록](../Docs/Management/Evidence/host-ui-browser-20261014/README.md), `final-artifacts.json`, `final-*` 캡처다. Client는 보고/최종 이미지/빈 콘솔을 읽고5개 최종 산출물의 SHA-256·크기가 현재 파일 및 Client 명세와 모두 일치함을 확인했다. 별도 브라우저를 실행하지 않았다. Unity의44px 좌표 검사와 총괄의 실제 포인터 검사를 함께 사용하며 실제 Toss SDK·휴대폰 시험으로 확대하지 않는다. 이번 마감은 문서만 갱신했고 코드·빌드·기존 테스트를 반복하지 않았다.

## 구조·자료구조 감사

- 42칸 `int[]`와 7열 높이 배열은 작은 고정 보드에 적합하며 착수 위치를 바로 구한다. 별도 그래프나 범용 보드 계층을 추가하지 않았다.
- 123개 문자열은 키 기반 Dictionary를 한 번 구성하고 언어 변경 시 기존 Text를 갱신한다. 화면 수가 적어 기존 GameApp의 직렬화 참조와 명시적 갱신으로 충분하며 전역 이벤트 버스나 MVVM 프레임워크를 추가하지 않았다.
- `IAdService`는 플랫폼 경계를 나누고 RewardGate의 요청 토큰은 늦은/중복 콜백을 거부한다. Status와 저장 경고는 UI 문자열 키로 변환했다. 실 SDK가 연결됐다는 뜻은 아니다.
- 결과 중복 거부에는 저장 List와 조회 HashSet을 사용한다. 완료 판 ID가 계속 쌓이므로 장기 저장 크기/압축 정책은 후속 검토 대상이다.
- AI는 기존 iterative deepening/alpha-beta 및 노드·시간 제한을 유지한다. 분기마다 Board.Clone, Score 객체, iterator가 할당되므로 GC 부담 가능성이 있다. 실기기 할당량·프레임 p95를 측정하기 전 최적화 완료로 주장하지 않으며 이번에 풀링·undo 보드 등으로 재작성하지 않았다.

## 이전 QA-008 크기 변경 결함과 수정 이력

- 총괄의 독립 브라우저 검사에서 390→320×568 전환 직후 설정 프레임이 약 232px, 닫기가 약 36px로 축소되고, 새로고침하면 약 284px/44px로 복구되는 결함을 확인했다. Canvas DOM은 이미 319.5×568이었으므로 HTML 폭 제한과 별개의 Unity 내부 배율 문제였다.
- 설치된 uGUI의 CanvasScaler는 `preWillRenderCanvases`에서 배율을 갱신한다. 기존 SafeAreaFitter.Update가 그 이전의 rect를 읽고 Screen 크기만 캐시해 이후 실제 rect 변경을 놓쳤다. 저장 씬에서 Screen은 고정한 채 Canvas 배율을 바꾸는 회귀로 수정 전 **기대 0.375 / 실제 1.0** 실패를 재현했다 (`Evidence/layout-before.xml`).
- SafeAreaFitter를 `Canvas.willRenderCanvases`에서 실행하고 Screen/safe-area와 별도로 실제 가용 rect 크기도 추적한다. OnDisable에서 이벤트를 해제하며, 가용 크기가 같으면 배율을 다시 쓰지 않는다. 프리팹·게임·언어·광고 로직은 변경하지 않았다.
- 수정 후 같은 검사에서 내용 배율이 **1→0.375→1**로 정상 복귀했고 전체 Unity 회귀 **42/42**를 통과했다 (`Logs/layout-final-tests.log`, `Evidence/unity-tests.xml`, `Evidence/layout-reflow-check.json`). Android와 WebGL도 최종 재빌드에서 오류/경고 각각 0으로 성공했다.
- 총괄 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`의 2026-10-14 최종 보고: `webgl-layout-final`을 로드한 뒤 390px 설정 프레임 약 346px/닫기 54px→새로고침 없이 320px에서 프레임 284px/닫기 44px→English 선택→390px 복귀에서 동일한 크기와 영어 유지→X로 로비 복귀를 확인했다. error/warn은 빈 배열이며 한국어로 복귀하고 뷰포트 설정을 복원했다. **QA-008 최종 독립 재시험 통과**로 기록한다. 직접 증거는 `Docs/Management/Evidence/localization-browser-20261014/final-ko-settings-390.jpg`, `final-en-settings-320.jpg`, `final-en-settings-390-resized.jpg`, `final-console.json`이다. 이 항목은 총괄의 실제 브라우저 보고이며 클라이언트의 별도 실행이나 실제 모바일 기기 검증으로 표기하지 않는다.

## 게임과 코드

- `ConnectFourBoard`: 7열×6행, 행 0은 바닥. 플레이어 1/AI -1. 가로·세로·양 대각선 4개 이상 승리, 만판 무승부, 만열/범위 밖/종료 후 착수 거부.
- `AiSearch`: 즉승과 피할 수 있는 즉패 방어를 우선하고 iterative deepening negamax/alpha-beta를 사용한다. 실수율도 즉승·즉패 방어를 무시하지 않는다. 원본 보드는 수정하지 않는다.
- AI는 깊이/노드 예산 외에 프레임 약 4ms, 전체 활성 사고시간 650ms의 시험 상한을 둔다. 전체 시간은 프레임 사이 대기를 포함하고 일시정지 시간을 제외한다. 매 16노드 단위로 검사하며 중단 시 마지막 완료 깊이의 합법 수를 사용한다. 250ms의 별도 연출 대기가 있다. 하드 실시간 보장이나 실기기 p95 수치가 아니다.
- 이번 Editor 배치 smoke에서 최고 예산의 빈 판 AI 응답은 연출 대기를 포함해 약 280ms였다. 이는 단일 개발 PC 검사이며 모바일 성능으로 일반화하지 않는다.
- `GameData`: JsonUtility wrapper 계약 v1을 읽고 정확히 30개, 연속 ID/챕터/문자열 참조/AI 범위/시작측/합법 시작 수열을 검사한다. 실패하면 로비에 명시적 오류를 표시하고 시작 버튼을 막는다.
- `GameApp`: 로비/스테이지 탐색→대국→결과→재도전/다음 스테이지, 일시정지·로비 복귀, 승리 줄과 패배 방향, 효과음·설정·도움말·고지를 연결한다.
- `ProgressStore`: 게임 데이터와 별도 저장 버전 1, 최고 해금/완료 판수/설정/광고 간격/처리한 판 ID를 PlayerPrefs로 저장한다. 비연속 A→B→A 및 재실행 후 같은 결과 중복 처리를 막는다. 저장 실패는 경고를 표시하고 현재 플레이/결과 화면 이동을 계속 허용한다.
- 서버/로그인/계정/결제 없이 대국과 해금을 진행한다.

## 실제 저장 UI

씬: `Assets/StageGame/Scenes/Main.unity`.

| 저장 프리팹 | 역할 |
|---|---|
| `Prefabs/AppRoot.prefab` | Canvas, SafeArea, GameApp의 모든 직렬화 참조 |
| `UI/Common/CloseButton.prefab` | 결과/설정/도움말/확인/고지에서 중첩 재사용 |
| `UI/Common/PrimaryButton.prefab` | 공통 행동 버튼 |
| `UI/Common/ModalFrame.prefab` | 배경·제목·공통 닫기 버튼 |
| `UI/Common/StageCard.prefab` | 스테이지 정보 |
| `UI/Screens/LobbyScreen.prefab`, `GameScreen.prefab` | 로비 및 대국 화면 |
| `UI/Screens/ResultModal.prefab`, `SettingsModal.prefab` | 결과 및 설정 |
| `UI/Screens/HelpModal.prefab`, `ConfirmModal.prefab`, `NoticesModal.prefab` | 도움말·일시정지 확인·스크롤 가능한 라이선스 전문 |
| `Board/Board.prefab`, `Board/Cell.prefab` | 저장된 42칸 및 7개 열 입력 영역 |

위 경로는 `Assets/StageGame/Prefabs/` 기준이다. 런타임에는 저장된 UI를 바인딩·활성화하며 UI 트리를 생성하지 않는다. 단순 원형 이미지와 착수 효과음은 Editor 도구에서 최초 생성해 파일로 저장했다.

`ProjectBuilder.Create`는 존재하는 프리팹을 재생성하지 않는다. 초기 누락 스프라이트/SafeArea 참조 보완과 고지 버튼 추가는 `LoadPrefabContents`/`UnloadPrefabContents`를 사용한 제한된 수정으로 수행했다. 모든 공통 CloseButton이 원본 프리팹을 중첩 참조하는지 검사한다.

## 광고 상태와 제한

- **실제 SDK나 광고 계정 ID는 연결하지 않았다.** 플랫폼 공통 `IAdService` 계약과 기본 `NoFillAds`만 있다. 기본 상태에서는 광고 수익이 발생하지 않는다.
- `EditorTestAds`는 `UNITY_EDITOR`에서만 컴파일한다. 성공/실패/취소/중복 콜백 검증을 위한 도구다.
- 힌트는 자발적 요청, 성공 콜백 1회/판만 지급한다. 미준비/실패/취소는 무보상이며 재도전은 무료다.
- 리워드 대기 중 보드와 중복 요청을 잠근다. 6초 응답 제한, 모달/백그라운드/판 이동 취소, 늦은·중복 콜백 거부 후 진행 복구를 검사했다.
- 전면 광고 기회는 첫 3판 제외, 마지막 실제 표시 후 2판 및 90초 조건을 모두 만족하는 결과 경계에서만 처리한다. 4초 응답 제한 후 결과 버튼을 복구한다.
- 향후 실제 SDK 어댑터는 `CancelPending()`에서 콜백뿐 아니라 **늦은 광고 표시 자체**도 취소해야 한다. 현재 no-fill 검증이 실제 SDK의 수명주기 검증을 대신하지 않는다.

## 다음 MVP 범위: 광고 제거 IAP (미구현)

2026-10-14 총괄이 전달한 사용자 결정으로 광고 제거 IAP를 다음 구현 범위에 추가했다. **이번 한영/UI 작업에 결제 구현은 포함하지 않았다. 결제 기능은 구현·연동·검증되지 않았다.**

- 상품 기획안: **3,900원 1회 결제**, 전면·배너 광고 제거 및 리워드 힌트 광고 면제. 구매자의 힌트도 **판당 1회**로 제한한다.
- 판매 노출 조건: 원스토어 광고 미연결 버전에서는 구매 버튼을 숨긴다. 아직 제공하지 않는 광고의 제거 상품을 판매하지 않는다.
- 구현 예정: 플랫폼별 결제 어댑터, 서버 검증을 거친 구매 권리 적용, 구매 복구, 저장된 Unity 프리팹 기반 구매/복구 UI. 로컬 값만으로 구매 권리를 확정하지 않는다.
- QA 예정: 구매·복구 성공/실패, 환불 후 권리 갱신, 광고 제거 적용, 판당 힌트 제한, 광고 미연결 버전의 판매 버튼 비노출을 검증한다. 아직 수행하지 않았다.
- 최신 총괄 계약은 v4이며 서버 계획은 **Cloudflare Workers Free + D1 Free + Secrets/mTLS**다. 무료 한도 내 운영 방향이고 실제 토스 mTLS·소유권·SDK 검증 및 배포는 미실행이다. 상세 조건은 `Server/README.md`를 따른다. JSON schema v1 및 오프라인 대국 구조는 유지한다.

## 한영 개선 이전의 검증·수정 이력

- 390×844 뷰포트에서 기존 높이 우선 CSS의 캔버스가 474.75×844로 커져 오른쪽 버튼이 잘리는 문제를 수정했다. 안전영역을 뺀 가로·세로 공간의 최솟값으로 크기를 제한하고 grid 최소 크기를 0으로 설정했다. 실제 브라우저 DOM 측정은 390×844→390×693.33, 320×568→319.5×568, 1280×720→405×720이며 모두 9:16 및 화면 내부 경계를 유지한다. 390×844에서 설정 버튼과 닫기 버튼도 포인터로 검증했다. 조회한 브라우저 오류·경고는 없었다. 이는 데스크톱 IAB의 뷰포트 검사이며 실기기 검사와 구분한다.
- 총괄의 최종 독립 브라우저 재시험은 390×844에서 캔버스 390×693.333, x=0/y=75.333, 로비·7열 보드·결과 화면 잘림 없음을 확인했다. 0 기준 열 수순 `P3 AI3 P3 AI3 P1 AI1 P2 AI1 P4`로 1스테이지 9수 승리 후 '연결 성공' 결과→로비 2/30 해금→새로고침 후 STAGE 02 '두 돌 잇기' 및 2/30 복원을 확인했다. 조회한 console error/warn은 빈 배열이며 뷰포트 설정도 복원했다. 이 항목은 총괄 채팅 `01a138d9-7ede-70d0-ba36-e84a05e6b105`의 2026-10-14 전달 결과를 기록한 것으로, 클라이언트의 별도 재실행이나 실제 모바일 기기 검사가 아니다. QA007은 클라이언트 수정 및 총괄 직접 재시험을 통과했으며 QA 문서 종결은 해당 담당 영역에서 반영한다.
- 반응형 수정 후 WebGL 재빌드는 오류/경고 0으로 성공했다. WASM·data·Android APK 해시는 이전 검증본과 동일하고 WebGL index.html만 변경됐다. 이번 변경은 템플릿 CSS이며 기존 Unity 30/30 결과를 보존하고 브라우저 검사를 추가했다. Android 재빌드나 Unity 전체 테스트 재실행은 하지 않았다.
- 규칙 4방향 승리/만열/종료 잠금/복사 불변성, AI 즉승·방어·예산, 30개 데이터 재생 및 합법 AI 수, 잘못된 데이터, 광고 실패·취소·중복·간격, 결과 저장 중복, 저장 프리팹 참조를 검사했다.
- EnterPlayMode 테스트로 실제 저장 UI의 클릭, AI 응수, 결과/다음 스테이지, 로컬 해금 복구, 광고 대기/시간 초과/모달/백그라운드/판 전환, 모의 중복 보상, 최고 AI 예산을 검사했다.
- 라이선스 X는 `EventSystem.RaycastAll`에서 최상위 입력 대상으로 판정되고 PointerClick 경로로 닫히는 회귀 검사를 통과했다. 실제 1280×720 브라우저에서도 관찰된 X 중심 `(783,85)` 클릭 후 설정 복귀를 확인했다. 별도 보고된 `(783,118)`은 이 빌드에서 버튼 아래였으며 상세 증거는 `Evidence/notices-close-check.txt`, `webgl-notices-close.jpg`다.
- 총괄도 동일 빌드에서 `(783,85)`로 즉시 닫힘을 독립 재확인하고 최초 보고를 좌표 판독 오류로 정정했다. 제품 결함으로 남기지 않으며 이 정정 때문에 코드 변경·재빌드는 수행하지 않았다. 최종 회귀 30/30, 런타임 소스·세 JSON·빌드 파일 해시 일치를 마지막으로 재확인했다.
- QA 독립 harness v4는 21개 통과/0개 실패 (`Docs/QA/Evidence/qa-core-v4-20261014/run.log`). 저장 실패 주입 등은 Unity 대체 API 검사이며 실제 기기 증거와 구분한다.
- 초기 테스트에서 A→B→A 결과 중복, 재실행 후 중복, 저장 예외 시 결과 화면 정지, 1개 스테이지 잘림 허용을 수정했다.
- 초기 캡처의 사각형 돌은 SpriteImportMode.Single과 누락 참조를 보완했다. 흐린 글자는 작은 GameView에서 생성된 Text 캐시를 캡처 해상도로 다시 생성해 수정했다.
- 첫 일반 WebGL 빌드는 컴파일 성공했지만 브라우저에서 자홍색 화면이 나왔다. 새 프로젝트의 `m_AlwaysIncludedShaders`가 비어 있어 런타임 기본 uGUI 셰이더가 누락된 문제였다. `UI/Default`, `Sprites/Default`를 명시 포함하고 실제 브라우저에서 로비·착수·AI 응수를 재검증했다. 임시로 시도했던 Editor 전용 `GUI/Text Shader`는 플레이어에 포함하지 않는다.
- 최초 sandbox Unity 실행은 라이선스 IPC 접근 실패로 중단했다. 자신이 생성한 Client 배치 프로세스만 경로 검증 후 종료하고 승인된 배치 실행으로 검증했다. 별도 SimpleGame 에디터는 조작하지 않았다.

## 최종 데이터 및 산출물 식별

10/12/20/22/24/26 스테이지의 수정 국면을 포함한 최종 데이터다.

| 파일 | SHA-256 |
|---|---|
| `stages.json` | `2D04B124A42F0F3798AD942E9AACAB97EE34A25F05B380144CD70F75C3904992` |
| `ads.json` | `D19F181FAF52DB28287BBED1B42291770DC3BF83D35E5CCA8AF4A6DCCE9E194A` |
| `strings.json` (한영 123개) | `ED5FD27463724F4789B2F5E96922316240F73BF73EBC9D663410F0758E0883C2` |
| Android APK (35,559,979 bytes) | `05E70261507378EEC397416566567802B49FB1173BC58A4F22A72488B3EE4B8D` |
| WebGL WASM | `FD86D320DB92967BBE3DDF6FFB9CA06C26120223E3034753413070792C53D581` |
| WebGL data | `72BBB0E061B59AD7EE3F72EF8C34A76F60B7B4CAEDA67184A2EFEB8B51D53407` |
| WebGL index.html | `53BDBF4813EA2CFE5A19E5C21B3B711CD6AD08AD1200254146D1AB3BA3C6C3CA` |
| WebGL ui-text.js | `2D958DA273B7A6ACC2DFBD37EE24B034F0A82078A7BCA7755858A41C33A70FC8` |

전체 산출물·런타임 소스 해시는 `Evidence/verification.json`에 기록했다. 빌드 파일/Library/Logs는 Git 제외 대상이며, 작은 검증 요약/XML/이미지는 Evidence에 별도로 남겼다. 커밋/푸시는 수행하지 않았다.

## 재현 명령

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe'
$project = 'C:\Github\StageGame\Client'
& "$project\Tools\SyncGameData.ps1"
# 최초 누락 자산만 생성; 기존 사용자 프리팹 재생성 금지
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.ProjectBuilder.Create -quit -logFile "$project\Logs\create.log"
# 기존 프리팹의 한영 키 바인딩/입력 영역 변경을 재적용할 때만 실행
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.LocalizationBuilder.Apply -quit -logFile "$project\Logs\localization-prefabs.log"
# 호스트 UI는 LocalizationBuilder 이후 적용하며 기존 자산을 보존한다
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.SoftUiBuilder.Apply -quit -logFile "$project\Logs\host-ui-prefabs.log"
# 테스트에는 -quit를 추가하지 않는다
& $unity -batchmode -projectPath $project -runTests -testPlatform EditMode -testResults "$project\Logs\editmode-results.xml" -logFile "$project\Logs\tests.log"
& $unity -batchmode -nographics -projectPath $project -buildTarget Android -executeMethod StageGame.Editor.ProjectBuilder.BuildAndroid -quit -logFile "$project\Logs\android-build.log"
& $unity -batchmode -nographics -projectPath $project -buildTarget WebGL -executeMethod StageGame.Editor.ProjectBuilder.BuildWebGL -quit -logFile "$project\Logs\webgl-build.log"
```

## 사용 자산과 기존 프로젝트 조사

- Noto Sans CJK KR Regular 공식 OTF를 사용했다. 폰트 SHA-256은 `6BCB2A0703AA137E874FC2DFFA85F6C21BA9A67FA329E81B8C801663AF7E992A`다. Adobe 저작권 및 OFL 원문, uGUI 저작권과 Unity Companion License 전문, 개발 도구 고지를 함께 포함했다.
- 설정→오픈소스 라이선스에서 고지를 스크롤해 읽을 수 있다. 원문은 `Assets/StreamingAssets/Notices`, UI용 묶음은 `Resources/LegalNotices.txt`다. `Tools/PrepareNotices.py`는 내려받은 공식 UCL HTML과 로컬 고지 원문을 묶는다.
- 공식 출처: https://github.com/notofonts/noto-cjk/blob/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf 및 https://unity.com/legal/licenses/unity-companion-license . 고지 포함은 사용자의 엔진 계약/계정 자격 확인을 대신하지 않는다.
- 설치 조사: 2022.3.62f3는 Android/WebGL 모듈 없음, 6000.3.15f1은 양쪽 모듈과 SDK/NDK/JDK 있음, 6000.5.9f1은 WebGL만 있음.
- SimpleGame ProjectVersion은 6000.3.15f1, uGUI 2.0.0, Input System 1.19.0, URP 17.3.0, 앱인토스 SDK release/v3.5.0이었다. 구조만 읽고 복사/수정하지 않았다. SimpleGame의 OneStore 어셈블리는 WebGL용이므로 원스토어 Android 광고 구현으로 재사용하지 않았다.
- StageGame은 uGUI 2.0.0, Test Framework 1.6.0 및 필요한 Unity 기본 모듈만 사용한다. 다른 담당 폴더를 수정하거나 사용자 자산을 덮어쓰지 않았다.
