# QA 발견 사항

기준일: 2026-10-14 (Asia/Seoul). 독립 실행 대상은 [소스 해시](Evidence/qa-core-v2-20261014/source-manifest.json)에 고정했다. 클라이언트/데이터 원본은 수정하지 않았다. 이후 수정은 새 실행에서 확인하기 전까지 해결로 표시하지 않는다.

최초 C# harness 실행은 구현 중 추가된 `IAdService.CancelPending()`에 QA 대체 어댑터가 대응하지 못해 컴파일 실패했다. QA 어댑터에 해당 인터페이스를 반영한 두 번째 실행은 컴파일 성공, 21개 검사 묶음 중 17 통과·4 실패다. 최초 실패는 제품 컴파일 결함이 아니다.

공통 실행: `Tests/run-independent.ps1 -RunId <새 ID>`. .NET 10.0.8에서 스냅샷 실제 소스를 컴파일한다. 규칙/AI/광고 상태는 실제 순수 C# 실행이며, ProgressStore와 GameApp은 `JsonUtility`, `PlayerPrefs`, UI/코루틴에 명시적인 대체 구현을 사용했다. 실제 Unity/디스크/SDK 동작까지 확인한 것은 아니다.

**최신 체크포인트:** v9 실제소스 독립40/40 통과. **QA-014 코드 수정·독립 재검증 통과**,runtime/Client JSON15파일이 최종freeze와 일치한다. [실행 결과](Evidence/qa-rank-v9-runtime-v1-20261014/results.json), [소스 연결](Evidence/qa-rank-v9-runtime-v1-20261014/runtime-freeze-link.json). 이번 웹 빌드/브라우저 실행은 사용자 승인 대기이고 실제 광고 SDK는 미연결이다. QA-011/012 종결은 이전 버전 기준이며 **QA-013은 시안 폐기로 후속 판정 중단·미확정 이력 보존**이다. 실제 기기/공식 광고/성능/사람 플레이/IAP·서버/출시 gate는 남는다.

## QA-014 — RankGameApp 파괴 뒤 지연 전면광고 완료가 저장을 변경

- 심각도/상태: **보통(P2), v9 코드 수정·독립 재검증 통과 / 최종 실행환경 확인 보류**. 현재 실제 광고 SDK는 미연결이며 실기기 증상은 확인하지 않았다. 아래 v8 실패는 수정 전 이력으로 보존한다.
- 대상: `RankGameApp.OnDestroy()`와 `ResultAdBoundary()`의 callback 생명주기.
- 재현: 광고 조건을 충족하는 승리 결과 → 약0.9초 연출/관찰 뒤 지연 전면광고 대기 → `OnDestroy()` → 보존된 callback에 `Completed` 전달.
- 기대: 파괴 시 광고 취소를 요청하고 판 식별자/생명주기를 무효화하여 후속 callback이 광고 기록·저장·화면에 접근하지 않는다.
- 실제: `OnDestroy()`가 AI/결과 coroutine·RewardGate만 취소한다. 광고 `CancelPending()` 호출 증가0, matchId와 result.activeSelf가 남아 callback이 `lastInterstitialMatch=0→4`와 시각을 변경하고 Save한다. 결과 총수는4로 유지한다.
- 증거: [24묶음 실행](Evidence/qa-rank-runtime-v2-20261014/results.json)의 `result.destroy_cancels_ad_and_stale_callback`, [고정 소스](Evidence/qa-rank-runtime-v2-20261014/Sources/RankGameApp.cs), [재현 코드](Evidence/qa-rank-runtime-v2-20261014/TestTools/Program.cs). 대상 RankGameApp SHA-256 `A63CAD6404834545BB425AD0CD4A927F946A18B027C74ED7A842553E209C9899`.
- 범위: 실제 제품 C#과 수동 Unity 대체환경/지연 광고 어댑터에서 재현했다. Unity 파괴 객체 접근 예외나 실제 SDK의 표시·콜백 타이밍을 재현했다고 주장하지 않는다. 현재 NoFill 일반 플레이에는 지연 광고가 없다.
- Client 요청: `OnDestroy`에서 광고 표시/콜백 취소와 판/생명주기 무효화를 보장한다. 실패 callback뿐 아니라 중복·늦은 완료, 파괴 시 reward/AI/결과연출 취소도 유지하고 영향 native 검사/빌드 해시를 갱신한다. QA는 Client 파일을 수정하지 않는다.
- 종료 조건: 동일 독립 파괴·늦은 callback 재현 통과, 정상 결과/NoFill/timeout·로비/Retry 취소 회귀, 최신 소스·Unity/빌드 연결. 현재 UI 가드 후속 변경은 이 파괴 경로를 수정하지 않았다.
- 마지막 수정 전 재현: [impact-v3](Evidence/qa-rank-runtime-impact-v3-20261014/results.json)에서8통과/동일1실패,RankGameApp `2F36B95E8AD1F116D5C97D19AEBDB77C905934C3CDDE7B305445FB0F5922ECAE`,검사 중 원본 변경0.
- v9 수정: OnDestroy에서 destroyed=true·matchId=null·pendingAction 초기화·광고취소를 적용하고 ResultAdBoundary는 destroyed/adGeneration/판ID를 검사한다. rewardGeneration은 이전 요청의 callback이 새 힌트/문구에 영향을 주지 못하게 한다. 소스 SHA-256 `3FDF28A138D2F931C63BCC95B8904B0E42A101E0F26DD545EE986AA19F7393F9`.
- 재검증: [v9 독립40/40](Evidence/qa-rank-v9-runtime-v1-20261014/results.json)에서 기존 `result.destroy_cancels_ad_and_stale_callback`가 통과했다. 광고취소 호출이 늘고 늦은 Completed로 광고 기록이 바뀌지 않는다. 보상/AI/확인/토스트 파괴 정리와 기존 정상 광고4결과·중복·timeout·로비/새판 취소 회귀도 통과했다. fixture22개를40묶음에 더하지 않는다.
- 최종 연결/한계: runtime12+Client JSON3이 최종freeze/현재파일과 일치하며 제공 Unity73/73 XML도 보존했다. QA는 Unity/브라우저/제품 빌드를 실행하지 않았다. 웹 재빌드·새 브라우저 실행은 사용자 승인 전 보류이므로 배포본/실제 SDK까지 종결한 것으로 표시하지 않는다. QA 소스 검증 업무는 완료했다.

