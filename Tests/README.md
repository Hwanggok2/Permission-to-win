# 계약 v1 독립 검증 준비

상태: **독립 실행 harness 및 데이터 검사 작성·실행됨.** 기준일 2026-10-14 (Asia/Seoul). 실제 결과·한계는 [QA 상태](../Docs/QA/STATUS.md), 결함과 재검증은 [DEFECTS.md](../Docs/QA/DEFECTS.md)에 기록한다. 아래 고정 기대값은 초기 준비 명세를 보존한 것이다.

실행 도구:

- **최신 Rank UI v9 독립40/40 통과:** `run-rank-independent.ps1 -RunId <새ID>`는 후보12소스·Client/Generated6JSON·v8비교3JSON·계약/fixture 및 별도 [RankHarness](RankHarness/README.md) 도구를 고정해 컴파일/실행한다. `-SnapshotOnly`로 후보만 고정하거나 `-CandidateId qa-rank-v9-candidate-v1-20261014`로 같은 v9 후보를 재사용할 수 있다. `-Scope 'v9.,result.'`처럼 영향 묶음을 제한한다. QA-014 원래 재현·확인/토스트·237키/스토리 검사를 통과했고 runtime12+Client JSON3이 최종freeze와 일치한다. [결과와 제한](../Docs/QA/RANK-UI-RESULTS.md). 이 runner는 독립 .NET 검사이며 WebGL/브라우저를 실행하지 않는다. 이번 제품 웹 빌드와 새 브라우저 실행은 사용자 승인 대기다.
- `Fixtures/rank-ui-interaction-cases.json`은 의미 기반 입력 준비물이다. 실제 소스 바인딩은 RankHarness/Program.cs에 있으며 승리선 helper와4벡터·구형 저장8개를 실행했다. 구 `run-independent.ps1`/GameApp/Tabletop32묶음·137키·두 결과 버튼 기대값을 새 Rank UI 통과로 사용하지 않는다.
- `check-rank-data-preparation.py`는 **이전 v8/228키 시점의 정적 준비 검사**로 보존한다. 현재 v9/237키에 실행할 기준이 아니다. 당시47승인변경·30단계/광고 보존·해시/참조·23입력/복수승리4벡터 검산은 별도 Evidence에 보존했고 최신 데이터 검사는 RankHarness의v9 승인 변경/해시 검사로 대체했다.

- `check-graphics-assets.ps1 -RunId <새 ID>`: 동결 소스/프리팹/아트/데이터·Client59검사/캡처를 보존하고 theme21·prefab sprite GUID·AppRoot theme/노드 icon10·이전 데이터/소스·제작도구 재적용 해시를 읽는다. 참조 존재를 시각 완성/실제 렌더로 대체하지 않는다.
- `check-graphics-evidence.ps1 -RunId <새 ID>`: Client 최종 freeze·59검사와 별도 Editor guard1검사·62캡처/타임라인/데이터·7빌드산출물·APK 고지8개·독립 실행 소스·현재 총괄 브라우저 파일을 연결한다. 총괄 README가 실행 전일 수 있으므로 도구 완료만으로 실제 포인터 통과를 선언하지 않는다.
- `run-independent.ps1 -RunId <새 ID> -Scope 'app.,l10n.,stageux.,ads.,data.actual_client_json,graphics.'`: 그래픽/새 결과 흐름 영향32묶음. 기존26은 sprite와 두 결과 버튼 계약에 맞췄고 graphics6은 연출 시간/입력/광고 순서, pause, 취소/순서 원복, 해금/로비, 무승부, 실제 Drop/Think 종료 진입을 확인한다. 중첩 IEnumerator·수동 시계·sprite identity·순서 목록은 명시적 대체 구현이며 Unity 스케줄·레이캐스트·그림 증거가 아니다.

- 신규 단계 UX 입력 `Fixtures/stage-ux-legacy-saves.json`: 최초설치1개·구형 raw저장8개 및 독립 의미 기대값. `highestActuallyCompletedStage`는 기대 의미이며 실제 `highestClearedStage`에 연결한다. 실행결과는 [단계 UX 기록](../Docs/QA/STAGE-UX-RESULTS.md)을 따른다.
- `run-progress-independent.ps1 -RunId <새 ID>`: 작업중UI와분리하여 ProgressStore/StringTable 실제소스 및 하네스/fixture를스냅샷으로 컴파일·검사한다. 구형이행·실제마지막완료·단조진척·중복/재도전·기존정보·잘못된입력/복구/저장예외8묶음이다. JSON/PlayerPrefs는기존명시적대체구현이며 native Unity·디스크내구성·GameApp/광고/CTA 검사를대신하지 않는다. 빌드중간파일은Tests/.work에둔다.

