# StageGame 클라이언트 구현 기록

최종 체크포인트: **2026-10-14, 모바일 폭 WebGL 수정 및 총괄의 승리·해금·새로고침 복원 재시험 통과, 문서 마감·소스 동결**. Unity **6000.3.15f1**. 프로젝트 루트는 `C:\Github\StageGame\Client`.

## 완료 결과

7×6 중력식 4목의 오프라인 AI 스테이지 초기 버전을 구현했다. **최종 30개 스테이지, 저장된 씬·프리팹, Unity 테스트 30/30, Android 개발 APK, 일반 WebGL 개발 빌드**를 확보했다. WebGL은 실제 로컬 브라우저에서 로비→대국 시작→착수→AI 응수까지 확인했다.

| 항목 | 확인 결과 | 증거 |
|---|---|---|
| Unity 컴파일·회귀 | 30개 통과, 0개 실패 | `Evidence/unity-tests.xml`, `Logs/tests-complete.log` |
| Android 개발 APK | 성공, BuildReport 오류/경고 각각 0 | `Builds/Android/StageGame-development.apk`, `Evidence/android-build-summary.txt`, `Logs/android-verified.log` |
| 일반 WebGL 개발 빌드 | 성공, BuildReport 오류/경고 각각 0 | `Builds/WebGL/index.html`, `Evidence/webgl-build-summary.txt`, `Logs/webgl-responsive.log` |
| 실제 브라우저 실행 | Codex IAB, localhost, 세로 UI·플레이어 착수·AI 응수 확인; 조회한 오류/경고 로그 없음 | `Evidence/webgl-browser-match.jpg` |
| 반응형 화면 | 390×844, 320×568, 1280×720에서 캔버스 경계가 뷰포트 내부; 390×844 설정 열기/닫기 클릭 성공 | `Evidence/responsive-check.json`, `responsive-390x844.jpg`, `responsive-settings-390x844.jpg`, `responsive-320x568.jpg`, `responsive-1280x720.jpg` |
| 총괄 독립 브라우저 재시험 | 390×844에서 1스테이지 9수 승리→2/30 해금→새로고침 후 STAGE 02 및 해금 복원 통과 보고; 오류/경고 없음 | `Evidence/verification.json`의 `rootIndependentRetest` (총괄 보고 출처 명시) |
| 저장 UI 시각 확인 | PlayMode의 실제 카메라 렌더, 720×1280 | `Evidence/lobby.png`, `match.png`, `result.png`, `settings.png`, `help.png`, `notices.png` |
| 최종 데이터 동기화 | 3개 JSON이 GameData/Generated와 SHA-256 일치 | `Evidence/verification.json` |
| 배포 고지 포함 | APK ZIP의 Notices 항목 및 WebGL StreamingAssets 확인 | `Evidence/apk-notices.json`, `Assets/StreamingAssets/Notices/` |

**실기기 Android 설치·플레이, 앱인토스 SDK/.ait 패키징, 실제 광고 송출, 릴리스 서명, 스토어 제출/승인은 미검증·미완료다.** 현재 산출물은 개발 빌드이며 출시 승인이나 수익 연동 완료를 뜻하지 않는다.

## 실행

- Unity Hub에서 `Client`를 Unity 6000.3.15f1로 열고 `Assets/StageGame/Scenes/Main.unity`를 Play한다.
- Android APK는 ARM64, 패키지 `com.stagegame.prototype`, 버전 `0.1.0`/1, min SDK 25, target SDK 36으로 생성했다. 앱 식별자와 서명은 출시 전에 확정해야 한다.
- WebGL은 `file://` 대신 HTTP 서버로 실행한다. 예: `python -m http.server 8765 --bind 127.0.0.1 --directory C:\Github\StageGame\Client\Builds\WebGL` 후 `http://127.0.0.1:8765/` 접속.
- `StageGamePortrait` 템플릿이 화면의 가로·세로 가용 공간에 모두 맞는 9:16 캔버스, 안전영역 여백, 로딩/오류 안내를 제공한다. 모바일 브라우저 실기기 성능은 측정하지 않았다.

## 게임과 코드

- `ConnectFourBoard`: 7열×6행, 행 0은 바닥. 플레이어 1/AI -1. 가로·세로·양 대각선 4개 이상 승리, 만판 무승부, 만열/범위 밖/종료 후 착수 거부.
- `AiSearch`: 즉승과 피할 수 있는 즉패 방어를 우선하고 iterative deepening negamax/alpha-beta를 사용한다. 실수율도 즉승·즉패 방어를 무시하지 않는다. 원본 보드는 수정하지 않는다.
- AI는 깊이/노드 예산 외에 프레임 약 4ms, 전체 활성 사고시간 650ms의 시험 상한을 둔다. 전체 시간은 프레임 사이 대기를 포함하고 일시정지 시간을 제외한다. 매 16노드 단위로 검사하며 중단 시 마지막 완료 깊이의 합법 수를 사용한다. 250ms의 별도 연출 대기가 있다. 하드 실시간 보장이나 실기기 p95 수치가 아니다.
- 최종 Editor 배치 smoke에서 최고 예산의 빈 판 AI 응답은 연출 대기를 포함해 약 300ms였다. 이는 단일 개발 PC 검사이며 모바일 성능으로 일반화하지 않는다.
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

## 검증과 수정 이력

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
| `strings.json` | `F307E1900A7E0CDF72A1B50D33CBFB0EECE4A6F46E69910978286A6CEF2D7752` |
| Android APK (34,676,997 bytes) | `8A809C7B1942E16D3B846721118A49D4E557CD24EA424E66D07C4D7C908C5688` |
| WebGL WASM | `341A44CDABBCBF20FAEF276B248C6C2861CCDDCDC42AACC2F3D79F64BE05C18F` |
| WebGL data | `449794A6FADDA05C4D336993125D1CB67BAC2B59EEA918BB9EF9601E79B800EB` |
| WebGL index.html (반응형 수정) | `2102BD16456F5BBD5788FFAB1107CB611F875B9DC211472218C7044CBAB23102` |

전체 산출물·런타임 소스 해시는 `Evidence/verification.json`에 기록했다. 빌드 파일/Library/Logs는 Git 제외 대상이며, 작은 검증 요약/XML/이미지는 Evidence에 별도로 남겼다. 커밋/푸시는 수행하지 않았다.

## 재현 명령

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.3.15f1\Editor\Unity.exe'
$project = 'C:\Github\StageGame\Client'
& "$project\Tools\SyncGameData.ps1"
# 최초 누락 자산만 생성; 기존 사용자 프리팹 재생성 금지
& $unity -batchmode -nographics -projectPath $project -executeMethod StageGame.Editor.ProjectBuilder.Create -quit -logFile "$project\Logs\create.log"
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
