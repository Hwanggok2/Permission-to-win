# 이겨도 됩니까?

제목과 기본 UI의 글꼴은 2026-10-16 요청에 따라 **Pretendard**로 적용했다. 네오둥근모 Pro는 선택 가능한 도트 글꼴 자산으로 추가했고, 출처·라이선스·UI 검증은 [글꼴 적용 기록](TYPOGRAPHY.md)을 따른다.

2026-10-16 사용자 확정. 게임의 한국어 제목은 **이겨도 됩니까?**, 영문 표시는 **Permission to Win**, 제작사는 **사부작대작전**, 저장소 이름은 `Permission-to-win`이다. 사용자가 대문자 P를 유지하도록 최종 확정했다.

Excel `Strings`의 `game_title`, `client_game_title`, `game_brand`와 `web_canvas_label`을 변경했다. 한국어·영어 8개 셀과 제목 확정 상태를 설명하는 메모 1개만 바꿨으며, 기존 난이도 작업·대사·광고·서식은 보존했다. JSON과 Unity Resources를 동일하게 생성·동기화했다.

저장된 로비·대국 프리팹의 제목, Unity productName, 웹 로딩·탭·캔버스 접근성 이름에 반영했다. 이후 초기 설정을 실행해도 Excel 제목을 읽도록 ProjectBuilder를 갱신했다. 내부 네임스페이스·로컬 `C:/Github/StageGame` 경로·Android 앱 식별자·저장 키는 유지한다.

데이터 46/46, Unity 관련 11/11 검사 통과. 한국어·영어의 PC·모바일 8종 캔버스 크기에서 로비·대국과 스크롤 끝점 64장을 캡처했다. 320px의 한영 제목과 버튼 배치를 시각적으로 확인했다. [검증 기록](../../Client/Evidence/title-20261016/verification.json), [한국어 대국](../../Client/Evidence/title-20261016/screens/ko-mobile-320x568-match.png), [영어 로비](../../Client/Evidence/title-20261016/screens/en-mobile-320x568-lobby.png).

제목 적용 당시에는 소스·에디터 검증까지만 진행했다. 이후 2026-10-16 사용자 요청으로 Pretendard와 함께 WebGL을 재빌드했고, 사용자가 Pages 커밋 `3d6c2a9`를 푸시해 공개 반영됐다. [최신 배포 기록](../Management/Evidence/title-font-web-20261016/README.md). 새 APK 빌드는 실행하지 않았다.

원격 저장소는 조사 시 이미 `Hwanggok2/Permission-to-win`으로 변경돼 있었다. 사용자 요청에 따라 대문자 P를 유지하고 로컬 origin을 `https://github.com/Hwanggok2/Permission-to-win.git`으로 맞췄다. 저장소 이름 변경을 위한 추가 로그인은 필요하지 않다.