- `run-independent.ps1 -RunId <새 ID>`: 실제 C# 소스를 증거 경로에 복사/해시 고정 후 오프라인 .NET 10 빌드·실행. 보드/AI/광고는 실제 로직, Unity API는 `IndependentHarness/UnityStubs.cs`에 명시된 대체 구현. 실행 증거·출처 검사를 `Docs/QA/Evidence/`에 남긴다.
- `run-independent.ps1 -RunId <새 ID> -Scope app.`: 변경된 GameApp 공통 흐름 7묶음만 실행. 기존 보드/AI 1,000개 검사 및 30스테이지 탐색은 반복하지 않는다. 대체 UI의 직접 이벤트 검사는 실제 포인터 레이캐스트 오류를 검출하지 못한다.
- `run-independent.ps1 -RunId <새 ID> -SkipStageExploration`: 등록된전체검사(기존21·다국어8·단계UX8)를실행하고30스테이지승리경로재탐색은생략한다. 넓은회귀가필요한변경에만사용하며보드·AI·Generated스테이지가같으면아래범위필터로불필요한반복을피한다.
- `run-independent.ps1 -RunId <새 ID> -Scope l10n.`: 현재137개 한영 조회, 기존58개/요청70개 분리, 누락/중복/포맷 오류, schema1 언어 이행·복원, 경고문구/UI소스키, 같은판언어변경8묶음. 직접리터럴키만정적검사하고동적tier키는단계UX검사로확인한다. PlayerPrefs/JsonUtility/UI/코루틴은대체구현이다.
- `run-independent.ps1 -RunId <새 ID> -Scope 'app.,l10n.,stageux.,ads.,data.actual_client_json'`: 단계UX영향회귀26묶음. 쉼표로여러prefix를지정한다. 새14키/기존123보존·최초/구형로비·한영30노드/잠금/매핑·승패무승부/최종30CTA·광고경계·저장경고를기존광고/다국어회귀와함께검사한다. UIRectTransform은slot지정만검사하며픽셀/프리팹/포인터를대신하지않는다.
- `check-stage-prefab.ps1 -RunId <새 ID>`: AppRoot노드3배열각10참조·챕터/결과7참조·도움말4번역참조·저장StageNode중첩10개의YAML선언을읽고스냅샷을남긴다. native Unity참조해결/5×2형상/44px는별도다.
- `Fixtures/stage-ux-approved-copy-changes.json`은QA-010에대해총괄이승인한패배3키의ko/en 첫문장유지·둘째문장삭제기대값이다. 기존문구보존검사에서이3키만명시적으로예외처리하며그외변경은거부한다.
- `check-stage-copy.ps1 -RunId <새 ID>`: 수정전137행과현재Generated/Client를비교해정확히3키가승인대로변경되고134행이그대로인지확인한다. UI/runtime검사와별개다.
- `check-stage-evidence.ps1 -RunId <새 ID> -IncludeFinalBrowser`: Client 최초 Unity57과 별도 문구 영향11·소스/프리팹/40캡처·재적용5프리팹·최종 APK/WebGL7산출물/고지8개·독립검사 소스 일치·승인 문구3키·총괄 최종 브라우저 해시를 보존/연결한다. 초기32캡처와 영향검사에서 갱신한 결과/완료8캡처를 구별한다. QA가 Unity/브라우저를 실행하지 않는다.
- `check-localization-evidence.ps1 -RunId <새 ID>`: 클라이언트 verification/XML·캡처·소스를 보존하고 빌드/소스 해시, 코어 변경 여부, AppRoot 번역 참조를 읽기 전용 검사한다. 진행 중 수정으로 XML/verification/빌드가 다른 버전일 수 있으므로 JSON의 `matchesDeclared`와 각 `matches`를 확인한다. Unity·브라우저는 실행하지 않는다.
- 최종 브라우저 자료가 준비되면 위 명령에 `-IncludeFinalBrowser`를 추가해 최종 캡처·콘솔·빌드 기록과 SafeArea 수정 전 실패 XML도 함께 보존한다. 결함 종료는 해시 대조와 실제 재시험 보고를 확인한 뒤 QA 문서에 기록한다.
- `run-host-independent.ps1 -RunId <새 ID>`: HostLayout 실제 C#과 host-layout.js/HostInsets.jslib 실제 JS를 스냅샷으로 고정해 검사한다. C# 숫자 API/JS DOM·event·HEAPF32는 명시적 대체 구현이다. 현재14묶음은 64 예약·OS/CSS max·변환비·controlsBottom·비정상 수치·resize·브리지 순서 및 최소 지원 세로 프로파일을 검사하며 Unity·브라우저·픽셀 hitbox·실제 Toss 검증을 대신하지 않는다.
- `check-host-evidence.ps1 -RunId <새 ID>`: Client의 전체49/수정 후 관련8 Unity XML·호스트 캡처28개·프리팹/자산/소스를 보존하고 독립 검사와의 호스트 해시 일치 및 기존 코어/JSON 불변을 확인한다. 빌드/브라우저 증거 연결은 별도이며 QA는 Unity·브라우저를 실행하지 않는다.
- 위 명령에 `-IncludeBuilds`를 추가하면 최종 본문30 화면1/1 XML·Client 선언/실제 산출물7개·프리팹/이미지/캡처 해시·본문24→30 단독 변경·APK 내부 고지8개와 원본 일치·총괄 브라우저 파일을 연결한다. 빌드 산출물은 QA/Git 폴더로 복사하지 않고 크기/해시만 기록한다. 총괄 브라우저의 초기 기능과 최종 폰트 검사 범위는 보존된 보고에서 구분한다.
- `audit-data.py <새 ID>`: bundled Python으로 실행. 실제 Excel/exporter/test를 스냅샷으로 읽고, 기존 테스트의 모든 쓰기를 `Tests/.work/`로 전환한다. 생성/임포트 정합성과 중간 출력 실패를 검사한다. Excel은 저장하지 않는다.
- `check-stage-solvability.py <stages.json> <output.json>`: Python 독립 보드로 제한 깊이 강제 패배 전략과 증명 트리를 검증. 미증명은 클리어 가능 판정이 아니다.
- 컴파일한 harness의 `--prove-fixed <data directory> <output.json>` 모드: 실제 고정 시드 AI를 상대로 플레이어 수를 전수 분기한다. 탐색 제한 국면은 unknown으로 유지한다.

