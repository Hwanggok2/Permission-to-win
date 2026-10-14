# 무료 구매 검증 서버 계획

확인일: 2026-10-14. **Cloudflare Workers Free(TypeScript) + D1 Free + Workers Secrets/mTLS binding**을 선택한다. 무료 한도에서 운영하고 소진 시 구매 기능을 중단한다. 게임 규칙·AI·진행은 로컬이다. 자체 회원가입·진행 동기화·플랫폼 간 구매 공유는 제외한다.

## 0원 조건과 대안

- **Cloud Run 기존안과 월 3만원 예산안은 폐기한다.** Google Free Tier도 결제 계정이 필요하고 한도 초과 과금 가능성이 있어 엄격한 0원 조건을 충족하지 않는다. [Google 무료 등급](https://docs.cloud.google.com/free/docs/free-cloud-features)
- Supabase/Firebase Functions는 이번 mTLS 운영 검증 범위에서 제외한다.
- Cloudflare는 **Workers/D1 모두 Free를 유지**하고 유료 전환·유료 부가 서비스를 사용하지 않는다. 서버 주소는 workers.dev를 사용할 계획이다. 무료 서버는 광고/IAP 수수료·등록 비용까지 0원이라는 뜻이 아니다. [Workers 요금](https://developers.cloudflare.com/workers/platform/pricing/)

## 무료 한도와 초과 동작

| 항목 | Free 한도 | 운영 원칙 |
| --- | --- | --- |
| Workers | 계정당 100,000 요청/일, HTTP 요청당 CPU 10ms | 요청 한도 초과 1027, CPU 초과 1102. 검증을 우회하는 fail-open 금지 |
| D1 쿼리 | 계정당 읽기 5,000,000행/일, 쓰기 100,000행/일 | 초과 시 쿼리 오류. 자동 초과 과금으로 처리하지 않음 |
| D1 저장 | 계정 합계 5GB, DB당 500MB, DB 최대 10개 | 한도 소진 시 쓰기/구조 변경 제한. 구매 원장을 임의 삭제하지 않음 |

일일 한도는 UTC 00:00(한국 09:00)에 초기화된다. D1은 읽고 쓴 **행 수** 기준이다. [Workers 제한](https://developers.cloudflare.com/workers/platform/limits/), [D1 요금·초과 처리](https://developers.cloudflare.com/d1/platform/pricing/), [D1 제한](https://developers.cloudflare.com/d1/platform/limits/)

한도 소진/장애 시 신규 구매·복구를 보류하고 도중 결제된 미결 건은 재구매 없이 복구한다. 기존 검증 권한의 서명 캐시와 오프라인 대국은 유지하며 재접속 시 환불을 반영한다. 사용량·실패율 감시, 구매 전 가용성 확인, 인덱스·페이지·재시도 제한을 적용한다. CPU/처리량은 부하 시험 전 미검증이다.

## 광고·결제 호환 조건

광고는 클라이언트 플랫폼 SDK가 담당한다. 서버는 구매 검증·권한·복구·환불만 처리한다. 광고 SDK 연결·심사는 별도다.

**토스:** 서버 API는 mTLS가 필요하다. 발급 인증서/개인키를 등록하고 `env.TOSS_MTLS.fetch()`로 호출한다. 만료 전 교체한다. 공식 발표는 모든 Workers 고객 대상이며 최신 문서에 Paid 한정 조건은 없다. **Cloudflare proxied zone 대상에는 520 오류가 난다.** 실제 토스 endpoint/인증서 호환은 미검증이며 판매 전 통과해야 한다. API Shield/Access inbound mTLS와 구분한다. [Workers mTLS](https://developers.cloudflare.com/workers/runtime-apis/bindings/mtls/), [전체 고객 제공](https://blog.cloudflare.com/mtls-workers/), [토스 요건](https://developers-apps-in-toss.toss.im/documentation/integration/server-api)

일회용 코드 교환으로 anonKey 세션을 만든다. 클라이언트 키/CORS만으로 인증하지 않는다. orderId 조회의 SUCCESS·SKU·상태를 검증한다. **주문 응답에 구매자 키가 없어 사용자와 주문 소유권 연결 증거가 별도로 필요하다.** 최초 제출자를 소유자로 간주하지 않는다. [사용자 인증](https://developers-apps-in-toss.toss.im/api/user-key), [주문 조회](https://developers-apps-in-toss.toss.im/api/iap)

**ONE store:** HTTPS/OAuth 서버 API는 Workers fetch로 연동할 수 있는 구조다. client secret은 Workers Secrets에 두고 앱·환경·상품·purchaseToken·구매 상태를 검증한다. 서버 발급 구매 식별자와 developerPayload를 대조하는 연결을 설계한다. managed 비소모 상품을 consume하지 않고 권한 기록 후 acknowledge한다. 미확인 구매는 3일 후 자동 환불되므로 미처리를 재조회·경보한다. 시작/재개 시 queryPurchasesAsync로 복구한다. 토큰만으로 사용자 인증·권한 이전을 허용하지 않으며 실제 SDK/소유권 복구는 미검증이다. [Server API V7](https://onestore-dev.gitbook.io/dev/tools/billing/v21/serverapi), [SDK V21](https://onestore-dev.gitbook.io/dev/eng/tools/billing/v21/sdk)

## 원장·복구와 완료 조건

- 플랫폼·환경·앱·거래 ID의 UNIQUE 제약과 [D1 batch](https://developers.cloudflare.com/d1/worker-api/d1-database/#batch)로 원장·권한을 원자적으로 기록한다. 클라이언트 DB 직접 쓰기는 금지한다. 중복 요청은 기존 결과를 반환한다.
- 외부 acknowledge/지급 완료는 DB 처리 밖에서 수행하고 미완료 상태로 재시도한다. 토스는 저장 성공 후에만 지급 성공을 반환하고 미결 복구 후 completeProductGrant를 호출한다. [토스 지급·복구](https://developers-apps-in-toss.toss.im/guide/monetization/in-app-payment)
- 취소 내역/주문 상태를 정기·재접속 시 조회한다. 알림은 재조회 계기로만 사용한다. 확정 환불은 해당 거래 권한을 회수하고 다른 유효 구매를 재계산한다. 재설치는 플랫폼 구매 내역·검증된 소유자·원장을 대조한다.
- Cloudflare Free 계정, 인증서 등록 권한, 플랫폼 상품·정산 준비가 필요하다. 비밀·토큰은 Git/로그에 남기지 않고 최소 접근 권한을 준다.
- **현재는 문서 검토만 완료.** 무료 계정의 인증서 등록, 원격 mTLS, SDK 연결, 10ms CPU·한도 소진·중복·환불·재설치·계정 변경 검증 전에는 출시 완료로 표시하지 않는다. 이번에 배포·유료 전환·과금·실결제를 실행하지 않았다.
