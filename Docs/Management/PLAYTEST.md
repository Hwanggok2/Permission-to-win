# 개발판 실행 안내

2026-10-16 **이겨도 됩니까? / Permission to Win** 제목과 **Pretendard**를 적용한 WebGL 버전으로 공개 링크를 갱신했다. 저장소·주소는 대문자 P가 있는 `Permission-to-win`을 사용한다.

## 최신 외부 테스트 — 2026-10-16

**[이겨도 됩니까? 플레이](https://hwanggok2.github.io/Permission-to-win/)**

사용자가 배포 커밋 `3d6c2a9`를 푸시했고 [Pages Actions 성공](https://github.com/Hwanggok2/Permission-to-win/actions/runs/38852414250) 및 공개 파일23/23 해시 일치를 확인했다. 제목·UI 폰트·웹 로딩 글꼴·폰트 라이선스 고지가 포함됐다. PC 중앙 세로 화면과 모바일 전체 폭을 유지한다. 현재 저장 소스의 상세 패널 중앙 배치와 초반 난이도 수정도 포함한다. [빌드·공개 검증 기록](Evidence/title-font-web-20261016/README.md).

WebGL 빌드는 13:38 KST 완료, 오류0·경고0. 게시 파일 합계91,553,486바이트(약92MB)이며 브라우저 캐시에 따라 실제 전송량이 달라진다. 로컬 한영·모바일 검사와 공개 PC/390px 화면·착수·AI 응수·로비 복귀를 확인했다. 실제 휴대폰·Toss 앱·실광고·실결제 검증은 별개다. 노트북을 꺼도 GitHub Pages 주소로 접속할 수 있다.

첫 실행은 자동 첫 대국이다. 열을 선택하고 **놓기**를 누른다. 무입력 시 제한 시간이 끝나면 자동으로 둔다. 설정은 일시정지에서 로비로 나온 뒤 우측 상단 톱니 버튼으로 연다.

## 이전 로컬 실행 기록

2026-10-14 기준. 실제 광고·원스토어 제출·토스 `.ait`를 포함하지 않은 개발판이다.

## 바로 실행

- Android APK: `C:/Github/StageGame/Client/Builds/Android/StageGame-development.apk` (34,676,997 bytes). arm64, 개발 패키지 `com.stagegame.prototype`. 실제 Android 기기 설치/검증은 아직 하지 않았다.
- 이 PC에서 WebGL 미리보기: http://127.0.0.1:8765/ . 총괄이 시작한 로컬 서버가 실행되는 동안만 접근 가능하다. 외부에 게시한 사이트가 아니다.
- Unity: 6000.3.15f1로 `Client`를 열고 `Assets/StageGame/Scenes/Main.unity` 실행. UI는 저장된 씬/프리팹이다.

로컬 서버를 다시 켜려면 PowerShell에서 아래를 실행한다. 사용 중인 포트가 있으면 기존 서버를 확인하고 중복 실행하지 않는다.

```powershell
& 'C:\Users\JW\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' -m http.server 8765 --bind 127.0.0.1 --directory 'C:\Github\StageGame\Client\Builds\WebGL'
```

실행한 터미널에서 Ctrl+C로 종료한다. Python이 다른 경로에 설치되면 해당 실행 파일로 바꾼다.

## 확인한 범위

데스크톱 브라우저에서 실제 입력으로 로비→대국→승리→해금→새로고침 복원을 확인했다. 390×844 크기에서 전체 화면과 입력을 확인했지만, 휴대폰 브라우저/토스 앱이나 Android 기기 검증으로 취급하지 않는다. 이 미리보기 주소의 테스트 저장에는 2스테이지 해금 기록이 남아 있을 수 있다.

광고를 요청하면 현재 준비된 광고가 없다는 안내가 나온다. 모의 광고 완료는 Unity Editor 테스트 전용이다. 광고가 없어도 대국·무료 재도전은 가능하다.

사람 플레이 검증은 [관찰 계획](VALIDATION.md)에 따라 판 길이·이해도·자발적 재도전 등을 기록한다. 결과와 파일 해시는 [클라이언트 검증](../../Client/Evidence/verification.json), [QA 상태](../QA/STATUS.md)에 있다.
