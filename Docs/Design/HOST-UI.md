# 토스 호스트 UI와 게임 화면 분리

확인일: **2026-10-14 (Asia/Seoul)**. 공식 문서와 Toss Unity SDK `release/v3.8.0`의 고정 커밋 `18814fe5b5a2af5cd2a11a7a5bae53403fafa15e`를 읽은 조사다. 토스 실기기에서 측정한 규격은 아니다.

## 확인된 사실

| 항목 | 공식 근거와 적용 범위 |
| --- | --- |
| 게임 상단 UI | `webViewProps.type: 'game'`의 기본 내비게이션은 **더보기 + 닫기(X)**이며, 게임은 투명 배경을 설정한다. 비게임의 뒤로/홈/제목 설정을 게임 기본 사양으로 혼동하지 않는다. 실제 뒤로 버튼이 추가되는 환경도 겹침 검사를 한다. [내비게이션 가이드](https://developers-apps-in-toss.toss.im/documentation/common/navigationbar) |
| X 버튼 위치 | 문서의 우측 기준 값은 `safeAreaInsets.right + 10`, 상단 기준 값은 **iOS `top + 5` / Android `top + 10`**이다. 이는 화면 왼쪽 원점의 절대 X 좌표가 아니다. **버튼 폭·높이·전체 터치 영역 크기까지 설명한 값은 아니다.** [Safe Area 가이드](https://developers-apps-in-toss.toss.im/documentation/common/screen/safe-area) |
| SafeAreaInsets 의미 | 노치·상태바·홈 인디케이터 등에 가려지지 않게 확보할 네 방향 여백이며 get/subscribe로 현재 값과 변경을 받는다. 이 값만으로 게임 내비게이션의 더보기/X 전체 영역이 이미 제외됐다고 판단할 수 없다. X 위치를 별도 가산하는 공식 설명이 있다. [Safe Area](https://developers-apps-in-toss.toss.im/documentation/common/screen/safe-area) |
| 단위 근거 | 공식 웹 예제는 inset 값을 변환 없이 `style.paddingTop = insets.top + 'px'` 등에 넣는다. **웹 레이아웃에서는 CSS px로 취급할 근거**다. 문서에는 네이티브 물리 픽셀 단위라는 별도 보장이 없다. [조회 API 예제](https://developers-apps-in-toss.toss.im/documentation/sdk/domains-api/safearea/getsafeareainsets) |
| Unity 브리지 | 고정 버전의 C#은 Top/Bottom/Left/Right를 double로 받고, jslib는 웹 `SafeAreaInsets.get()` 결과를 JSON으로 전달한다. 확인한 경로에는 DPR 곱셈이나 내비게이션 높이 가산이 없다. Editor 반환은 mock/default여서 실환경 규격 증거가 아니다. [타입](https://github.com/toss/apps-in-toss-unity-sdk/blob/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/Runtime/SDK/AIT.Types.SafeArea.cs), [브리지](https://github.com/toss/apps-in-toss-unity-sdk/blob/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/Runtime/SDK/Plugins/AppsInToss-SafeArea.jslib), [C#](https://github.com/toss/apps-in-toss-unity-sdk/blob/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/Runtime/SDK/AIT.SafeArea.cs) |
| 심사 조건 | 닫기 버튼 정상 노출/작동, Safe Area와 Dynamic Island 비침범, 전체 화면 게임 및 Android 시스템 뒤로가기 동작이 요구된다. [게임 출시 가이드](https://developers-apps-in-toss.toss.im/checklist/app-game.html) |

## 미확정과 구현 제안

- **공식 문서에서 확인되지 않은 값:** 더보기/X 묶음의 전체 폭·높이·hit area, 모든 기기의 단일 상단 제외 높이, 게임 WebView가 항상 같은 inset을 반환한다는 보장. **커뮤니티의 54px mock은 실기기 공식 규격이 아니다.** 현재 확인한 문서는 X 위치만으로 이 값을 확정할 수 없다.
- **총괄 채택 시험값:** OS/호스트 상단 inset 아래에 **추가 64 CSS px**를 예약한다. 즉 같은 viewport 좌표계의 UI 시작선은 `safeTop + 64 CSS px`이며, 64는 공식 토스 높이가 아닌 **StageGame 제품 안전 여백**이다. 이 띠가 플랫폼별 X 위 여백과 실제 호스트 hit area 및 필요한 간격을 모두 덮는지 실기기에서 검증한 뒤 확정한다. 54px mock이나 64px 시험값만으로 실기기 통과를 선언하지 않는다.
- 배경과 canvas는 전체 화면에 두고, 제목·버튼·보드·말·모든 모달·라이선스 닫기 조작은 위 예약 영역 아래에 둔다. 호스트 UI를 덮거나 숨겨서 겹침을 해소하지 않는다.
- **좌표 변환 제안:** CSS inset을 Unity 렌더 픽셀로 직접 더하지 않는다. 전체 viewport를 채운 canvas라면 수직 변환비는 `canvas 렌더 높이 / canvas CSS 높이`, 수평은 해당 폭 비율이다. 여백·레터박스가 있으면 viewport 기준 금지 영역과 `getBoundingClientRect()`의 canvas 위치를 먼저 대조한다. DPR은 이 비율과 같다고 무조건 가정하지 않으며 두 번 곱하지 않는다. 이미 CSS에서 제외한 영역을 Unity에서 다시 빼지 않는다.
- 최초 진입·화면 크기/회전·복귀에서 inset과 실제 canvas rect를 다시 반영한다. SDK 문서의 API 명칭과 고정 Unity 버전의 래퍼 명칭이 다르므로 실제 도입 버전의 동작을 확인한다.

## 완료 증거

Android/iOS 토스 실환경에서 기기·OS·토스/SDK 버전, 네 방향 inset, CSS viewport/canvas rect, 렌더 크기·DPR, 상단 호스트 터치 영역을 기록한다. 로비/대국/결과/설정/라이선스 화면에서 제목·게임 버튼과 토스 더보기/닫기/해당 뒤로 버튼이 겹치지 않고 각각 작동해야 한다. 에디터·일반 브라우저 mock 통과와 토스 기기 검증을 구분한다. 현재 **실환경 검증 미완료**.

그래픽은 추가 외부 에셋 없이 직접 생성한 단순 도형을 우선하며, 패널·버튼·보드·말의 색상 팔레트와 9-slice 도형을 재사용한다. 설정의 작은 라이선스 진입 메뉴와 전문을 읽을 수 있는 스크롤 화면을 유지하며, Noto OFL/uGUI UCL 원문을 축약하지 않는다. 메뉴·닫기 등 사용자 UI 문구는 Excel Strings의 한국어/영어 키로 관리하고 라이선스 원문은 그대로 유지한다. 생성 결과·출처·해시는 Client 산출물과 대조해 기존 [권리 대장](../../Licence/ASSET-REGISTER.csv)의 변경 항목에 반영했다.

**참고 화면 분석 후 변경:** [새 단계 선택/도움말 계약](REFERENCE-UX.md)에도 위 호스트·44px·라이선스 조건을 그대로 적용한다. 아래 통과 기록은 기존 본문30 빌드의 증거이며, 이후 5×2 단계 노드·그림 도움말·결과 강조 변경의 통과를 뜻하지 않는다. 새 저장 프리팹·한영 캡션·고정 버튼 비중첩과 라이선스 화면을 새 빌드에서 다시 확인한다.

## 권리 확인 상태

- 기존 `LegalNotices.txt`에 Noto 정합 OFL 고지, uGUI LICENSE, UCL 전문이 각 Client 원문과 줄바꿈 정규화 후 전부 포함됨을 재확인했다. 기존 Noto 폰트와 `Disc.png` 해시는 대장과 같다.
- `GameApp`의 설정→라이선스 열기/닫기 연결, 전문 TextAsset 로드, 저장된 NoticesModal의 세로 ScrollRect·ContentSizeFitter와 `legal_title` 한국어/영어 바인딩을 확인했다. Client의 새 [한국어 설정](../../Client/Evidence/host-ko-320-settings.png)·[영어 설정](../../Client/Evidence/host-en-320-settings.png)에서 작은 메뉴, [본문 끝](../../Client/Evidence/host-ko-320-notices-bottom.png)에서 uGUI 고지 끝을 확인했다. 본문을 30단위로 키운 최종 저장본의 제공 [검증 기록](../../Client/Evidence/host-ui-verification.json)은 전체 49/49, 경계 수정 후 8/8, 본문 확대 후 UI 1/1을 구분한다. 마지막 검사는 ko/en·390→320→390 모의 호스트에서 본문 TextAsset 전체 일치와 스크롤 끝 도달을 포함한다. **이는 Editor 자동 검사/캡처이며 최종 브라우저 포인터 조작과 토스 실기기 검증은 별도**다.
- 새 `SoftUiBuilder.cs`가 만든 `Art/SoftUI/RoundRect96.png`(96×96, 1115 bytes)와 `SoftDisc128.png`(128×128, 2597 bytes)의 픽셀을 생성식과 별도로 대조했다. 오차는 채널당 1/255 이내이며 외부 이미지 입력은 없다. 저장 프리팹의 sprite GUID 참조와 사각형의 26px 9-slice 경계를 확인했고, 기존 `Disc.png`는 보존됐다. 출처·SHA-256은 권리 대장의 `ART-SOFT-001/002`, 생성 코드는 `CODE-003`에 기록한다. 이는 자산 생성/참조 증거이며 최종 빌드·실기기 UI 통과 증거와 구분한다.
- **21:04 KST Client 기록 및 21:08 KST 총괄 최종 기록 기준 대조:** Client 소스·그래픽·Android/WebGL 빌드 해시가 제공 검증 기록과 일치한다. APK(`05E70261…B3EE4B8D`)의 `assets/Notices/` 8개 파일을 원본과 바이트 단위로 비교했고 Noto CJK OFL·uGUI LICENSE·UCL 전문을 포함한다. APK 내부 packed runtime 전수 분석이나 실기기 열람 검사는 아니며, 재빌드 시 다시 대조한다. 대장은 기존 기록을 보존하면서 관련 항목을 갱신한 **62행·16열**이다.
- **총괄 최종 브라우저 재검사 통과:** [실행 기록](../Management/Evidence/host-ui-browser-20261014/README.md)과 [최종 산출물 해시](../Management/Evidence/host-ui-browser-20261014/final-artifacts.json)를 대조했다. 본문 30단위 최종 빌드를 다시 로드한 320×568에서 라이선스 진입→[처음](../Management/Evidence/host-ui-browser-20261014/final-ko-notices-top-320.jpg)→[마지막 uGUI 문단](../Management/Evidence/host-ui-browser-20261014/final-ko-notices-bottom-320.jpg)까지 드래그→X 닫기→390 복귀가 확인됐다. 기본 실행의 mock 숨김과 관찰 구간 콘솔 오류·경고 없음도 총괄 기록에 있다. 이전 본문 24단위 빌드의 대국·한영·도움말 기능 검사와 이번 최종 본문 재검사를 한 번의 같은 빌드 시험으로 합치지 않는다. 기획 역할은 증거 파일과 해시를 검토했으며 브라우저를 직접 조작하지 않았다. **실제 토스 SDK/호스트·Android/iOS 기기 검증은 미완료**다.