## QA-001 — 저장 실패 시 종료 판에서 결과 화면으로 이동하지 못함

- 심각도: **높음**, 실패 주입으로 재현. 실제 기기 저장 실패 재현은 미실행.
- 대상: `GameApp.Finish`, `ProgressStore.RecordResult`.
- 재현: 정상 앱/판 준비 → 승리 보드 → PlayerPrefs.Save에 예외 주입 → Finish 호출.
- 기대: 저장 실패를 알리고 결과/로비/재도전 중 복구 가능한 화면을 제공. 성공하지 않은 저장 상태도 구별.
- 실제: `resultRecorded=true`를 먼저 설정하고 RecordResult → Save에서 예외가 전파된다. 결과 화면 활성화 코드까지 도달하지 못해 result=false, lobby=false 상태다. Finish 재호출은 resultRecorded에 막힌다.
- 영향: 저장 API 오류 때 완료한 판이 끝나지 않는 화면 상태가 될 수 있다. 같은 API에 다른 실패 모드가 있는지 실제 기기 확인 필요.
- 증거: [실행 결과](Evidence/qa-core-v2-20261014/results.json)의 `app.save_failure_still_exits_finished_match`, [GameApp 스냅샷](Evidence/qa-core-v2-20261014/Sources/GameApp.cs), [저장 스냅샷](Evidence/qa-core-v2-20261014/Sources/ProgressStore.cs).
- 상태: **core v3 재검증 통과**. Save 오류를 경고로 처리하고 결과 화면이 표시됨. 실제 기기 저장 실패/복구 검증은 남음.

## QA-002 — 동일 결과를 다른 결과 뒤 또는 재로드 뒤에 다시 집계

- 심각도: **보통**, 저장 API 단위 재현. 현재 GameApp의 `resultRecorded`/판 ID 방어를 우회하는 정상 UI 경로는 확인하지 못했다.
- 대상: `ProgressStore.RecordResult`의 단일 메모리 `processedMatch`.
- 재현 A: A 결과 → B 결과 → A 결과. 기대 completedMatches=2 및 마지막 호출 false, 실제 3 및 true.
- 재현 B: A 결과 저장 → 새 ProgressStore 생성 → A 결과. 기대 재집계 없음, 실제 completedMatches=2.
- 영향: 저장 API가 지연/재시도 결과를 받으면 완료 판 수가 증가해 광고 유예/간격 계산에도 영향을 줄 수 있다. 현재 실광고는 연결되지 않았다.
- 증거: [실행 결과](Evidence/qa-core-v2-20261014/results.json)의 `progress.nonconsecutive_duplicate`, `progress.duplicate_after_reload`.
- 상태: **core v3 재검증 통과**. 저장되는 processedMatchIds와 HashSet 중복 방어로 A/B/A 및 재로드/A가 거부됨. 실제 저장 내구성은 남음.

## QA-003 — 30스테이지 계약인데 잘린 유효 접두 목록을 정상 수용

- 심각도: **보통**, 검증 함수 재현. 실제 임포트 파일은 이번 실행에서 30개로 통과했다.
- 대상: `GameData` 생성자의 목록 길이 검사.
- 재현: 정상 schemaVersion=1인 30개 fixture를 1개로 잘라 관련 문자열을 정상 제공.
- 기대: 30개 계약의 누락을 검출하거나 프로토타입 부분 목록 모드를 명시.
- 실제: 목록 길이 1~30이면 수용하며, 1개가 전체 콘텐츠로 인식된다. 누락된 마지막 스테이지까지 런타임 검증에서 확인하지 않는다.
- 영향: 잘못 잘린 데이터 배포 시 콘텐츠가 조기에 끝나도 오류가 나지 않는다. exporter의 30개 검증과 런타임 검증 강도가 다르다.
- 증거: [실행 결과](Evidence/qa-core-v2-20261014/results.json)의 `data.reject_truncated_stage_catalog`, [검증 스냅샷](Evidence/qa-core-v2-20261014/Sources/GameData.cs).
- 상태: **core v3 재검증 통과**. 길이 30만 허용하며 정상 30개와 잘린 목록을 각각 수용/거부함.