기준은 [계약 v1](../Docs/Management/CONTRACT.md), 상세 사례는 [QA 계획](../Docs/QA/TEST-PLAN.md), 실제 검증 현황은 [QA 상태](../Docs/QA/STATUS.md)를 따른다. 아래 기대값은 제품의 승리/합법 수/보상 함수를 복사해 만들지 않는다. 후속 통합검증에서 실제 구현의 결과와 비교한다.

## 1. 실행 범위

- `Client/`, `GameData/`, `Server/`, `Licence/`는 읽기만 한다. 소스·Excel·생성 JSON·프리팹/씬을 QA가 수정하지 않는다.
- 입력 변이·격리한 실행 출력·테스트 도구는 `Tests/`, 증거는 `Docs/QA/`에만 쓴다. 원본 exporter의 출력 경로가 `GameData/Generated/`로 고정돼 있으면 그대로 실행하지 않고, 격리 출력 방법을 확인한다.
- Unity 에디터 실행·프로젝트 열기·배치 빌드·프리팹 생성은 클라이언트 담당 소유다. 컴파일/에디터 테스트·실기기 증거를 받으면 실행자·빌드·조건을 명시한다.
- 순수 규칙/광고 상태 소스를 독립 실행할 수 있는지 먼저 의존성을 읽는다. 가능하면 실제 소스를 대상으로 `Tests/` 안에서 검증한다. 불가능하면 소스 검토와 클라이언트 제공 실행 증거를 분리한다.
- 독립 참고 모델/수기 수순의 자기검사는 제품 통과로 집계하지 않는다. 문자열 검색으로 코드가 존재함을 확인한 것도 기능 통과가 아니다.
- 실행 전 실제 파일·지침·실행 환경을 확인한다. 구현 API 변경으로 harness가 컴파일되지 않으면 제품 테스트 실패와 구별한다.

## 2. 규칙의 고정 기대값

각 수순은 빈 보드에서 `starter=1`로 시작해 교대로 재생한다. 행 0을 바닥으로 표기한다. 제품 내부의 행 방향이 다르면 입출력 경계에서만 변환한다. 종료 수까지의 규칙 엔진 테스트와, 종료 전이어야 하는 스테이지 opening 검증을 구분한다.

