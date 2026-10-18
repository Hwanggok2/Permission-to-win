# 공개 멀티 서버 연결·웹 빌드 검증

2026-10-18 KST. **멀티 서버·WebGL 공개 갱신 완료. 사용자가 Pages 커밋 `1fdfd83`을 푸시했고 Actions 성공·공개23파일 일치를 확인했다. 공개 URL에서도 친선전 초대 입장·서버 착수를 확인했다.**

- 게임: https://hwanggok2.github.io/Permission-to-win/
- 서버: `wss://permission-to-win-multiplayer-dev.hsj2014528.workers.dev/v1/play`
- Worker 버전: `dc7452ca-2df7-46fd-b3b0-8025c2b6870f`
- 사용자 대시보드에서 Workers Free Active 확인: `workers-free-confirmation.png`.
- 공식 Wrangler 인증만 사용했다. 키·저장된 인증정보 파일 직접 열람 없음. 기존 구매 Worker 변경 없음.
- Pages 브랜치 `codex/github-pages`, 정확한 SHA는 `deployment-commit.txt`. 에이전트는 로컬 커밋만 만들었고 GitHub 푸시는 사용자가 수행했다.

## 실행한 검증

| 항목 | 결과 |
| --- | --- |
| 최종 Unity6000.3.15f1 WebGL 빌드 | 성공, 오류0·경고0, 18.10247초, 보고서91,652,726바이트 |
| 공개 endpoint 저장 프리팹·멀티 UI 검사 | 기존2/2 통과 (`unity-endpoint.xml`) |
| 공유 안내가 경기 결과를 가리는 문제 회귀 검사 | 수정 후1/1 통과 (`unity-share-feedback.xml`), 한영·390/320 폭 포함 |
| 실제 공개 WSS 검사 | 11개 통과: 인증·입장설정·차례·레이팅·리매치·재접속·퇴장·무작위 매칭 |
| WebGL 브라우저와 공개 서버 연동 | 초대 URL 입장, OFF 설정 확인, 선공·돌 색, 7수 대국, 양쪽200 유지, 리매치 요청 문구, 양쪽 수락 후2라운드, 퇴장 문구, 로비 복귀 확인 |
| 브라우저 콘솔 | 오류0·경고0 |
| GitHub Actions와 공개23파일 해시 | 성공, 23/23 일치 (`public-deployment-verification.json`) |
| 실제 공개 게임 URL | 초대 OFF 미리보기·입장·첫 착수와 응수·상대 돌 테두리 확인 (`public-match.png`) |

브라우저 연동은 새 빌드를 localhost에서 제공하고 실제 공개 WSS 서버에 연결해 테스트 참가자와 대국했다. `invite-preview.png`, `unrated-result.png`, `rematch-request.png`, `rematch-started.png`, `opponent-left.png`, `returned-lobby.png`가 화면 증거다. 복사 성공 안내는 확인했지만 IAB가 OS 클립보드 내용을 반환하지 않아 붙여넣기 완료까지 검증했다고 기록하지 않는다. 실제 카카오톡·Android 공유, 모바일 백그라운드 복귀, 부하·무료 한도 소진은 별도 실기기/운영 검사 대상이다.

## 빌드 재현과 배포 확인

`published-files.json`은 최종 배포23파일의 크기/SHA-256, `source-files.json`은 빌드 입력575파일이다. Unity 생성 JS의 공백은 산출물 해시를 보존하기 위해 수정하지 않았다. `build-WebGL.txt`는 최종 빌드 보고서다. UI의 복사/공유 안내는 대국 시작 때 지우며, 경기 시작 후 늦게 도착한 공유 결과 콜백도 무시한다.

`server-deployment.json`은 서버 배포·Free 확인·공개 검사 기록, `verification.json`은 전체 상태다. `verify-public.py`는 인증정보 없이 GitHub Actions의 정확한 커밋과 공개 파일23개의 HTTP 응답·해시를 비교한다. 성공 결과는 `public-deployment-verification.json`에 저장한다.

직전 로컬 멀티11항목·서버37·데이터47·Unity33개 고유 테스트는 [이전 기록](../multiplayer-20261017/README.md)이며 실제 공개 검증과 구분한다. 위 검사는 실제 사람 플레이·스토어 출시 검증을 대신하지 않는다.

공개 배포: [GitHub Actions](https://github.com/Hwanggok2/Permission-to-win/actions/runs/39048448496). 실제 공개 URL 검사도 브라우저 콘솔 오류·경고0.
