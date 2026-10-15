# Rank UI 독립 harness 준비

대상은 새 활성 `RankGameApp`, `RankTheme`, `RankStageCard`다. 기존 `Tests/IndependentHarness`의 `GameApp`/Tabletop32묶음을 새 UI의 통과로 사용하지 않는다. Client·데이터 동결 뒤 새 전용 실행 진입점과 `.csproj`를 구성한다. 현재 이 폴더에는 **제품 실행 진입점이 없으며 검사를 실행하지 않았다.**

재사용할 요소는 기존 독립 whole-board oracle, 실제 Core/광고/저장 소스 snapshot, 가상 시계·중첩 coroutine·지연광고 callback이다. UI 대체 구현은 새 실제 API를 확인한 범위만 추가한다. 기존 MakeApp의 GameApp 참조·137키·Tabletop sprite·두 버튼 기대값을 그대로 복제하지 않는다.

- 입력: `../Fixtures/rank-ui-interaction-cases.json` 및 `../Fixtures/rank-ui-crossing-win-cases.json`.
- 준비한 독립 승리선 oracle: `WinningSegmentOracle.cs`. 두 끝점이 같은 승자 돌의 수평/수직/45도 대각 연속4개 이상이며 마지막 착수칸을 포함하는지 판정한다. 런타임의 WinningCells 전체 최장 두 점 알고리즘을 재사용하지 않는다.
- source 바인딩 필요: RankGameApp의 Place/Undo/선택·opening/history·AI 취소 세대·first-launch 판정·preview 슬라이드/배경 guard·실제 승리선의 두 끝점 또는 RectTransform 좌표, RankTheme/Card 저장 참조.
- 데이터 전용 검사: rankKey/descriptionKey/aiIntroKey/aiWinKey/aiLoseKey 누락·공란·미존재 key, difficultyStars0/6,10계급×3·별6단계 구간, AI 승/패 대사 방향, 기존 opening/AI 설정 의미 보존.
- 추가 상태 기대값: Undo는 힌트 Granted/Pending을 과거로 되감아 사용 권한을 돌려주지 않는다. 사용 완료된 힌트는 계속 사용 완료이며 기존 추천/강조 표시만 지운다. 늦은 보상 callback은 기존 판/요청 유효성 규칙으로 별도 확인한다.

자세한 실행 순서·자료 요청은 [RANK-UI-RESULTS.md](../../Docs/QA/RANK-UI-RESULTS.md)를 따른다. fixture/oracle 코드 준비는 새 제품 통과 증거가 아니다.
