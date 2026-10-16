# PC 중앙 세로 화면 — 2026-10-16

상태: **Client 수정·자동 검증 완료, 소스 동결, Unity 종료. 총괄 WebGL 통합 빌드 시작 가능.** 최초 규칙/중앙 안내/물음표 버튼을 포함한 현재 프리팹과 코드를 보존했다. 새 빌드·Git·Pages 재배포는 총괄 담당이며 이 세션에서는 실행하지 않았다.

## 적용

- WebGL player 폭은 최대480 CSS px, 좌우 중앙 정렬, 높이는 visualViewport 높이다. 가로 창에서는 높이×9/16로 더 줄이며, 짧은 가로 화면에는320 CSS px 폭을 확보해44px 터치와 기존 세로 스크롤을 유지한다. 세로 PC800×1200도480px 제한을 적용한다. 모바일320/390/454 세로 폭은 그대로다.
- HTML CSS의 초기 배치와 host-layout.js의 실제 visualViewport 배치를 함께 수정했다. 여백은 기존 흰색 페이지 배경이다.
- Unity bridge에는 페이지 전체가 아닌 **실제 canvas의 CSS 폭/높이**를 전달한다. 좌우 safe inset은 중앙 여백과 교차하는 부분만 canvas 안의 inset으로 바꾼다. 상하 inset,64px 명시적 host allowance, 실제 controlsBottom 계약과8-float ABI는 유지한다. 미리보기 호스트 컨트롤은 player 안으로 옮겼다.
- SafeAreaFitter도 에디터/네이티브 가로 창에서 폭 기준 무제한 확대를 막는다. 세로 모바일 폭 채우기는 유지한다. 실제 웹의 큰 세로 창 최대폭은 HTML/adapter가 담당한다.

| 바깥 viewport | 게임 canvas | 좌측/우측 여백 |
|---|---|---|
|1280×720|405×720|437.5 /437.5|
|1920×1080|480×1080|720 /720|
|2556×1338|480×1338|1038 /1038|
|800×1200|480×1200|160 /160|
|320×568|320×568|0 /0|
|390×844|390×844|0 /0|
|390×664|390×664|0 /0|
|모바일 가로844×390|320×390|262 /262|

## 변경 파일

- `Assets/WebGLTemplates/StageGamePortrait/index.html`
- `Assets/WebGLTemplates/StageGamePortrait/host-layout.js`
- `Assets/StageGame/Scripts/UI/SafeAreaFitter.cs`
- `Tools/VerifyWebViewport.cjs`
- `Assets/StageGame/Tests/EditMode/RankDesktopViewportTests.cs` 및 메타
- `Client/IMPLEMENTATION.md`, 본 증거 폴더

`before.json`와 `source-changes.json` 비교상 기존 파일 변경은 런타임3개와 Node 검사1개다. 이전 미커밋 안내 UI·프리팹·아트·오디오·게임 데이터·씬을 보존했으며 서버/타 역할 폴더는 수정하지 않았다.

## 검사 결과와 한계

- **Unity12/12 통과**, 실패/스킵0: 새 PC/모바일 캔버스 검사2개, 기존 모바일 DPR 검사, HostLayout7개, 저장 프리팹 참조 검사, 앱 세션 안내 회귀 검사. `unity-tests.xml`/`unity-tests.log` 참조.
- **Node VM19/19 통과**: 모바일/PC/큰 세로 창, 가로 회전 및 복귀, visualViewport offset/resize, SDK/preview inset, 캔버스와 bridge 좌표, 비정상 수치. `viewport-adapter-tests.json` 참조. 실제 브라우저 실행 검사와 구분한다.
- **Unity100캡처**: 한국어/영어의 로비 상하 끝, 대국 상하 끝, 네이티브 가로 창, 모바일 DPR1/1.5/2 및 호스트 가로 회전 포함. `screens/layout-metrics.json`, `screens/viewport-metrics.json`에 수치가 있다. CSS로 제한한 게임 캔버스 크기를 Unity RenderTexture에 적용했다. 바깥 PC 창의 흰 여백을 브라우저에서 캡처한 자료는 아니다.
- PC1280×720의 실제 게임 캔버스405×720에서 로비 목록 높이398.125px, 최소 터치61.875px, 대국 최소 터치53.89px. 모바일320×568/가로 회전에서는44px 터치와 스크롤로 하단 다시하기 버튼 접근을 확인했다. 모든 검사에서 첫/마지막 스테이지 접근, 클릭 raycast, 제목/목록/하단 버튼 겹침 방지를 확인했다.
- 기존390×664 모바일은 제목 중심 y110.919px, 보드337.358px, 최소 터치49.289px로 유지된다. PC 로비 및 작은 모바일 대국 캡처를 직접 시각 확인했다.
- `git diff --check -- Client` 성공.
- 로컬 기존 WebGL 바이너리에 수정 템플릿을 덧대어 확인하는 `preview_server.py`를 준비했으나, 인앱 브라우저가 로컬 주소 연결 시간 초과 후 오류 페이지의 data URL을 정책상 차단했다. 다른 접근 수단으로 우회하지 않았다. 서버는 종료했다. 브라우저 크기 원복/탭 닫기도 같은 정책에서 막혔고, 검증 탭은 임시 탭이다. **실제 브라우저 DOM 배치·CSS 렌더·새 WebGL 실행·공개 사이트·실제 기기 검사 성공으로 기록하지 않는다.**

## 총괄 다음 단계

1. `source-freeze.json`을 기준으로 최초 규칙/중앙 안내/물음표 버튼까지 포함해 WebGL을 재빌드한다. 템플릿과 새 data/framework/wasm을 함께 배포한다.
2. 공개 사이트에서 위 viewport 표의 게임 폭·좌우 중앙·목록 표시를 DOM과 화면으로 확인한다. 새로고침 없이1280×720→390×844→800×1200 전환 후 진행 저장과 언어 설정이 유지되는지 확인한다.
3. 모바일390×664/320×568, 가로 회전 및 Toss preview에서 터치·스크롤·인셋을 확인한다. 로비/대국, 첫 실행 규칙 패널, `?` 재열기 및 앱당1회 중앙 안내도 함께 확인한다.
4. 공개 Pages의 배포 버전과 검증 결과를 총괄 문서에 기록한다. 이 파일의 “준비 완료”는 공개 배포 완료를 의미하지 않는다.
