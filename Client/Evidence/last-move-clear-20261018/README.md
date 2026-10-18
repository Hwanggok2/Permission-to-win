# 내 착수 후 상대 돌 강조 해제

2026-10-18 KST 사용자 요청. 싱글플레이와 멀티플레이에서 내가 돌을 놓으면 이전 상대 수의 흰 테두리를 즉시 숨기도록 변경했다. 열 선택만으로는 숨기지 않으며 상대가 다음 수를 두면 새 돌을 강조한다.

- 싱글: 보드의 가장 최근 착수가 상대 수일 때만 표시한다. 시작 국면의 사전 배치 돌은 제외한다. 되돌리면 복원된 판의 마지막 착수에 맞춰 표시한다.
- 멀티: 선공과 누적 착수 순서로 마지막 플레이어를 판별한다. 착수 전송 중에도 숨기고 거절된 착수는 표시를 복원한다. 양쪽 좌석과 선공 조합을 모두 검증했다. 서버나 레이팅 규칙 변경은 없다.
- Unity 실제 실행 2/2 통과, 실패·건너뜀0: `unity-results.xml`. 내 착수 직후·중복 서버 응답·거절 복원·다음 응수·최종 상대 수·되돌리기·새 판을 검사했다.
- WebGL 빌드 성공, 오류0·경고0, 17.71079초, 보고서91,652,829바이트: `build-WebGL.txt`.
- Pages 커밋 `e845e2d`, 브랜치 `codex/github-pages`. 사용자 푸시 후 Actions39049252671 성공과 공개23파일 해시 일치를 확인했다. `public-deployment-verification.json`이 확인 증거다.

배포23파일 해시는 `published-files.json`, 입력575파일 해시는 `source-files.json`에 기록했다. `deployment-commit.txt`는 정확한 커밋 SHA다. 이전 멀티 전체 검증은 [공개 연결 기록](../multiplayer-public-20261018/README.md)을 따른다.

사용자 푸시 명령:

```powershell
git -C C:\Users\JW\.codex\worktrees\github-pages\StageGame push origin codex/github-pages
```

2026-10-18 `verify-public.py` 실행으로 GitHub Actions 성공과 공개 파일23/23 해시 일치를 확인했다. 다음 멀티 UI 리모델링 빌드는 별도 기록을 따른다.