## BAL-001 — 초기 데이터 5개 스테이지에서 고정 AI가 모든 플레이어 경로를 이김

- 분류: **높음, 밸런스/진행 gate 차단**. 아래 추가 탐색 전에는 단순 예시 미확보였으나, 실제 정책의 종료 트리 탐색으로 구체화됐다.
- 대상: 10, 12, 20, 22, 24, 26번.
- 방법: 플레이어 AI 깊이 4/5/6, 노드 예산 30,000, 최대 12개 다양화 시도. 스테이지 AI는 실제 파라미터와 GameApp 고정 시드 식 `(id-1)*997 + MoveCount*31` 사용.
- 결과: 24/30개에서 승리 예시 발견. 위 6개는 12회 안에 찾지 못했다. 전체 예시는 독립 보드 oracle로 착수·승자를 재검사했다.
- 한계: 실제 GameApp의 0.65초 벽시계 제한은 이 탐색에 적용하지 않았다. 제한에 걸리는 기기는 AI 수가 달라질 수 있다. 승리 예시는 최적 상대에 대한 강제승리, 사람의 클리어 가능성, 재미 증명이 아니다. 예시 미발견도 패배 강제 증명이 아니다.
- 증거: [스테이지별 경로](Evidence/qa-core-v2-20261014/stage-paths.json).
- 후속: 독립적인 제한 깊이 강제 패배 검사와 대상 스테이지 승리 경로 탐색. 승리 해금 구조이므로 합법 배치만으로 이 항목을 통과시키지 않는다.

### 추가 독립 증거

[깊이 제한 강제 패배 증명](Evidence/qa-core-v2-20261014/bounded-loss-proofs.json)에서 초기 10번은 8수, 12·20·22번은 6수 이내에 AI가 승리를 강제할 수 있는 전략을 찾았다. 모든 플레이어 합법 수를 열거하고 AI의 이기는 응수를 증명 트리로 저장한 뒤 다시 검증했다. 사용한 판정기는 실제 C# 보드를 호출하지 않는 Python 전체 4칸 창 검사다.

이는 **정확히 대응하는 상대에 대한 초기 배치의 강제 패배** 증거다. 실제 스테이지 AI가 반드시 그 전략을 선택한다는 증명은 아니며, 실제 고정 시드 정책과 연결한 탐색은 별도로 확인한다. 24·26번은 깊이 8까지 강제 패배를 증명하지 못했으며, 이 결과도 클리어 가능 판정이 아니다. 데이터 수정/재검증 전까지 밸런스 gate는 보류한다.

### 실제 고정 시드 정책의 추가 탐색

[고정 AI 탐색](Evidence/qa-core-v3-20261014/fixed-ai-loss-search.json)은 플레이어의 모든 합법 수를 분기하고, AI 턴은 실제 AiSearch의 `(id-1)*997+MoveCount*31` 결과 하나로 진행했다. 깊이 제한에 도달한 미종료 국면은 `unknown`으로 처리했으며 패배로 간주하지 않았다.

- **10·12·20·22·24번:** 모든 플레이어 분기가 AI 승리로 종료됨. 증명에 필요한 검사 깊이 상한은 각각 32·8·8·24·24수였으며, 해당 상한이 최단 패배 수라는 뜻은 아니다.
- **26번:** 깊이 24 안에 플레이어 승리 분기가 존재함. 앞선 12회 시뮬레이션의 예시 미발견과 구별한다.
- 사용 데이터는 core v2의 원본 JSON 스냅샷, 실행 소스는 core v3다. 보드/AI 소스 해시는 두 실행에서 동일하다. GameApp의 기기별 0.65초 계산 제한을 적용하지 않은 **탐색 완료 정책** 결과다.
- 영향: 승리로 다음 스테이지가 해금되는 구조에서 빠르게 계산을 완료하는 환경은 10번부터 진행이 막힌다. 수정된 데이터로 30개 모두의 승리 경로·해금 흐름을 다시 확인해야 한다. 사람 재미와 난이도 적절성은 별도 미검증이다.

### 수정 Generated 데이터 재검증

[core v4 경로](Evidence/qa-core-v4-20261014/stage-paths.json)에서 수정된 30개 모두 플레이어 승리 예시를 찾고 독립 oracle로 확인했다. 변경은 10/12/20/22/24/26의 openingMoves이며, 입력 stages.json SHA-256은 `2D04B124A42F0F3798AD942E9AACAB97EE34A25F05B380144CD70F75C3904992`다. [통합 스냅샷](Evidence/qa-integration-v1-20261014/results.json)에서 Client/Generated 3파일 일치, 검증한 데이터 및 6개 C# 소스 해시 유지를 확인해 **해당 정책의 진행 차단 해소**로 판정한다. 650ms 중단/사람 재미의 한계는 유지한다. 기존 26번의 승리 경로도 [별도 수순](Evidence/qa-core-v3-20261014/fixed-ai-stage26-win.json)에 보존했다.

