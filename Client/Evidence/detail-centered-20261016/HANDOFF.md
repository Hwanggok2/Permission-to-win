# 스테이지 상세 중앙 배치 — 2026-10-16

Client 준비 완료. 소스 동결, Unity 배치 종료, 프로젝트 잠금 없음. 총괄은 이 변경을 포함해 WebGL 통합 빌드와 배포를 진행할 수 있다. 이번 세션은 빌드·배포·커밋·푸시를 하지 않았다.

## 변경

- `RankDetailFitter`가 저장된 번호/닫기 버튼과 Scroll의 상하 여백을 계산해 안전영역 안에서 전체 상세 내용을 중앙에 둔다. 공간이 부족하면 최소 여백 48 설계 단위를 남기고 본문을 스크롤한다. UI 객체를 런타임에 생성하지 않는다.
- Detail 루트 좌표를 바꾸지 않아 `SlideDetail`의 Y=0 및 가로 슬라이드와 충돌하지 않는다. 닫기 버튼은 고정 헤더에 남는다.
- 설명 높이를 105→150 설계 단위로 늘리고 아래 상태/시작 버튼을 30 단위 내렸다. 글자 크기는 39 설계 단위를 유지한다. 전체 본문 높이는 1477.5→1507.5다.
- `RankUiBuilder.ApplyCenteredDetail`이 Detail/RankAppRoot를 저장·검증했다. RankAppRoot는 중첩 Detail을 상속하므로 파일 자체의 추가 차이는 없다. 전체 제작 경로에도 같은 설정을 적용했다.
- 실제 소스 변경은 제작 도구, Detail 프리팹, 새 fitter/검사 코드와 각각의 meta다. 아트·음원·데이터·대국/로비·씬·웹 템플릿·다른 프리팹·패키지/프로젝트 설정은 기존 해시와 같다. `source-changes.json` 참조.

## 검증

Unity 6000.3.15f1: `unity-tests.xml` **7/7 통과**, `screens/` PNG **58장**. 배치 적용도 종료 코드 0. 실행 로그는 `unity-apply.log`, `unity-tests.log`다.

- 한영 × 1080×1920, 320×568, 390×664, 390×844, PC 내부 캔버스 405×720/480×1080 × 일반/Toss inset 59/34 = 24조합, 상단·하단 스크롤 48캡처. 기존 호스트 상단 컨트롤 예약도 포함한다.
- 헤더 위와 본문 아래 여백 오차는 0.001px 미만. 최소 터치 크기 48.88px 이상. 좁은 호스트 화면에서도 아래로 스크롤하면 시작 버튼이 완전히 보이고 실제 UI raycast가 해당 버튼을 찾는다.
- 가로 슬라이드 중 입력 잠금, 슬라이드 완료 후 위치, 이전/다음 선택, 시작 및 닫기, 로비 스크롤 복원 확인.
- 한영 30개 스테이지 설명의 preferredHeight가 저장된 영역 안에 들어온다. 가장 긴 영어 설명을 포함한 대표 이미지를 직접 확인했다.
- 최초 실행 1스테이지 + 자동 규칙 패널, 패널 중 타이머/착수/AI 정지와 BGM 유지, 닫은 후 착수 안내, 기존/복구 저장의 로비 시작 회귀 검사 통과.
- `git diff --check -- Client` 통과. `before.json`과 `source-freeze.json` 비교 결과를 `source-changes.json`에 기록했다.

대표 이미지: `screens/ko-390x844-plain-detail.png`, `screens/ko-320x568-host-detail-bottom.png`, `screens/en-390x664-longest-description.png`, `screens/en-480x1080-host-detail.png`. 수치: `screens/detail-metrics.json`.

이는 Unity 렌더 및 입력 자동 검사다. 실제 브라우저/Toss 기기/사람 플레이 및 새 WebGL 빌드·공개 사이트 반영은 미실행이다. 총괄이 통합 빌드·배포 후 확인해야 한다.
