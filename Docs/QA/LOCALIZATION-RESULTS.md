# 다국어 코어 독립 검증

2026-10-14 KST. 실행 `qa-l10n-core-v1-20261014`: **29묶음 통과, 0실패**(기존 21 + 신규 8). [결과](Evidence/qa-l10n-core-v1-20261014/results.json) · [빌드/실행 로그](Evidence/qa-l10n-core-v1-20261014/run.log) · [소스·검사 도구 해시](Evidence/qa-l10n-core-v1-20261014/source-manifest.json) · [실행 중 변경 확인](Evidence/qa-l10n-core-v1-20261014/post-run-source-check.json).

**최종 상태:** Unity **42/42**, 최종 Android/WebGL·소스 해시 일치, 총괄의 크기 변경 재시험으로 **QA-008 종결**. [최종 대조](Evidence/qa-l10n-integration-final-20261014/results.json). 아래 코어 실행/중간 증거 검토는 당시 기록이며 최종 판정은 마지막 절을 따른다.

명령: `Tests/run-independent.ps1 -RunId qa-l10n-core-v1-20261014 -SkipStageExploration`. 제품 소스·Client JSON을 복사해 고정한 뒤 .NET 10으로 실행했다. 원본 Client/GameData 및 Unity 에디터는 수정·실행하지 않았다. 실행 중 해시 변경은 없었다.

| 범위 | 확인 결과 |
| --- | --- |
| 기존 21묶음 | 규칙·AI·데이터·광고 보상·중복 결과·저장 실패·화면 상태 회귀 통과. 규칙 1,000 시퀀스, AI 1,000국면 포함 |
| 한영 123개 | 각 키의 ko/en 조회 및 ko→en→ko 전환, Client/Generated 문자열 바이트 일치 |
| 기존/신규 UI 분리 | 기존 58개 키/한국어 보존. 요청 70개(신규 65개)의 ko/en·formatArgCount와 실제 출력 일치. 기존 `ui_start`와 변경 `client_ui_start` 분리 |
| 누락·오류 | 영어 필드 누락, 빈 ko/en, 빈/중복 키, 잘못된 스키마·빈 목록 거부. 미등록 UI 키 조회는 명시적 예외이며 키 문자열을 반환하지 않음 |
| 포맷 | 인자 재배열·반복·이스케이프 중괄호 출력 일치. 인자 누락, 언어별 인덱스 불일치, 비연속/이름/형식 지정자/불완전 괄호 거부 |
| 언어 저장/이행 | 기존 schema 1에서 language 누락/null/빈값/unknown은 ko. en 저장·재로드와 해금·효과음·중복 방지 이력 보존 |
| 오류 안내 | 로드/저장 실패·기본 광고 상태가 문구 키로 연결되고 양 언어 조회 가능. 성공적인 재저장 후 실패 경고 해제 |
| 같은 판 변경 | 대체 UI에서 ko↔en 변경 시 보드/턴/수·matchId·사용한 힌트·화면·효과음·완료 판 수 보존. 턴/돌/정적 라벨/힌트 문구 갱신. 새 앱 객체의 언어 복원 확인 |

## 코어 실행 당시 범위와 남은 작업

