# 서버 연결 검증 — 2026-10-16

| 검사 | 결과 |
| --- | --- |
| Workers 요금제 | 같은 로그인 계정의 대시보드에서 Free / $0 / Current plan 확인 |
| 계정 API/권한 | Wrangler OAuth 인증 성공, Workers/D1 권한 확인. 구독 API 403은 무료 증거로 사용하지 않음 |
| TypeScript | `npm run check` 통과 |
| 자동 테스트 | `npm test` 10개 통과. D1 오류/한도·스키마 불일치, CORS, 비활성 결제·쓰기 경로 검사 |
| 로컬 D1 | migration 적용 후 실제 Workers 런타임에서 스키마 행 읽기 성공 |
| 배포 준비 | `wrangler deploy --dry-run` 성공, 번들 2.24 KiB / gzip 1.00 KiB |
| 원격 D1 | 새 전용 DB APAC 생성, migration 0001 적용 성공 |
| 개발 Worker | 버전 `4cd54545-76f5-457f-af45-d2f79151210c` 배포 성공 |
| 로컬 HTTP | [local-http.json](local-http.json), 5개 통과 |
| 원격 HTTP | [remote-http.json](remote-http.json), 5개 통과. `/v1/status` 200, 결제 비활성 |

실패 처리 검사 중 D1 오류는 단위 테스트 주입이다. 실제 계정 무료 한도를 소진하지 않았다. 신규 결제, 소유권·서명 권한, 환불·복구, 클라이언트 오프라인 권한 유지, 토스 mTLS, 실제 CPU/부하, 실기기·사람 플레이는 미검증이다. 상태 확인 서버 연결 성공과 구매 서버 출시 완료를 구분한다.

로컬 검증의 첫 실행은 샌드박스의 로그 경로/네트워크 제한으로 실패했다. 로그를 Server/.local로 제한하고 승인된 실행 환경에서 다시 수행해 성공했다. 기본 npm 조회도 DNS 제한으로 실패한 뒤 승인된 환경에서 고정 버전을 확인·설치했다. 실패를 통과로 기록하지 않았다.