## QA-004 — exporter 출력 교체 도중 오류가 나면 서로 다른 데이터 버전 혼합

- 심각도: **높음**, 실제 exporter의 격리 출력 디렉터리에서 실패 주입 재현.
- 대상: `GameData/export_data.py:write_outputs`의 JSON별 순차 `os.replace`.
- 재현: 정상 출력 3개를 Tests/.work에 저장 → stage_001의 titleKey와 대응 strings.key를 함께 변경한 새 묶음 준비 → 두 번째 os.replace에서 OSError 주입.
- 기대: 전체 변경 실패 시 이전 유효 출력 묶음 보존 또는 혼합 데이터 사용을 막는 원자적 묶음 선택.
- 실제: 첫 stages.json만 새 버전, strings.json은 이전 버전으로 남아 `stage_001_qa_changed` 참조가 사라짐.
- 영향: 디스크/파일 교체 실패 뒤 부분 생성물을 임포트하면 클라이언트 데이터 로드가 막힐 수 있다. 정상 입력의 검증 실패 보존 테스트만으로는 검출되지 않는다.
- 증거: [데이터 실행 결과](Evidence/qa-data-v1-20261014/results.json)의 `batch_write_failure_preserves_all_outputs`, [실제 exporter 스냅샷](Evidence/qa-data-v1-20261014/Sources/export_data.py), 재현 코드 `Tests/audit-data.py`.
- 기타 결과: 기존 exporter 테스트 21개 통과, Excel→Generated 3파일 바이트 일치, Generated→Client 의미 일치. 원본은 수정하지 않았다.
- 상태: **data v2 재검증 통과**. 기존 파일 백업과 처리 가능한 교체 오류의 rollback, 최초 생성 시 원래 파일 부재 복원, rollback 실패 시 마커/백업 보존 및 `--check` 차단을 확인했다. exporter 28테스트 및 독립 정합성/실패 주입 3검사 모두 통과. 실제 Excel/Generated/Client는 수정하지 않았다.
- 한계: 전원 상실/프로세스 강제 종료에 대한 파일시스템 원자적 트랜잭션을 증명한 것은 아니다. 해당 경우 잔존 마커를 확인하고 `--check` 성공 전 임포트/배포하지 않는 절차가 필요하다. 검증한 것은 처리 가능한 교체/복구 예외와 불완전 묶음 차단이다.

## QA-005 — WebGL 로드 후 전체 magenta 화면, UI 미표시

- 심각도: **높음**, 총괄의 실제 localhost 브라우저 실행 결과 전달. QA 자체 브라우저 재현은 수행하지 않았다.
- 환경: 클라이언트가 생성한 WebGL 프로토타입을 총괄이 localhost에서 실행. 최종 산출물 해시는 연결했으며 브라우저 제품/버전·스크린샷·콘솔 원본은 QA에게 별도 전달되지 않았다.
- 재현: 페이지/Unity loader 실행 완료 후 canvas 확인.
- 기대: 로비 UI 표시 및 스테이지 시작 가능.
- 실제: 전체 magenta 화면, UI 없음으로 보고됨.
- 영향: 해당 WebGL 산출물에서 기본 플레이 진입을 검증할 수 없음. 앞선 build-WebGL.txt Succeeded만으로 런타임 통과 처리할 수 없음.
- 원인 상태: 총괄은 `GraphicsSettings.m_AlwaysIncludedShaders=[]`를 확인하고 UI/Default·UI/Default Font 포함 후 정상 렌더됐다고 보고했다. QA는 수정된 설정을 읽었고 원인 재현/제품 수정은 직접 수행하지 않았다.
- 소유/후속: 클라이언트가 수정·재빌드, 총괄이 실제 브라우저 재시험. QA는 결과와 최종 해시를 읽고 gate 갱신. UI/Graphics만 바뀌면 변경 없는 코어 v4 검증을 재사용.
- 상태: **총괄의 localhost 브라우저 재시험에서 해소**. 로비·대국 시작·민트 착수·AI 응수·NoFill 후 입력 유지·쉼→로비·설정·라이선스 표시/스크롤, 콘솔 error/warning 0이 보고됨. [최종 브라우저 기록](Evidence/qa-integration-final-20261014/browser-report.md)과 빌드 해시를 연결했다.

## QA-006 — 라이선스 모달 X 클릭 보고: 재현 좌표 오류로 기각

