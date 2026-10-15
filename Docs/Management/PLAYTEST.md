# 개발판 실행 안내

## 최신 외부 테스트 — 2026-10-16

**[친구 테스트용 4목 플레이](https://shares-country-steel-fridge.trycloudflare.com/)**

**01:34 KST 수정 빌드:** iPhone 카카오톡 제보에 대응한 짧은 효과음 변환 보호와 모바일 중복 여백·추가 축소 수정이 반영됐다. 최신 AI·도트 그래픽·Easy Lemon은 유지한다. WebGL 오류 0·경고 0. 공개 HTTPS에서 대사, 착수·AI 응수, 재도전 확인과 390×664/390×844 배치를 확인했다. 게임 오류 로그 0. **iPhone 카카오톡 실제 재시험은 아직 대기 중**이다. 기존 게임 창을 닫고 같은 주소를 다시 열어 테스트한다. [최신 수정·배포 증거](Evidence/ios-playtest-fix-20261016/README.md) · [최초 배포 기록](Evidence/external-playtest-20261016/README.md).

이 PC와 테스트 서버/터널이 켜져 있어야 접속된다. 절전·종료 시 끊기는 임시 주소이며 첫 로딩에 약 60MB를 내려받는다. 친구는 설치 없이 브라우저에서 열면 된다. 실제 휴대폰과 토스 앱은 별도 검증 대상이다. 실광고·실결제는 활성화하지 않았다.

첫 실행은 자동 첫 대국이다. 열을 선택하고 **놓기**를 누른다. 무입력 시 제한 시간이 끝나면 자동으로 둔다. 설정은 일시정지에서 로비로 나온 뒤 우측 상단 톱니 버튼으로 연다. 테스트 피드백은 기기/브라우저, 막힌 화면, 단계·Rating, 체감 난이도, 다시 하고 싶은지를 함께 받는다.

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
