# 제목·Pretendard 웹 배포 — 2026-10-16

상태: **사용자가 Pages 커밋을 푸시했고 배포 성공·공개 파일23/23 해시 일치·실제 공개 브라우저 검증까지 완료했다.**

- 게시 대상: [이겨도 됩니까?](https://hwanggok2.github.io/Permission-to-win/)
- 준비된 배포 커밋: `3d6c2a990616453974c871682c1f749c25f49b33`
- 배포 워크트리: `C:/Users/JW/.codex/worktrees/github-pages/StageGame`, 브랜치 `codex/github-pages`.
- 사용자가 배포 커밋을 직접 푸시하겠다고 답했다. 에이전트의 GitHub 로그인 대기는 취소했으며 에이전트가 푸시한 것으로 기록하지 않는다.
- Unity 6000.3.15f1 WebGL 개발 빌드: 오류0·경고0, 56.48초. 게시 파일23개, 91,553,486바이트. 기존 Pages 워크플로와 공개 주소를 유지한다.
- 제목·Pretendard·네오둥근모 고지가 포함된 현재 저장 소스로 빌드했다. 현재 소스의 상세 패널 중앙 배치와 앞선 초반 난이도 데이터 수정도 포함된다. Server 폴더는 Unity 입력과 배포 커밋에 포함하지 않는다.
- 빌드 전후 입력559개 해시 동일. 폰트 검증 당시 소스 해시 일치, Excel exporter `--check`와 데이터46/46 통과. 폰트 적용 때의 Unity12/12는 이전 실행 증거로 구분한다.
- 웹폰트 Regular/Bold WOFF2, 폰트별 라이선스 전문이 원본과 같은 바이트로 포함됐고 Git 배포 커밋의 파일23개도 빌드 결과와 일치한다.
- Unity는 `PRODUCT_NAME`을 HTML 템플릿에 넣을 때 물음표를 제거한다. Excel에서 생성된 `ui-text.js`가 즉시 정확한 제목을 설정하며 실제 브라우저 탭·접근성 이름·게임 화면의 **이겨도 됩니까? / Permission to Win**을 확인했다.
- 실제 로컬 브라우저: PC1280×720 중앙 배치, 첫 규칙 안내, 열 선택·놓기·AI 응수, 제한시간 자동 착수, 로비 복귀, 모바일390×844 설정·라이선스, 영어 전환·320×568 로비 확인. warn/error0. `local-browser.json`, `screens/` 참조.
- 실제 휴대폰·Android 빌드·실광고/실결제 검증은 수행하지 않았다. 공개 배포 확인과 실제 기기 검증은 별개다.

`verify-public.py`로 해당 커밋의 [Pages Actions 성공](https://github.com/Hwanggok2/Permission-to-win/actions/runs/38852414250)과 공개 파일23개 SHA-256 일치를 확인했다. 공개 PC1280×720와 모바일390×844에서 새 제목·Pretendard·첫 도움말·착수·AI 응수·일시정지·나가기 확인·로비 복귀를 직접 검증했다. warn/error0. `public-deployment-verification.json`, `public-browser.json`, `screens/public-mobile-lobby.jpg`와 `screens/public-mobile-match.jpg` 참조.