- 최초 분류: 보통 후보. 총괄 실제 localhost WebGL의 최초 클릭 보고였으며 아래 정정으로 **제품 결함 분류를 철회**했다.
- 재현: 설정 → 오픈소스 라이선스 → 라이선스 모달 X 클릭.
- 기대: 라이선스 모달이 닫히고 설정으로 복귀.
- 최초 보고: X가 반응하지 않으며 Escape·설정 모달 X는 정상. 재확인에서 첫 클릭이 버튼 아래였음이 확인됐다.
- 최종 영향: 이 보고로 확인된 제품 영향 없음. 모바일 실기기 검증 미실행은 일반 gate로 남으며 이 오보고의 제품 결함 근거로 사용하지 않는다.
- 코드 읽기: GameApp의 noticesClose 리스너는 존재한다. 리스너 직접 호출만 하는 대체 UI 검사는 실제 포인터/레이캐스트 오류를 검출할 수 없으므로 총괄 브라우저 재시험을 기준으로 닫는다.
- 처리: 총괄이 실제 X 중심을 재클릭해 정정. 클라이언트 수정 요청은 철회했다. 새 모달 추가에 대한 별도 GameApp 공통 흐름 7묶음은 통과했으나 포인터 검증을 대체하지 않는다.
- 정정 근거: 총괄이 **동일 브라우저·동일 빌드**에서 실제 X 중심 `(783,85)` 클릭 시 즉시 설정으로 복귀함을 확인했다. 최초 클릭 `(783,118)`은 버튼 아래였다고 정정했다. 좌표는 해당 재시험 화면에만 적용한다.
- 상태: **제품 결함 아님 / QA 재현 오류로 기각**. 제품 코드 수정·재빌드를 요구하지 않는다. 정상 Escape·설정 X와 함께 기록을 보존한다.

## QA-007 — 390×844 viewport에서 WebGL canvas 가로 초과/우측 UI 잘림

- 심각도: **높음**, 총괄의 실제 localhost 브라우저 viewport 실측 보고. 실제 모바일 기기 검증은 아님.
- 재현: 같은 WebGL 페이지를 viewport 390×844로 표시.
- 기대: 세로 비율을 유지하면서 canvas와 핵심 UI가 가용 너비 안에 들어옴.
- 실제: DOM canvas가 **474.75×844**로 측정되어 390폭보다 84.75px 넓고 우측 UI가 잘림. desktop 1280×720은 정상.
- 영향: 휴대전화에 가까운 좁은 폭에서 UI 일부가 보이지 않아 해상도 대응 gate 미충족.
- 담당: 클라이언트 CSS 수정, 총괄 실제 브라우저 재시험. QA는 변경 없는 코어 증거와 수정 웹 산출물 해시를 연결했다.
- 상태: **총괄의 최종 실제 브라우저 재시험에서 해소**. 390×844에서 canvas 390×693.333(y=75.333), 로비·7열 보드·결과 잘림 없음. 실제 9수 승리→2/30 해금→reload 후 STAGE 02·2/30 복원까지 확인. [브라우저 기록](Evidence/qa-integration-final-20261014/browser-report.md). CSS/웹 산출물만 바뀐 부분은 새 해시로 연결하고 보드/AI 검사를 반복하지 않았다.

## QA-008 — 실행 중 390→320 리사이즈 후 UI 축소가 남음

- 심각도: **보통(P2)**. 글자·버튼 영역이 불필요하게 작아져 가독성/조작성이 떨어짐. 재로드로 회피 가능하며 진행 차단은 보고되지 않았다.
- 재현: 다국어 WebGL을 총괄 localhost 브라우저에서 390폭으로 실행 → 설정을 연 상태에서 320×568로 변경 → 같은 320×568에서 페이지 재로드와 비교. QA는 브라우저를 직접 조작하지 않고 총괄 재현 보고와 두 이미지를 독립 확인했다.
- 기대: 같은 가용 화면 크기에서는 진입/리사이즈 순서와 무관하게 UI가 같은 크기로 맞춰짐.
- 실제: 총괄 측정에서 리사이즈 후 설정 프레임 **232px·닫기 36px**, 재로드 후 **284px·닫기 44px**. 이미지에서도 축소 차이를 확인했다. 각각 영어/한국어 화면이므로 최종 재시험은 **같은 언어에서도** 비교한다.
- [축소 캡처](Evidence/qa-l10n-integration-v1-20261014/Docs/Management/Evidence/localization-browser-20261014/en-settings-320.jpg) · [재로드 캡처](Evidence/qa-l10n-integration-v1-20261014/Docs/Management/Evidence/localization-browser-20261014/ko-settings-320-reloaded.jpg).
- 수정 전 소스 관찰: `SafeAreaFitter.Update`는 Screen 크기/safeArea가 같으면 바로 반환한다. 이후 Canvas 내부 사각형만 바뀌는 경우를 다시 계산하지 않는 구조는 현상과 일치하지만, QA가 실제 브라우저에서 원인 실험을 수행한 것은 아니다.
- 담당/최종 상태: **수정·재검증 완료 / 종결**. 클라이언트 수정, 총괄 실제 브라우저 재시험, QA 소스/산출물 대조와 캡처 독립 검토. 중간 검토에서 41검사 빌드와 새 42검사 XML이 섞였던 증거는 보존하고, 별도 [최종 스냅샷](Evidence/qa-l10n-integration-final-20261014/results.json)으로 버전을 연결했다.
- 수정: `Canvas.willRenderCanvases`에서 크기를 계산하고 실제 가용 Rect 크기도 비교한다. OnDisable에서 이벤트를 해제한다. 최종 SafeArea SHA-256은 `BA20C7C48F2C47505BA9436FBD07570FE2F2838AC275DCD76F76832BE7C53222`이며 verification·현재 소스·총괄 최종 빌드 기록이 일치한다.
- 재현/회귀 증거: 클라이언트 `layout-before.xml`의 동일 회귀는 수정 전 실패(기대 0.375, 실제 1.0), 최종 XML에서는 통과했고 전체 **42/42**다. QA가 Unity를 실행한 결과가 아니다.
- 총괄 최종 브라우저: 390 설정 프레임/닫기 **346/54px → 재로드 없이 320에서 284/44px → 영어 선택 → 390 복귀에서 크기/영어 유지 → X로 로비**. 콘솔 error/warn `[]`, 한국어 복귀·viewport 초기화. QA는 [최종 캡처와 보고](Evidence/qa-l10n-integration-final-20261014/Docs/Management/Evidence/localization-browser-20261014/README.md)를 읽어 확인했다.
- 종료 범위: 결함이 발생한 크기 변경과 언어/닫기 입력 회귀를 직접 확인한 총괄 증거로 종결한다. 기존 언어/대국 7소스·JSON은 변경이 없어 앞선 한영 전환·재로드 후 2/30 해금·9수 승리 증거를 함께 사용하며 모든 대국을 다시 실행했다고 기록하지 않는다. 회전·실기기 안전영역/터치·장시간 저장 내구성은 일반 미검증 범위로 남긴다.

