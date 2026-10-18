# 첫 플레이 방법 안내 유지

2026-10-18 KST. 구현·검증·WebGL 빌드 완료. Pages 커밋 `82401bd`는 사용자 푸시 대기이며 현재 공개본 `b0e2901`에는 아직 이 수정이 없다.

## 원인과 변경

기존 코드는 `stagegame.progress.v1` 저장 키 유무로 처음 안내를 판단했다. 첫 방문만 했거나 설정/일시 정지 저장이 발생해도 이미 안내를 확인한 것으로 취급할 수 있었다. 초대 링크가 멀티 패널을 열면서 도움말을 닫는 경로도 있었다. 사용자의 A 기기 저장 내용을 직접 조사한 것은 아니며, 코드의 재현 가능한 누락 경로를 수정했다.

- 저장 형식을 유지하며 `rulesAcknowledged`를 추가했다. 대국을 아직 마치지 않은 사용자는 도움말을 직접 닫기 전까지 다음 실행에서도 안내를 받는다.
- 이미 대국을 완료하거나 스테이지를 진행한 기존 사용자는 처음 안내를 반복하지 않는다.
- 초대/멀티 진입에서도 처음 안내를 멀티 패널 위에 표시한다. 멀티 도움말은 서버 규칙인 한 턴 60초·시간 초과 패배를 안내하며 싱글의 30초 자동 착수 설명과 구분한다.
- 새 문구는 Excel Strings의 `mp_help_drop_caption` 한영 키다. 기존 489개 문자열은 보존하고 총 490개가 됐다.

## 검증

- `reproduction.xml`: 변경 전 새 회귀 검사 1개 실패. 저장은 있지만 대국을 하지 않은 방문자의 안내가 누락되는 현상을 재현했다.
- `unity-tests.xml`: 최종 Unity EditMode 4/4 통과. 저장/재실행/직접 닫기/초대 진입 위계/한영 멀티 문구, 기존 최초 안내와 대국 도움말의 타이머·선택 보존을 검사했다.
- `screens/`: 저장 방문의 1080×1920 안내 및 멀티 한영 390×844 렌더링. 실제 기기 검사가 아닌 Unity 캡처다.
- 실제 새 WebGL을 `http://localhost:8080/`에서 실행했다. 최초 안내(`browser-fresh-rules.png`) → 닫지 않고 새로고침해도 유지(`browser-unacknowledged-reload.png`) → X로 닫은 뒤 새로고침하면 로비(`browser-acknowledged-reload.png`)를 확인했다. 브라우저 저장 값 직접 열람·주입 없이 조작했다.
- 데이터 검사 47/47 및 exporter 재현성 통과. Excel 기존 셀·서식과 다른 생성 데이터 보존도 확인했다. 세부 작업 로그는 `.work/first-rules-persistence-20261018/strings/`에 남겼다.
- `data-tests.txt`와 `data-verification.json`은 데이터 담당 검증의 보존본이다. 해당 JSON의 Client 변경 없음은 담당 작업 종료 시점 기준이며, 이후 총괄이 생성된 490개 문자열을 Client에 동기화해 최종 검사·빌드했다.
- `build-WebGL.txt`: 성공, 오류 0·경고 0, 91,663,749바이트.
- `published-files.json`: 웹 빌드 23파일과 Pages 복사본 해시 일치. `source-files.json`은 빌드 당시 소스·설정·Excel 해시다. 자동 검사와 Unity 캡처가 사용자의 A 기기 재시험을 대신하지 않는다.

사용자 푸시 명령:

```powershell
git -C C:\Users\JW\.codex\worktrees\github-pages\StageGame push origin codex/github-pages
```

푸시 후 `verify-public.py`로 Actions 및 공개 파일을 대조한다. 이번 빌드에는 광고 제거 홍보나 광고 빈도 제안을 구현하지 않았다.