- StringTable/보드/AI/광고/저장/GameApp의 실제 소스를 실행했으나 Unity JsonUtility·PlayerPrefs·UI·코루틴·시계는 명시적인 대체 구현이다. 실제 Unity의 누락 필드 역직렬화, 디스크/웹 저장 내구성, 브라우저 재접속, 포인터 입력은 별도다.
- UI 문자열 누락은 테이블/소스 조회 수준의 검사다. 실제 저장 프리팹의 모든 바인딩·동적 오류 경로·긴 영문 렌더링과 누락 데이터 화면 처리는 최종 Unity/화면 증거를 기다린다. 실제 플랫폼 호스트의 오프라인도 미검증이다.
- 보드·AI 소스와 현재 Generated `stages.json`·`ads.json`은 기존 경로 증거 입력과 같다. `stages.json`은 `2D04B124A42F0F3798AD942E9AACAB97EE34A25F05B380144CD70F75C3904992`. 변경 없는 30스테이지 승리 경로 탐색은 반복하지 않았다. 과거 core v4 Client 복사본의 해시는 다르므로 경로 입력인 **Generated 스냅샷**과 연결한다.
- 데이터 담당의 36테스트 통과는 제공 보고이며 이번 QA가 재실행한 수가 아니다. [데이터 담당 확인 결과](../../GameData/Validation/localization-results.json)의 123개·요청 70개·보존 정보와 구분한다. 이번 실행은 Excel을 열거나 저장하지 않았다.
- 수정 전 [사전 감사](UI-LOCALIZATION-AUDIT.md)는 보존한다. 언어 처리 코어·문구 연결은 위 범위에서 개선 확인, 실제 UI 가독성/프리팹과 개발 문구 제거의 최종 확인은 대기다. 실기기 AI 할당/성능·사람 플레이·IAP는 미검증을 유지한다.
- 최종 클라이언트 완료 시 이 실행의 소스/데이터 해시와 비교해 변경된 부분만 재검증한다. `L10N01~08` 전체를 통과로 표시하지 않는다.

## Unity·프리팹·화면 후속 검토 — 중간 기록

[증거 스냅샷](Evidence/qa-l10n-integration-v1-20261014/results.json) · [보존 XML](Evidence/qa-l10n-integration-v1-20261014/Client/Evidence/unity-tests.xml) · [보존 verification](Evidence/qa-l10n-integration-v1-20261014/Client/Evidence/verification.json).

- 최초 읽기에서는 **41/41**, 종료 2026-10-14 20:15:49 KST의 Unity XML을 확인했다. 보존 도구 실행 시에는 SafeArea 회귀가 추가된 **42/42**, 종료 20:25:49 KST의 XML로 바뀌었다. 당시 verification은 41검사 및 수정 전 SafeArea 해시를 기록한다. 작업 중 버전 혼합을 명시하고 최종 완료 통보를 기다린다.
- XML의 `SavedPrefabReferencesAndNestedButtonsValidate`, `EverySavedUiTextHasAKeyOrExplicitDynamicOwnerAndButtonsHaveTouchArea`, 한영 30스테이지 제목 검사, 언어 버튼/장면 재로드 검사, 기존 저장 언어 이행 검사는 통과했다. QA가 Unity를 실행한 결과가 아니다.
- AppRoot YAML을 독립 검사해 **번역 바인딩 30개**의 키 존재/로컬 fileID 선언, 언어 버튼·라벨 등 **6개 필드**, **열 라벨 7개**를 확인했다. 실제 Unity GUID 해결·포인터 입력·레이아웃은 이 정적 검사의 범위가 아니다.
- 최초 다국어 빌드 APK·WebGL 5개 파일의 크기/해시는 verification과 일치했다. APK는 38,905,857 bytes, SHA-256 `8B1481013DA5561BC84D543359C9AE1FAA5F6D4D0D13AEE1CFFA2460382D216B`. 빌드 성공 요약은 양쪽 Errors 0 / Warnings 0. 최신 SafeArea 소스와 같은 빌드라는 의미는 아니다.
- 독립 코어 검증 대상 7파일은 그대로다. SafeArea만 verification의 이전 해시와 달라졌으며, 기존 코어 29묶음을 불필요하게 다시 실행하지 않았다.
- 기존 720×1280 한영 설정, 영문 로비·대국·도움말·일시정지·결과 캡처를 읽었다. 문구/주요 조작이 표시되고, 설정의 SDK/수익 설명과 기본 NoFill 배너는 보이지 않는다. 언어명과 ‘언어 / Language’는 의도된 자국어/병기 표시다. 고정 캡처는 화면 크기 변경이나 실제 터치 자연스러움을 증명하지 않는다.
- 총괄 브라우저 보고: 390폭 한영 전환, 영어 로딩, 재로드 후 영어 및 2/30 복원, NoFill 안내 후 9수 승리 통과. 이전 빌드의 `rootIndependentRetest`와 혼동하지 않고 이번 별도 보고로 기록한다.
- **QA-008 열림:** 리사이즈 후 320 화면의 설정 프레임/닫기 버튼이 재로드 후보다 작다. 전후 이미지에서 차이를 독립 확인했다. UI 가독성/반응형 전체 통과로 판정하지 않는다. 최종 빌드 해시와 총괄 재시험으로 종료 여부를 결정한다.
- `SafeAreaRefitsAfterCanvasLayoutChangesWithoutScreenResize`의 최신 XML 통과는 저장 장면에서 Canvas 가용 사각형 변경을 재현한 회귀 증거다. 실제 WebGL 390→320 크기 변경 검증은 아직 대신하지 못한다. 실제 모바일, GC/p95, 사람 플레이, IAP·서버 실연동은 미검증이다.

