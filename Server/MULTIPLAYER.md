# 멀티 배틀 서버

**2026-10-18 공개 멀티 연결 완료:** 사용자 대시보드의 `Workers Free · Active` 확인 후 공식 Wrangler로 별도 멀티 Worker를 배포했다. 사용자가 Pages 커밋 `1fdfd83`을 푸시했고 GitHub Actions 성공·공개23파일 해시 일치를 확인했다. 공개 WSS 11개 검사, 실제 WebGL 초대 입장·레이팅 OFF 대국·리매치·퇴장 알림, 공개 게임 주소의 초대 입장·서버 착수를 검증했다. 초대 복사 안내가 경기 결과를 가리는 문제도 수정했다. 최종 WebGL 오류·경고0, 추가 Unity 회귀1/1 통과. 키·저장된 인증정보 직접 열람과 기존 구매 Worker 변경은 없었다. [공개 게임](https://hwanggok2.github.io/Permission-to-win/) · 증거: `Client/Evidence/multiplayer-public-20261018/README.md`.

2026-10-18. 별도 Worker `wrangler.multiplayer.jsonc`와 `src/multiplayer/`다. 기존 구매 API 설정과 원격 배포는 변경하지 않았다. **멀티 Worker·게임 사이트 공개 갱신·브라우저 연결 검증 완료.** 기능·Elo·시간·무료 운영 제한은 [계약](../Docs/Management/MULTIPLAYER-CONTRACT.md)을 따른다.

## 로컬 실행

Node24 이상과 기존 `npm ci` 의존성을 사용한다. Server 폴더에서 실행한다.

```powershell
npm run check
npm test
npm run build:multiplayer
npm run dev:multiplayer
```

마지막 명령이 로컬 서버를 실행한다. 다른 터미널에서 `npm run smoke:multiplayer`를 실행한다. smoke는 localhost만 허용하며 기본 `ws://127.0.0.1:8788/v1/play`에 접속한다. 테스트용 게스트가 로컬 SQLite에 남을 수 있다. Node의 TypeScript parameter property 변환을 위해 테스트 스크립트에 `--experimental-transform-types`를 지정했다.

Unity의 `StageGame/Apply Multiplayer and Last Move`는 Board/OnlinePanel/RankAppRoot 프리팹을 저장한다. 현재 저장된 OnlineGameController.endpoint는 `wss://permission-to-win-multiplayer-dev.hsj2014528.workers.dev/v1/play`다. 개발 시험은 인스펙터에서 로컬 주소를 지정하거나 자동 검사 환경변수 `STAGEGAME_TEST_MULTIPLAYER_URL`을 사용한다. 자동 검사는 endpoint와 기존 게스트 저장값을 복원한다. 일반 화면에는 개발용 endpoint를 표시하지 않는다.

## 연결 계약

`/v1/play` WebSocket 연결 후 첫 메시지는 `auth`와 선택적 기존 token이다. 신규는 서버 발급 token을 응답받고 이후 기기 저장소에 보관한다. URL에 인증 token을 넣지 않는다. 서버는 JSON `state`를 보내고 클라이언트는 `queue`, `cancel`, `create`, `preview`, `join`, `setRated`, `move`, `rematch`, `leave`, `sync`를 보낸다. move/rematch에는 현재 roomId/round, move에는 seq/column을 포함한다. join에는 초대와 확인한 rated 값을 보낸다. 착수/점수의 권위는 서버에 있다.

현재 WebSocket은 동일 Origin 목록 또는 Origin이 없는 네이티브 연결만 허용한다. `MULTIPLAYER_ENABLED=false`는 신규 연결을503으로 막는 운영 스위치다. 신규 등록/메시지/소켓/방 수 제한과 SQLite 거래로 기본 자원·중복 처리를 제한한다. 실제 무료 소진·대규모 부하·모바일 백그라운드 복귀는 미검증이다.

## 공개 연결 상태

공개 설정은 Git 제외 `wrangler.multiplayer.remote.jsonc`에 있으며 `workers_dev:true`, `preview_urls:false`, 허용 Origin은 GitHub Pages 및 localhost/127.0.0.1:8080이다. 기본 `wrangler.multiplayer.jsonc`는 로컬 전용 설정을 유지한다. 공개 버전은 `dc7452ca-2df7-46fd-b3b0-8025c2b6870f`다. 기존 구매 Worker의 설정/라우트는 재사용하거나 덮어쓰지 않았다.

재배포는 공식 Wrangler만 사용하고 인증정보 파일을 직접 읽지 않는다. `scripts/publish-multiplayer.ps1 -FreePlanConfirmed`는 준비 빌드 해시와 Free 확인 후 멀티 서버만 배포·검사하며 Git 푸시는 하지 않는다. 사용자는 준비된 `codex/github-pages` 브랜치를 직접 푸시한다.

[공개 서버·웹 빌드 기록](../Client/Evidence/multiplayer-public-20261018/README.md) · [이전 로컬 검사](../Client/Evidence/multiplayer-20261017/README.md).