| 준비 ID | 열 순서/입력 | 독립 기대값 | QA 사례 |
| --- | --- | --- | --- |
| F-R01 | 빈 수순, 열 0 입력 | `(열0, 행0)=1`, 나머지 0, 다음 턴 -1 | R01/R02 |
| F-R02 | `0,0,0,0,0,0` 뒤 열 0 재입력 | 높이 6, 승자 없음, 마지막 입력 거부·보드/턴 불변 | R03 |
| F-R03 | 열 -1/7 입력 | 범위 밖 거부·보드/턴 불변 | R03 |
| F-R04 | `0,6,1,6,2,5,3` | 플레이어 가로 승리, 바닥 열 0~3 | R04 |
| F-R05 | `0,1,0,1,0,2,0` | 플레이어 세로 승리, 열 0 행 0~3 | R04 |
| F-R06 | `0,1,1,2,3,2,2,3,4,3,3` | 플레이어 우상향 대각선 승리 `(0,0)`~`(3,3)` | R04 |
| F-R07 | `6,5,5,4,3,4,4,3,2,3,3` | 플레이어 좌상향 대각선 승리 `(6,0)`~`(3,3)` | R04 |
| F-R08 | 위 승리 수순 뒤 임의 열 0~6 추가 | 모든 착수 거부·결과/턴/수 수 불변 | R03/R05 |
| F-R09 | F-R04~07을 `starter=-1`로 재생 | 위치는 동일, 승자만 AI=-1로 대칭 변경 | R01/R04 |
| F-R10 | 아래 42수 수순 전부/앞 41수 | 전부는 무승부, 앞 41수는 미종료·마지막 열 5에 착수 가능 | R07 |
| F-R11 | `0,6,1,6,2` 후 AI 수 | 즉시 승리 수가 없으므로 열 3으로 패배 방어 | AI02 |
| F-R12 | `0,6,1,6,2,6,4` 후 AI 수 | AI 열 6 즉시 승리 우선. 플레이어 열 3 방어보다 승리를 선택 | AI02 |

F-R10 무승부 후보 수순(수기 기대값, 후속 독립 실행에서 재확인):

```text
0,2,2,2,2,2,2,0,0,0,0,0,
1,3,3,3,3,3,3,1,1,1,1,1,
4,6,6,6,6,6,6,4,4,4,4,4,
5,5,5,5,5,5
```

수순의 최종 정답 보드(위 행부터):

```text
-1 -1  1  1 -1 -1  1
 1  1 -1 -1  1  1 -1
-1 -1  1  1 -1 -1  1
 1  1 -1 -1  1  1 -1
-1 -1  1  1 -1 -1  1
 1  1 -1 -1  1  1 -1
```

추가 성질 검증: 거부 수는 전체 상태 불변, 정상 수는 빈 칸 하나만 변경, 칸 수 합계 42, 공중에 뜬 돌 0개, 좌우 반사/돌 값 반전에 따른 승자 대칭, AI 호출 전후 원본 상태 불변. 무작위 표본은 합법 수를 재생해 만들고 종료 뒤 수를 이어 붙이지 않는다.

## 3. 30스테이지 데이터의 독립 확인과 변이

확인할 실제 입력은 `GameData/FORMAT.md`, Excel 원본, exporter, `GameData/Generated/{stages,ads,strings}.json`, 클라이언트가 임포트한 `Client/Assets/StageGame/Resources/GameData/`다. 생성/임포트 완료 여부는 후속 시점에 확인한다.

| 준비 ID | 정상/변이 입력 | 기대값 |
| --- | --- | --- |
| F-D01 | schemaVersion=1인 wrapper, stages 30개 | ID 1~30 모두 1회, chapter 1~3 각 10개, titleKey 전부 참조 가능 |
| F-D02 | 정상 입력에서 ID 중복/누락, chapter 범위 초과, 없는 titleKey 1개씩 변경 | 변이마다 명확한 실패, 잘못된 스테이지를 조용히 제외한 성공 금지 |
| F-D03 | depth 1/6, budget 100/30000, random 0/1000, starter ±1 | 경계값 허용. 각 범위 바깥·잘못된 타입 거부 |
| F-D04 | opening `""`, `"0"`, `"0,0,0,0,0,0"` | 양 시작측 모두 유효한 미종료 중력식 국면 |
| F-D05 | opening `"-1"`, `"7"`, `"x"`, `"0,,1"`, `"0,0,0,0,0,0,0"` | 범위/형식/가득 찬 열 오류 검출 |
| F-D06 | F-R04 등 승리 수순, 그 뒤 추가 수, F-R10 무승부 전체를 opening으로 제공 | 모두 종료된 시작 국면이므로 거부. 규칙 엔진의 정상 종료 판정과 구분 |
| F-D07 | 정상/변이 입력의 Excel→JSON 및 생성→임포트 묶음 비교 | 원본 행·타입·단위 보존, schema/키/내용 대응. 허용한 포맷 차이는 의미 비교 |
| F-D08 | ads의 계약값 `3,2,90,true,true,1` 및 strings UTF-8/키 | 계약 필드와 값 일치, 실제 FORMAT의 필수값·타입·중복 기준 적용 |

