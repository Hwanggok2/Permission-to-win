# 개인정보·등급 설문용 구현 사실 초안

2026-10-14. 공개 처리방침이나 법적 검토 완료 문서가 아니다. 현재 개발판의 코드/패키지/권한을 확인한 사실 자료이며, 실제 광고·토스 SDK 도입 뒤 다시 확인한다.

## 현재 로컬 저장

`Client/Assets/StageGame/Scripts/Core/ProgressStore.cs`의 `stagegame.progress.v1`에 저장한다.

- 저장 형식 버전, 최고 해금 스테이지, 완료 판 수, 효과음 설정.
- 전면 광고 간격 관리용 마지막 판/UTC 시각. 현재 NoFill이므로 실제 광고 송출 기록이라는 뜻은 아니다.
- 결과 중복 반영 방지용 판별 ID 목록. 계정이나 사용자 ID가 아닌 로컬 대국 구분값이다.
- 저장 형식 오류 때 복구용 기존 문자열을 `.recovery` 키에 보관할 수 있다.

온라인 계정, 서버 동기화, 랭킹, 결제, 보상 금전은 구현하지 않았다. Android 저장 삭제/재설치, 브라우저 사이트 데이터 삭제, 기기 변경에 따른 실제 유지·삭제 동작은 실환경 확인 후 이용자 안내를 확정한다. 현재 대국 중 보드 상태는 저장하지 않으며 나가기 전에 이를 안내한다.

## 패키지·통신·권한 확인

- `Client/Packages/manifest.json`: uGUI 2.0.0, Test Framework 1.6.0, Unity 기본 audio/imageconversion/jsonserialize/ui/unitywebrequest 모듈. 실제 광고 SDK·앱인토스 SDK·별도 분석 SDK 없음.
- 자체 게임 스크립트에서 `UnityWebRequest`, `HttpClient`, `WebRequest`, `Analytics`, `Application.OpenURL`, URL 문자열 호출을 검색해 외부 통신 코드를 발견하지 않았다. 이것은 전체 엔진/의존성의 네트워크 미사용이나 개인정보 미수집을 입증하는 동적 검사가 아니다.
- WebGL 로더는 호스팅 위치에서 게임 파일을 내려받는다. 공개 호스팅의 접속 로그/운영 주체는 아직 정해지지 않았다. 현재 확인한 호스트는 이 PC의 `127.0.0.1`이다.
- Android 개발 APK SHA-256: `8A809C7B1942E16D3B846721118A49D4E557CD24EA424E66D07C4D7C908C5688`. Android SDK 36.0.0의 `aapt dump permissions` 결과는 `android.permission.INTERNET`, 앱 자체 `com.stagegame.prototype.DYNAMIC_RECEIVER_NOT_EXPORTED_PERMISSION` 선언/사용이다. 이 검사는 해당 개발 APK에 한정된다.
- 실기기 트래픽·SDK 실제 요청·동의 전 동작은 검사하지 않았다. 개발 빌드의 권한을 최종 릴리스 권한으로 확정하지 않는다.

## 등급 설문에 사용할 게임 사실

7×6 보드에 돌을 떨어뜨려 네 개 이상을 연결하는 로컬 AI 대전이다. 현재 콘텐츠는 30스테이지, 승리 해금, 무료 재도전이며, PvP·채팅·사용자 게시물·게임머니 구매·베팅·현금성 보상은 없다. 리워드의 계획 용도는 한 판에 한 번 전술 힌트다. 실제 등급/웹보드 해당 여부는 플랫폼의 분류 절차로 확인한다.

## 공개 문서 확정에 남은 입력

운영자/문의 연락처, 적용 광고·토스 SDK 버전과 실제 처리 항목, 대상 이용자와 동의 동선, 공개 호스트·지원/삭제 절차, 보유·파기 기준, 제공·위탁·국외이전 해당 여부를 확정해야 한다. 빈 값을 임의로 채우거나 현 개발판만 보고 최종 앱을 '데이터 미수집'으로 신고하지 않는다. 상세 공식 근거는 [출시 요건](../Design/RELEASE-REQUIREMENTS.md)에 있다.