## 최종 판정 — QA-008 종결

- 실행: `Tests/check-localization-evidence.ps1 -RunId qa-l10n-integration-final-20261014 -IncludeFinalBrowser`. 제품 실행 없이 제공 증거를 보존·대조했다. [최종 결과](Evidence/qa-l10n-integration-final-20261014/results.json)에서 XML **42/42**와 verification의 검사 수가 일치하고, Android/WebGL 산출물 5개 및 런타임 소스 8개가 모두 선언 해시와 일치한다.
- 독립 코어 7소스·Client/Generated JSON은 `qa-l10n-core-v1-20261014`와 같다. SafeArea 변경에 해당하는 Unity 회귀와 총괄 브라우저 입력 증거를 확인했으며 코어 29묶음은 반복하지 않았다. 번역 바인딩 30개/새 필드 6개/열 라벨 7개 정적 검사도 다시 확인했다.
- 같은 SafeArea 회귀는 보존된 수정 전 XML에서 실패(기대 스케일 0.375, 실제 1.0)했고 최종 XML에서는 통과했다. 최종 소스는 Canvas 렌더 직전 계산 및 가용 Rect 변경 감지를 포함한다. 클라이언트의 재현/실행 증거이며 QA 직접 Unity 실행이 아니다.
- [총괄 최종 브라우저 기록](Evidence/qa-l10n-integration-final-20261014/Docs/Management/Evidence/localization-browser-20261014/README.md): 390 설정 **346/54px**(프레임/닫기) → 재로드 없는 320 **284/44px** → 영어 선택 → 390 복귀 크기/언어 유지 → X 닫기 정상. QA가 세 캡처를 독립 검토했다. [콘솔](Evidence/qa-l10n-integration-final-20261014/Docs/Management/Evidence/localization-browser-20261014/final-console.json)은 `[]`다.
- 앞선 한영 전환·영어 로딩·언어 및 2/30 복원·NoFill 후 9수 승리 결과는 변경 없는 대국/언어 소스에 연결한다. 최종 단계에서 모든 핵심 흐름을 새로 실행한 것으로 기록하지 않는다.
- 최종 APK **38,905,900 bytes**, SHA-256 `7E49F8CD0156D4D975413C2FE8D9D08EAD07C7387411110C7C6699AB33646978`. WebGL WASM SHA-256 `816CE335BC51C67C6D34AB8D97FC16178253C5FBB60886D23308A700A1D760B4`; data SHA-256 `A036C8F993D418C2E0C39DF32FDF16BD0BC165A762E3CE81F28C6115A56D0EF4`. 양쪽 개발 빌드 Errors 0 / Warnings 0.
- **확인된 열린 결함 0건**. 지원 실기기/토스 호스트·회전/안전영역·터치 자연스러움·GC/p95·사람 플레이·저장 내구성·공식 광고·IAP/구매 서버 실연동은 미검증이다. 사용자 친화성/최적화/출시 완료로 확장 판정하지 않는다.