opening 재생은 시작측을 매 수 교대하고, **각 수 전/후**에 종료 상태·열 높이를 검사한다. 정상 JSON임과 게임상 유효한 데이터임은 별개다. 본문에 나열된 변이로 실제 exporter/runtime validator가 실패하는지도 별도로 확인한다. 새 참고 검사기가 변이를 거부했다는 사실만으로 실제 exporter가 검증된 것은 아니다.

## 4. 광고 상태의 이벤트 기대값

`M1/M2`는 서로 다른 판, `T1/T2`는 서로 다른 광고 거래다. 실제 구현이 별도 ID를 사용하지 않더라도 판·요청을 구별하고 중복을 막는 효과를 검증한다. 성공은 SDK 계약상 검증된 완료를 의미한다.

| 준비 ID | 입력 순서 | 기대값 |
| --- | --- | --- |
| F-A01 | M1/T1 시작→성공→동일 성공→닫기→동일 성공 | M1 힌트 지급 횟수 1, 판당 힌트 남은 횟수 0 |
| F-A02 | 미준비/로드 실패/표시 실패/취소/오프라인 각각→닫기 | 지급 0, 로비·대국·무료 재도전 가능, 버튼 복구 |
| F-A03 | M1/T1 진행 중 같은 버튼 연타 | 동시에 표시되는 광고/유효 요청 최대 1 |
| F-A04 | M1/T1 시작→M1 종료→M2 시작→T1 늦은 성공 | M2에 힌트 지급 0, M2 중 전면광고 표시 없음 |
| F-A05 | M1/T1 성공→M1/T2 성공 시도 | M1 총 힌트 1회 상한 유지. 이미 소진한 신규 요청 차단 |
| F-A06 | M1/T1 실패→M1/T2 정상 성공 | 첫 실패는 소진 아님. M1 정상 힌트 1회 가능 |
| F-A07 | 닫기→늦은 유효 완료, 완료→닫기 | SDK가 허용한 순서는 1회 지급. 닫기만 도착한 경우 0회 |
| F-A08 | 종료/저장 경계 중단→재실행/복구→같은 거래 이벤트 | 정한 복구 정책 유지, 같은 판 총 지급 ≤1, 이전 판 보상 이월 없음 |
| F-A09 | 완료 경기 수 ≤3 | 시간·간격과 무관하게 전면광고 없음 |
| F-A10 | 유예 이후, 직전 노출 뒤 (1판,90초)/(2판,89.9초)/(2판,90초) | 앞 두 조건 불허, 마지막 조건만 경기 종료 경계에서 기회 허용 |
| F-A11 | 대국/일시정지/앱 재진입, 로비/결과 전환 | 앞 세 위치 전면광고 없음, 배너는 로비/결과 예약 공간만 |
| F-A12 | 기본 프로토타입의 플레이어 빌드·Editor 분기 읽기 | 기본 no-fill, Editor 전용 모의 성공/실패/중복 구분. 정적 판독은 SDK 연동 통과 아님 |

실패/취소 뒤 유효 완료가 도착하는 모순 이벤트, 첫 전면광고의 시간 기산점·경기 카운터, no-fill 시 간격 소진 여부는 플랫폼/제품 계약에서 먼저 확정한다. 확정되지 않은 순서를 임의로 성공·실패로 판정하지 않는다.

## 5. 읽기 검토·동적 실행·기기 증거의 분리

| 검증 층 | 확인할 것 | 주장할 수 없는 것 |
| --- | --- | --- |
| 소스/파일 읽기 | 규칙/검증 분기, 광고 상태 격리, 런타임 UI 조립 유무, 실제 prefab/scene/참조 파일 존재 | 컴파일·실행 성공, 화면 배치·안전영역·기기 성능 |
| 독립 실행 | 실제 순수 소스에 고정 수순·상태 이벤트 입력, 실제 데이터/변이와 비교 | Unity 직렬화·라이프사이클·SDK·플랫폼 호환 |
| 클라이언트 제공 Unity 결과 | 빌드 식별자와 연결한 컴파일/EditMode/Play 결과 | Android/WebGL 실제 기기 또는 승인된 테스트 광고 동작 |
| 실제 플랫폼 | 테스트 광고·복구·입력·프레임·저장·배포 경로 | 스토어 승인·출시 후 수익/유지율 |

최초 후속 실행부터 명령, 도구 버전, 파일 해시, 실제 결과, 출처와 한계를 남긴다. 읽기 검토의 발견 사항은 별도 결함 후보로 기록하고 미실행 사례를 통과로 변경하지 않는다.
