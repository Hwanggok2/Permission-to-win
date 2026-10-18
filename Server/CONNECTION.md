# Workers + D1 개발 연결

2026-10-16 사용자 지시로 서버 작업을 재개했다. 기존 구성인 Cloudflare Workers Free(TypeScript) + D1 Free를 유지한다. 이번 변경은 `Server/`에만 있다. 전체 요약과 공통 계약에 반영할 현재 상태는 **개발 인프라 연결 완료 / 결제 기능 미구현 / Client 연결 미실행 / 출시 미완료**다.

## 실제 연결 상태

| 항목 | 현재 값 |
| --- | --- |
| Worker | `stagegame-purchase-api-dev` |
| 개발 주소 | https://stagegame-purchase-api-dev.hsj2014528.workers.dev |
| D1 | `stagegame-purchases-dev` |
| 바인딩 | `DB` |
| DB ID | `29d48cfd-3e72-4d78-b939-98cfa1002961` |
| 생성 지역 | APAC; 국가 단위 보관 위치 보장은 아님 |
| 배포 버전 | `4cd54545-76f5-457f-af45-d2f79151210c` |
| 스키마 | `0001_connection.sql`, `server_metadata` 한 행 |
| 결제 상태 | 항상 비활성, 플랫폼 어댑터 없음 |

기존 로그인으로 Workers/D1 권한을 확인했다. 구독 API는 현재 토큰 권한 부족으로 403을 반환했으므로, 같은 계정의 대시보드 Workers plans에서 **Free / $0 / Current plan**을 직접 확인했다. API의 `default_usage_model: standard`만으로 무료 여부를 판단하지 않았다. 기존의 다른 앱 Worker/DB는 수정하지 않았다. 인증 토큰과 계정별 원격 설정은 Git에 넣지 않는다.

## API와 Client 계약

- `GET /healthz`: HTTP 200, `{"apiVersion":1,"status":"ok"}`. Worker 자체 실행 여부만 확인하며 D1은 읽지 않는다.
- `GET /v1/status`: D1의 스키마 행을 PK로 한 번 읽는다. 성공하면 HTTP 200과 `status:"ready"`, `purchases:{enabled:false,reason:"PAYMENTS_NOT_CONFIGURED"}`를 반환한다. **ready는 DB 연결 상태이며 결제 가능 상태가 아니다.**
- DB 오류·일일 한도 오류·스키마 누락/불일치는 HTTP 503과 `purchases:{enabled:false,reason:"TEMPORARILY_UNAVAILABLE"}`를 반환한다. `Retry-After: 60`, `Cache-Control: no-store`를 사용하며 내부 오류/주문/토큰을 노출하지 않는다.
- 이번 API는 권한을 발급·회수하지 않는다. 구매 제출/복구/환불 경로도 아직 없다. `/v1/purchases/verify`는 404, 상태 경로의 POST는 405다. 클라이언트가 보낸 `paid` 플래그로 권리를 주지 않는다.
- Client는 네트워크 오류, 비JSON 응답, 1027/1102 등 Cloudflare 오류, 비정상 HTTP 또는 미지원 API 버전이면 신규 구매를 닫아야 한다. `purchases.enabled`를 기존 광고 제거 권한으로 해석하면 안 된다. 오류만으로 마지막 검증 권한을 삭제하거나 오프라인 대국을 막지 않는다. **Client 쪽 캐시·서명·오프라인 유지 동작은 아직 구현/검증하지 않았다.**
- 응답 코드는 기계용이다. 이용자 안내는 향후 Excel Strings ko/en 키로 매핑하며 SDK 상태/개발 설명을 일반 화면에 표시하지 않는다.

원격 `ALLOWED_ORIGINS`는 현재 빈 값이다. Origin 없는 상태 확인 요청만 가능하며 WebGL 페이지에서 직접 호출하는 CORS 연결은 아직 닫혀 있다. 실제 Client 테스트/앱인토스 origin을 확인한 뒤 정확한 주소만 등록한다. CORS는 구매자 인증을 대체하지 않는다. 로컬 설정은 `http://localhost:8080`, `http://127.0.0.1:8080`만 허용한다. 허용된 origin에서도 데이터 쓰기 API는 없다.

## 실행과 원격 설정

Node 24 이상에서 `Server/`를 작업 폴더로 사용한다. 버전은 package-lock.json으로 고정했다. 런타임 npm 의존성은 없고 개발 도구만 설치한다.

```powershell
npm.cmd ci
npm.cmd run check
npm.cmd test
npm.cmd run db:local
npm.cmd run dev
```

기본 `wrangler.jsonc`는 로컬용 식별자를 사용하며 공개 배포를 끈다. 계정별 `wrangler.remote.jsonc`는 이 PC에 저장되어 있고 Git에서 제외한다. 새로운 환경에서는 기본 설정을 복사하고 실제 account_id/DB ID, workers_dev, 승인된 origins를 설정해야 한다. 비밀은 Workers Secrets/mTLS binding에 별도로 보관하며 설정 파일에 넣지 않는다.

```powershell
# 배포 없이 번들·바인딩 검사
npm.cmd run build:check
# 별도 터미널에서 로컬 HTTP 검증
node scripts/smoke.mjs http://127.0.0.1:8787
# 원격 연결 상태 검증 (상태 조회·거부되는 요청만 전송)
node scripts/smoke.mjs https://stagegame-purchase-api-dev.hsj2014528.workers.dev
```

원격 변경은 무료 플랜과 대상 이름을 다시 확인한 후 아래 명령으로 수행한다. 현재 migration은 이미 적용되어 있다. 테스트 DB를 초기화한다는 이유로 원격 DB나 기존 구매 원장을 삭제하지 않는다.

```powershell
node node_modules/wrangler/bin/wrangler.js d1 migrations apply stagegame-purchases-dev --remote --config wrangler.remote.jsonc
node node_modules/wrangler/bin/wrangler.js deploy --config wrangler.remote.jsonc
```

## 무료 한도와 후속 작업

2026-10-16 공식 문서 재확인: Workers Free는 하루 100,000 요청·요청당 CPU 10ms, D1 Free는 하루 읽기 5,000,000행·쓰기 100,000행·총 저장 5GB다. **계정 단위로 다른 앱과 한도를 공유**한다. D1 Free 한도를 넘으면 쿼리가 오류를 반환한다. 자동 유료 전환을 설정하지 않았으며 유료 상품을 추가하지 않았다. [Workers 요금](https://developers.cloudflare.com/workers/platform/pricing/), [D1 요금·한도 초과 동작](https://developers.cloudflare.com/d1/platform/pricing/).

상태 조회는 쓰기를 하지 않는다. Worker 시작 2ms는 배포 도구의 시작 시간이며 결제 처리의 10ms CPU 준수/부하 시험 결과가 아니다. 실제 무료 한도를 일부러 소진하지 않았다. 단위 테스트에서는 D1 한도 오류를 주입해 503·구매 차단·권한 회수 정보 없음·오류 상세 미노출을 확인했다.

다음 구현은 플랫폼 구매자 인증과 주문 소유권 증명, 서버 검증 후 유일 거래 원장/권한 기록, 서명 권한 캐시, 복구·환불·중복·미결 재시도 순서로 진행한다. 토스 상품·앱 설정과 mTLS 인증서/개인키 및 주문 소유권 연결이 필요하다. 원스토어 무광고 초판에서는 광고 제거 상품을 판매하지 않는 공통 계약을 유지한다. 현재 원격 DB에는 구매/개인정보가 없고, 실제 결제·Client 변경·게임 빌드·Git 커밋/푸시는 하지 않았다.
