# 훈련병 3상태 연결 준비 — 2026-10-15

**후속 상태:** 사용자가 이 기획 세션의 Client/GameData 수정 범위를 확장하고 적용·검증 진행을 승인했다. 훈련병 v12와 신규 18명·군모 변형을 저장 프리팹에 연결했으며 현재 상태는 [픽셀 통합 기록](../Docs/Design/PIXEL-INTEGRATION.md)을 따른다. 아래 적용 중단 및 v11 복원 내용은 이전 이력이다. v11 보관본은 변경하지 않는다.

**최신 상태: 사용자 지시로 적용 중단.** 이미지 확정과 별도 인게임 적용 지시 전까지 Client 구현·프리팹 연결을 진행하지 않는다. v11 아틀라스는 Unity에 가져오기 전에 `Archive/Character-20261015/RecruitThreeStateV11-not-applied.png`로 옮겼다. 작성 도중이던 RankGameApp/RankTheme 상태 전환 초안은 같은 폴더의 `*-state-draft.cs.txt`로 분리했다. 초안은 컴파일·검증하지 않았다.

실제 Assets의 두 C# 파일은 직전 진행 개편 상태로 복원했다. `Evidence/progression-source-freeze.json`의 전체 321개 파일과 해시가 일치하며 새 Assets 파일도 없다. 따라서 이전 로비·계급·난이도 변경은 모두 보존된다. 캐릭터 프리팹 연결·Unity 실행·빌드·브라우저 실행은 하지 않았다. 확인 기록: `Evidence/character-application-held.json`. 아래는 중단 이전 조사 기록이다.

총괄의 후속 요청을 받았을 때 Client는 로비 여백·30단계 난이도/18계급 변경의 최종 검증 중이었다. 해당 작업은 `Evidence/progression-verification.json`에 완료 기록을 남겼다. 훈련병 캐릭터 상태 전환은 아직 구현하지 않았으며, 총괄이 준비하는 텍스트 없는 투명 시작/대국 중/AI 패배 스프라이트 경로를 기다린다. 다른 역할 세션은 재개하지 않았다.

현재 구조는 `RankUiBuilder.BuildMatch`가 저장한 `AiHeader/AiAvatar`, `RankHat`, `AiName`, `AiStars`를 `RankGameApp`이 직렬화된 참조로 소비한다. `StartStage`는 보드 설정 직후 AI 탐색 또는 내 차례 토스트를 시작하고, `RefreshMatchHeader`는 단계의 `aiIntroKey`/`aiWinKey`/`aiLoseKey`를 사용한다. `FinishSequence`의 결과 표시 시점에도 같은 헤더를 갱신한다.

연결 준비안은 `rankKey`별 세 스프라이트 슬롯을 저장 자산에 두고, 준비된 `rank_01`만 전환하는 방식이다. 나머지 계급은 기존 얼굴/모자 표시를 유지한다. 진행 개편 후 훈련병은 1단계뿐이므로 과거 `(id-1)/3` 기준으로 훈련병을 세 단계에 복제하면 안 된다. 런타임 UI 오브젝트 생성 없이 저장된 Image를 교체한다.

시작 대기 상태는 `StartStage`에서 설정하고 `CanChoose`, 되돌리기·힌트 활성 조건, AI 탐색 시작을 함께 막는다. 기존 `ActiveTime`은 pause/확인창/OnApplicationPause를 제외하므로 재사용할 수 있지만 현재 OnApplicationFocus 처리는 없다. 포커스 이탈도 별도 정지 조건에 추가해야 한다. 대기 종료 후 playing 상태와 내 차례 토스트 또는 AI 탐색을 시작한다. 재도전·로비 이탈·OnDestroy에서는 시작 코루틴과 이전 matchId를 취소하고, 확인 취소는 남은 대기 시간을 유지한다. AI 패배 상태는 플레이어 승리(`board.Winner == 1`)에만 적용한다.

첫 도입은 기존 `story_first_intro`를 재사용한다. 요청된 새 패배 문구 `내가 지다니!!`는 현재 문자열 묶음에 없다. 최소 원본 변경은 기존 1단계 전용 `story_first_lose`의 ko/en 값을 수정하고 exporter→세 JSON 동기화를 거치는 것이다. 새 하드코딩이나 Generated 직접 수정은 필요 없다. 기존 대사 보존 정책과 영어 표현을 총괄이 확정한 뒤 원본 수정 범위를 명시하면 적용할 수 있다. 이번 GameData 예외 승인은 난이도·계급 변경 범위로 기록되어 있다.

스프라이트를 받은 뒤 필요한 검증은 시작 중 실제 입력/AI 잠금, 대사 종료 후 전환, pause/focus와 확인 취소 동안 남은 시간 보존, 재도전/이탈 후 이전 코루틴 무효화, 훈련병 외 계급 fallback, 한영/320·390·1080 캡처다. UI 원본·배너 여백·AI 응수 1초·확인창·정사각 보드를 보존한다. 새 WebGL/Android 빌드 및 브라우저 실행은 계속 보류한다.