## QA-009 — 호스트 inset의 비유한/float32 범위 입력 방어 미흡

- 심각도/상태: **낮음(P3), 수정 및 독립 재검사 완료 / 종결**. 정상적인 실제 Toss 입력에서 발생했다는 증거는 없다.
- 독립 재현 A: `HostLayout.ContentArea`에 NaN inset을 직접 전달하면 Rect가 NaN을 포함한다. 숫자 Unity API 대체 환경의 실제 HostLayout 소스 실행이다. 현재 JS의 NaN 정리는 통과하므로 실제 SDK 경로에서 이 NaN이 유입됐다고 단정하지 않는다.
- 독립 재현 B: `stageGameHostLayout.setInsets({left:1e300,top:1e300,right:1e300,bottom:1e300},1e300)` 후 `StageGameReadHostInsets`로 전달하면 HEAPF32에 Infinity가 기록된다. JS `Number.isFinite`만으로는 float32 표현 범위를 보장하지 못한다. 실제 JS/jslib·Node VM 대체 환경에서 확인했다.
- 기대: 외부 입력·숫자 변환 경계에서 비유한값과 표현 범위를 벗어난 값의 처리 정책을 정하고 유한한 좌표만 렌더링 계층에 전달한다. 과도한 inset의 빈 영역은 최소 지원 크기/콘텐츠 표시 정책과 구분한다.
- 제한: B의 Infinity inset은 이번 C# 테스트에서 유한한 빈 영역으로 clamp됐다. B가 A의 NaN 출력으로 곧바로 이어진다는 증거는 아니다. 브라우저 UI 소실/충돌은 아직 재현하지 않았다.
- 증거: [C# 실패](Evidence/qa-host-pre-v1-20261014/geometry-results.json), [브리지 실패](Evidence/qa-host-pre-v1-20261014/bridge-results.json), [고정 소스](Evidence/qa-host-pre-v1-20261014/source-manifest.json).
- 수정: Client가 JS의 float32 표현 불가능한 inset을 0, controlsBottom을 -1로 정규화하고 C#의 NaN/Infinity inset도 0으로 처리한다. 64px 제품 예약은 유지한다.
- 수정 확인: [독립 C# 8/8](Evidence/qa-host-fixed-v1-20261014/geometry-results.json), [JS/브리지 6/6](Evidence/qa-host-fixed-v1-20261014/bridge-results.json). 기존 실패 입력에 대해 유한값 여부뿐 아니라 기대 Rect `(0,0,320,504)`, 브리지 `[0,0,0,0,390,844,-1]`, CSS 상단 `64px`까지 확인했다. 정상 inset/좌표/resize 회귀도 통과했고 실행 중 소스 변경은 없었다.
- 종료 범위: 두 수치 경계 결함의 소스 독립 재현·수정 확인으로 종결한다. Client의 관련 Unity8/8 증거를 [별도 보존](Evidence/qa-host-editor-v1-20261014/results.json)했으며 호스트4소스가 독립 검사 스냅샷과 같다. 이후 본문30 [최종 빌드/브라우저 연결](Evidence/qa-host-integration-final-20261014/results.json)에서도 동일 소스를 확인했다. 실제 Toss·기기 검증은 별도다.

## QA-010 — 불투명 패배 결과창이 가린 금색 줄을 보라는 안내

- 심각도/상태: **낮음(P3), 수정·재검증 완료 / 종결**. 총괄 실제 브라우저에서 발견한, 화면에서 수행할 수 없는 행동을 안내하는 문제다.
- 재현: 현재 단계지도 WebGL에서 패배 → 불투명 결과 모달이 보드를 가림 → `result_loss_horizontal/vertical/diagonal`의 둘째 문장이 ‘금색 줄을 살펴보세요 / Look at the gold line.’로 표시된다. 실제 입력은 총괄, QA는 [수정 전 패배 캡처](Evidence/qa-stage-loss-copy-before-20261014/en-loss-retry-390.jpg)와 당시 [문구/빌드 해시](Evidence/qa-stage-loss-copy-before-20261014/source-manifest.json)를 보존·검토했다.
- 기대: 모달 안에서 읽을 수 있는 패배 방향 설명을 제공한다. 보이지 않는 판의 표시를 보라는 요청은 제거한다.
- 승인된 수정 범위: 위3키의ko/en에서 첫 문장은 그대로 유지하고 둘째 문장만 삭제. 코드·프리팹·나머지134키는 유지한다. [정확한 변경 전/후 기대값](../../Tests/Fixtures/stage-ux-approved-copy-changes.json)에 총괄 지시를 기록했다.
- 문구 수정 확인: [정확한 JSON 비교](Evidence/qa-stage-loss-copy-fixed-v1-20261014/results.json) **PASS**. 137행 중 승인3키의 ko/en 첫 문장만 남고 나머지134행은 동일하다. Generated/Client SHA-256 `57E392CF68464898829946F17030E4AE3008AAB86370452BACF6DBE56D68A291` 일치, 읽기 중 변경 없음.
- 최종 연결: [통합 증거](Evidence/qa-stage-integration-final-20261014/results.json)에서 기존 독립검사 런타임7소스, Client 선언 코드/프리팹20개, 재적용5프리팹의 현재 해시 일치를 확인했다. 최종 APK/WebGL7산출물과 Client 두 보고서·총괄 기록이 모두 일치한다. 코드/프리팹 변경이 없어 앞선8·26묶음을 반복하지 않았다.
- 재시험: Client의 문구 수정 후 영향 **11/11**은 최초 전체57개와 별도다. QA는 갱신된320폭 한영 세로 패배 이미지2개와 총괄의390폭 한국어 최종 패배/로비/도움말3개를 읽어 한 문장 안내·재도전 강조·주요 잘림 없음 확인. 총괄은 최종 WebGL을 새로 불러와2단계10수 세로 패배와 로비 복귀를 실제 입력으로 확인했고 콘솔 오류/경고0이다. [캡처 검토](Evidence/qa-stage-integration-final-20261014/capture-review.json).
- 종료 범위: 방향3개×한영은 정확한 데이터 비교, 최종 한영 세로 패배는 Unity 렌더, 최종 한국어 세로 패배는 브라우저 포인터 증거다. 모든 방향·언어를 최종 브라우저에서 재현했다고 주장하지 않는다. QA는 Unity나 브라우저를 직접 실행하지 않았다.

## QA-011 — 확대 중인 마지막 말이 위 빈칸에 가려짐

- 심각도/상태: **보통(P2), 수정·재검증 완료 / 종결**. 사용자 요청의 승리 마지막 말 연출이 가려지던 문제다. 게임 진행 차단이나 저장 손실 증거는 없다.
- 출처/관찰: 총괄이 Client 중간 캡처에서 발견했고 QA가 [animation-player-0.png](Evidence/qa-graphics-visual-findings-20261014/animation-player-0.png)를 보존·열어 확인했다. 하단4열의 확대 민트 말 윗부분 위로 바로 위 빈칸 원형이 그려진다. QA가 연출을 실제 실행한 결과는 아니다.
- 기대: 플레이어/AI 양측 승리의 마지막 말이 확대·놓임 중 인접 빈칸/말에 가려지지 않고 식별된다. 연출 완료 또는 취소/새 판 뒤에는 크기·위치·그리기 순서를 원래대로 복원한다.
- 원인/수정 계획: 총괄은 렌더 sibling 순서를 원인으로 보고했고 Client가 마지막 말을 앞에 렌더한 뒤 취소 시 원복하도록 수정할 예정이다. 원인과 수정 효과는 QA 독립 실행 전이다.
- 종료 조건: 소스 동결 후 RES07로 양측 승리 연속 렌더 및 정상 완료/취소/재시작 상태 복원을 확인한다. 정지 이미지 하나·관련 파일 존재만으로 애니메이션 전체 통과를 선언하지 않는다. [수정 전 해시](Evidence/qa-graphics-visual-findings-20261014/source-manifest.json).
- 수정 확인: [독립 graphics6 포함31개 통과](Evidence/qa-graphics-core-v2-20261014/results.json)에서 확대/관찰/광고 대기의 취소·로비/새 판·파괴 시 원복을 확인했다. Client59/59의 양측 연출 및 취소 native 검사, 수정 후 양측 확대/배치4장에서도 가림이 해소됐다. [캡처/타임라인 판독](Evidence/qa-graphics-frozen-v1-20261014/capture-review.json). 실제 최종 브라우저 보고 연결은 대기 중이다.
- 최종 종료 근거: [최종 브라우저 보고](Evidence/qa-graphics-browser-final-20261014/README.md)에서 실제9수 플레이어 승리·10수 AI 승리의 전환/승리 표시와 결과 보드 노출을 확인했다.400ms의 모든 프레임/취소 원복은 위 native/독립 증거와 구분하고 동일 배포본 해시를 연결해 종결한다.

## QA-012 — 결과 화면에 일시정지 아이콘이 남음

- 심각도/상태: **낮음(P3), 수정·재검증 완료 / 종결**. 결과에 Retry/Lobby 두 버튼만 남기는 요구와 맞지 않는 일시정지 아이콘이 표시되던 문제다.
- 출처/관찰: 총괄 발견 후 QA가 [승리 결과](Evidence/qa-graphics-visual-findings-20261014/map-ko-320-result-0.png)와 [패배 결과](Evidence/qa-graphics-visual-findings-20261014/map-ko-320-result-1.png)를 읽었다. 둘 다 상단 오른쪽 일시정지 아이콘이 남는다. 총괄은 비활성 상태라고 보고했으며 QA는 캡처만으로 클릭 차단 여부를 판정하지 않는다.
- 기대/수정 계획: 승리/패배/무승부/마지막30 결과에서 일시정지를 숨기고 Retry 후 진행 중에는 표시·동작을 복원한다. resolving/result 동안 입력 guard를 유지한다. Client가 수정할 예정이다.
- 종료 조건: 소스 동결 후 RES08로 모든 결과의 가시성·Retry 복원 및 총괄 실제 포인터 동선을 확인한다. 보드 노출과 Retry/Lobby의 가로 배치는 중간 이미지에서 확인했으나 전체 결과 기능 통과로 승격하지 않는다. [관찰 범위](Evidence/qa-graphics-visual-findings-20261014/capture-review.json).
- 수정 확인: 독립 승패/무승부/30 결과의 두 행동과 새 판 pause 표시/입력 복원, Client59/59의 `AssertTwoResultActions` 및 결과 캡처에서 숨김을 확인했다. 최종 빌드가 해당 런타임/프리팹과 연결됐으며 총괄 실제 포인터 보고를 받아 종료한다. [빌드 연결](Evidence/qa-graphics-build-link-v2-20261014/results.json).
- 최종 종료 근거: 총괄이 실제 승패 결과에서 pause 미노출·보드 클릭 차단, Retry로 같은1단계 빈 판과 pause 복원, Lobby로 최고 해금2 선택을 확인했다. [최종 브라우저](Evidence/qa-graphics-browser-final-20261014/README.md)와 QA 결과 이미지 판독을 연결해 종결한다.

## QA-013 — 일부 모달 캡처에서 닫기 표시 누락

- 최신 처리: 사용자가 해당 Tabletop 시안을 폐기했으므로 **후속 판정 중단·미확정 이력 보존**. 아래 후보 증거를 삭제하거나 수정 완료로 바꾸지 않는다. 새 UI에 동일 결함이 있다고 가정하지 않으며 신규 닫기 표시/포인터 검증으로 확인한다.

- 심각도/상태: **보통(P2) 표시 결함 후보, 실제 표시·클릭 재확인 필요**. 원인과 사용자의 닫기 조작 차단은 아직 확정하지 않았다.
- 최종 브라우저 증거: [영어320 도움말](Evidence/qa-graphics-browser-final-20261014/after-en-help-320.jpg)의 상단 오른쪽에 닫기 아이콘/버튼이 보이지 않는다. 같은 빌드 [영어320 설정](Evidence/qa-graphics-browser-final-20261014/after-en-settings-320.jpg)에서는 보인다. 파일은 총괄 실제 WebGL 캡처이며 QA가 직접 열어 판독했다.
- 선행 CAP-01: Client native `host-ko-320-settings.png`·`host-en-320-confirm.png`에도 닫기와 eyebrow가 누락됐지만 다른 언어/해상도 이미지에서는 정상이다. [관찰 목록](Evidence/qa-graphics-frozen-v1-20261014/capture-review.json). native 참조·hitbox 통과만으로 실제 표시 정상이라고 판정할 수 없다.
- 기대: 결과 외 닫기가 있는 도움말/설정/일시정지에서는 사용자가 식별 가능한 닫기 표시와 정상 조작을 제공한다. 결과 Close 제거 요구를 다른 모달에 확대 적용하지 않는다.
- 최소 재확인: 동일 최종 WebGL의320 영어 도움말에서 닫기 표시·클릭 및 새 캡처를 확인한다.320 한국어 설정·영어 일시정지도 비교한다. 실제 화면에서 재현되면 Client의 영향 범위 수정/검사를 요구하고, 캡처만의 문제면 그 근거를 남겨 후보를 기각한다. QA가 원인을 추측해 제품 코드를 수정하지 않는다.
- 판정 제한: 총괄 완료 보고는 도움말 표시 및 라이선스 닫기 성공을 기록했으며 해당 도움말 캡처 시점의 닫기 표시/클릭을 명시하지 않는다. 이 후보를 숨기고 전체 그래픽을 통과 처리하지 않는다. 기존 QA-011/012 종결과 독립32묶음·native59/별도1·빌드 연결은 유지한다.
