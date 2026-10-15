# Rank UI 독립 harness

현재 v10은 준비 단계다. `../Fixtures/rank-v10-ai-timing-cases.json`에 AI 약1초 활성시간·탐색 겹침·pause/확인 제외·취소·힌트 제외를 준비했으며 Client 동결 전 제품에 실행하지 않는다. 아래40/40은 마지막 완료된v9 증거다. v10 타이틀 선택에 따라 `Rank by Rank` literal/데이터 count 기대값은 승인 변경 목록에 맞춰 갱신한다. 사용자승인전WebGL/Android빌드·새브라우저실행은금지이며 이번산출물은Unity이미지+소스QA다.

대상은 새 활성 `RankGameApp`, `RankTheme`, `RankStageCard`, `RankDetailBlocker`와 실제 Core/광고다. `Program.cs`/`V9Checks.cs`/`RankHarness.csproj`로 최신v9 후보를 실행해 **40/40 통과**했다. 기존 `Tests/IndependentHarness`의 `GameApp`/Tabletop32묶음을 새 UI 통과로 사용하지 않는다. 구 GameApp/Tabletop 파일은 LocalizedLabel 공유 타입 의존성 때문에 컴파일할 뿐 테스트 인스턴스로 만들지 않는다.

기존 whole-board oracle·수동 시계/중첩 coroutine/지연 광고 방식을 재사용하고 별도 UnityStubs에 Rank API의 Vector/RectTransform/Image/ScrollRect만 추가했다. 실제 AiSearch를 실행하고 취소한 iterator를 강제로 재개해 generation 방어도 확인한다. 테스트의 UI 객체 조립은 명시적 대체환경으로 제품 런타임 UI 제작이 아니다. 실제 프리팹·레이캐스트·네이티브 코루틴/물리 저장 증거로 사용하지 않는다.

실행은 저장소 루트에서 `./Tests/run-rank-independent.ps1 -RunId <새ID>`. `-SnapshotOnly`는 후보만 보존, `-CandidateId <기존후보ID>`는 같은 제품 소스를 재사용, `-Scope 'result.'`는 영향 그룹만 실행한다. 사용한 제품/도구 snapshot으로 컴파일하며 기존 Evidence를 덮어쓰지 않는다. 실제 Product 파일은 수정하지 않는다.

v8의 서로 다른25묶음24통과/QA-014실패1은 이전 이력이다. **v9는 확인 패널·턴 toast·스토리 API를 연결해 기존25회귀+추가15=40묶음 단일 실행 통과**했다. QA-014 원래 재현과 늦은 reward/AI callback도 통과했다. 사용자 버튼 Retry/Lobby는 확인→수락 경로로 검사하고 내부 초기화용 직접 호출과 구분한다.

v9 최종237키/Stage1전용3대사·승인변경37/추가9/보존191·최종해시를 연결했다. `data.frozen_rank_contract`는237키와 첫 상대 예외를 검사한다. 별도 `check-rank-data-preparation.py`의228키 고정해시는 이전v8 전용이다. 사용자 원문을22자/한줄 조건 때문에 축약하지 않는다.

- 입력: `../Fixtures/rank-ui-interaction-cases.json` 및 `../Fixtures/rank-ui-crossing-win-cases.json`.
- v9 입력: `../Fixtures/rank-v9-flow-cases.json`은 작성 당시22개 준비 상태를 보존한다. 실제 확인 취소/수락·늦은 action/토스트 정리·QA-014·한영 대사는V9Checks.cs에 연결해 실행했다.22개 모두의 포인터/정사각형/짧은 viewport를 독립 검증한 뜻은 아니며 실제 geometry/레이캐스트/렌더는 native/browser 범위다.
- 준비한 독립 승리선 oracle: `WinningSegmentOracle.cs`. 두 끝점이 같은 승자 돌의 수평/수직/45도 대각 연속4개 이상이며 마지막 착수칸을 포함하는지 판정한다. 런타임의 WinningCells 전체 최장 두 점 알고리즘을 재사용하지 않는다.
- source 바인딩 완료: RankGameApp의 PlaceSelected/UndoTurn/SelectColumn·undoHistory·searchGeneration·first-launch·SlideDetail·ShowWinLine 좌표. RankTheme/Card는 실제 타입의 직렬화 필드를 대체 객체로 공급한다. 실제 저장 참조/geometry와 popup/overlay pointer 차단은 native/browser 범위다.
- 데이터 전용 검사: rankKey/descriptionKey/aiIntroKey/aiWinKey/aiLoseKey 누락·공란·미존재 key, difficultyStars0/6,10계급×3·별6단계 구간, AI 승/패 대사 방향, 기존 opening/AI 설정 의미 보존.
- 추가 상태 기대값: Undo는 힌트 Granted/Pending을 과거로 되감아 사용 권한을 돌려주지 않는다. 사용 완료된 힌트는 계속 사용 완료이며 기존 추천/강조 표시만 지운다. 늦은 보상 callback은 기존 판/요청 유효성 규칙으로 별도 확인한다.

자세한 결과·제한은 [RANK-UI-RESULTS.md](../../Docs/QA/RANK-UI-RESULTS.md)를 따른다. 초기 준비 JSON의 미실행 표시는 작성 당시 상태이며 최신 실제 실행은 별도 Evidence에 보존한다. runtime/Client JSON15파일이 Client 최종freeze와 일치하며 제공Unity73/73과독립40/40을 합산하지 않는다. Text.preferredWidth는고정대체값이므로중앙정렬/말풍선렌더측정이아니다. 이번 WebGL 재빌드·새브라우저실행은사용자승인전보류,휴대폰시험도보류다.
