# Rank UI 독립 harness

대상은 새 활성 `RankGameApp`, `RankTheme`, `RankStageCard`, `RankDetailBlocker`와 실제 Core/광고다. `Program.cs`/`RankHarness.csproj`를 구성해 v8 후보를 실제 실행했다. 기존 `Tests/IndependentHarness`의 `GameApp`/Tabletop32묶음을 새 UI 통과로 사용하지 않는다. 구 GameApp/Tabletop 파일은 LocalizedLabel 공유 타입 의존성 때문에 컴파일할 뿐 테스트 인스턴스로 만들지 않는다.

기존 whole-board oracle·수동 시계/중첩 coroutine/지연 광고 방식을 재사용하고 별도 UnityStubs에 Rank API의 Vector/RectTransform/Image/ScrollRect만 추가했다. 실제 AiSearch를 실행하고 취소한 iterator를 강제로 재개해 generation 방어도 확인한다. 테스트의 UI 객체 조립은 명시적 대체환경으로 제품 런타임 UI 제작이 아니다. 실제 프리팹·레이캐스트·네이티브 코루틴/물리 저장 증거로 사용하지 않는다.

실행은 저장소 루트에서 `./Tests/run-rank-independent.ps1 -RunId <새ID>`. `-SnapshotOnly`는 후보만 보존, `-CandidateId <기존후보ID>`는 같은 제품 소스를 재사용, `-Scope 'result.'`는 영향 그룹만 실행한다. 사용한 제품/도구 snapshot으로 컴파일하며 기존 Evidence를 덮어쓰지 않는다. 실제 Product 파일은 수정하지 않는다.

v8 전체24묶음23통과/1실패와 최신 영향9묶음8통과/1실패를 합쳐 서로 다른25묶음24통과/QA-014 한 건 실패다. 실패는 OnDestroy 후 전면광고 callback이 광고 기록을 저장하는 경로다. **v9 확인 패널·턴 toast·새 레이아웃은 아직 API 연결/실행 전**이며 기존 직접 Retry/Lobby 테스트를 그대로 새 사양에 적용하지 않는다.

v9 스토리 대사 변경으로 데이터 동결도 해제됐다. 현재 `data.frozen_rank_contract`의228키·계급공통 대사키 기대값과 별도 정적 checker의 고정해시는 v8 후보 전용이다. v9는 stage1 별도키 예외/최종 총수·승인 변경을 받은 뒤 연결한다. 사용자 원문을22자/한줄 조건 때문에 축약하지 않는다.

- 입력: `../Fixtures/rank-ui-interaction-cases.json` 및 `../Fixtures/rank-ui-crossing-win-cases.json`.
- v9 준비 입력: `../Fixtures/rank-v9-flow-cases.json`. 확인 취소/수락1회·이전 action/토스트 취소·QA-014·한영 첫 대사·정사각형/짧은 viewport를 의미 기반으로 명시했다. 아직 제품 API에 바인딩하거나 실행하지 않았으며 Root의 준비 완료 인계 후 연결한다.
- 준비한 독립 승리선 oracle: `WinningSegmentOracle.cs`. 두 끝점이 같은 승자 돌의 수평/수직/45도 대각 연속4개 이상이며 마지막 착수칸을 포함하는지 판정한다. 런타임의 WinningCells 전체 최장 두 점 알고리즘을 재사용하지 않는다.
- source 바인딩 완료: RankGameApp의 PlaceSelected/UndoTurn/SelectColumn·undoHistory·searchGeneration·first-launch·SlideDetail·ShowWinLine 좌표. RankTheme/Card는 실제 타입의 직렬화 필드를 대체 객체로 공급한다. 실제 저장 참조/geometry와 popup/overlay pointer 차단은 native/browser 범위다.
- 데이터 전용 검사: rankKey/descriptionKey/aiIntroKey/aiWinKey/aiLoseKey 누락·공란·미존재 key, difficultyStars0/6,10계급×3·별6단계 구간, AI 승/패 대사 방향, 기존 opening/AI 설정 의미 보존.
- 추가 상태 기대값: Undo는 힌트 Granted/Pending을 과거로 되감아 사용 권한을 돌려주지 않는다. 사용 완료된 힌트는 계속 사용 완료이며 기존 추천/강조 표시만 지운다. 늦은 보상 callback은 기존 판/요청 유효성 규칙으로 별도 확인한다.

자세한 결과·v9 영향 계획은 [RANK-UI-RESULTS.md](../../Docs/QA/RANK-UI-RESULTS.md)를 따른다. 초기 준비 JSON의 미실행 표시는 작성 당시 상태이며 최신 실제 실행은 별도 Evidence에 보존한다.
