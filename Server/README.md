# 광고 제거 IAP 구매 검증 서버

확인일: 2026-10-14. **Cloud Run(Node.js/TypeScript) + Firestore + Secret Manager**를 선택한다. 비소모성 광고 제거 권한만 서버에서 검증·보관한다. 규칙·AI·진행은 로컬이다(WebGL 최초 로드 제외). 제거 범위·SKU는 총괄 계약을 따른다.

## 선택 비교 — 설계 판단

- [Supabase](https://supabase.com/docs/guides/functions): Deno 기반 mTLS 운영 검증 추가로 제외.
- [Firebase Functions](https://firebase.google.com/docs/functions): 대안 가능. 이번에는 TLS·컨테이너 직접 관리를 선택.
- **Cloud Run 선택:** [Secret Manager 파일 주입](https://docs.cloud.google.com/run/docs/configuring/services/secrets)과 [Node HTTPS 인증서 옵션](https://nodejs.org/api/https.html#httpsrequestoptions-callback)으로 토스 outbound mTLS를 구성할 수 있다고 판단.

## 플랫폼 검증과 사용자 연결

**토스:** 파트너 서버→토스 API는 mTLS가 필요하다. 인증서·개인키는 Secret Manager에서 읽고 만료 전 교체한다. 미니앱 Origin만 CORS에 허용하되 CORS를 사용자 인증으로 간주하지 않는다. [서버 요건](https://developers-apps-in-toss.toss.im/documentation/integration/server-api)

SDK의 일회용 인증 코드를 서버에서 교환해 anonKey 기반 세션을 만든다. 클라이언트가 보낸 anonKey나 키 검증 성공만으로 본인을 인증하지 않는다. [인증 코드 교환](https://developers-apps-in-toss.toss.im/api/user-key)

orderId로 서버 조회하여 SUCCESS·SKU·결제 상태를 검증한다. **공개 주문 응답에는 구매자 키가 없다.** 인증된 사용자와 주문의 소유 관계를 입증할 공식 연동 방법·실기 증거를 확보하기 전 판매를 열지 않는다. 최초 제출자를 소유자로 간주하지 않는다. [주문 API](https://developers-apps-in-toss.toss.im/api/iap)

**ONE store:** managed 상품을 비소모로 유지하고 consume하지 않는다. 서버 OAuth로 purchaseToken·앱·상품·구매 상태를 확인한다. 서버 발급 구매 식별자를 developerPayload와 대조하는 연결을 설계한다. 토큰만으로 사용자 인증·권한 이전을 허용하지 않으며 재설치 후 소유권 확인 방식은 SDK 연동 시 검증한다. [Server API V7](https://onestore-dev.gitbook.io/dev/tools/billing/v21/serverapi)

권한 저장 후 acknowledge를 처리한다. 3일 내 확인/소비하지 않으면 자동 환불되므로 미처리 건을 재조회·재시도·경보한다. 시작/재개 시 queryPurchasesAsync로 비소모 구매를 다시 확인한다. [SDK V21](https://onestore-dev.gitbook.io/dev/eng/tools/billing/v21/sdk)

## 지급·환불·복구 계획

1. 플랫폼·환경·앱·거래 ID를 원장 키로 삼아 권한과 처리 상태를 [Firestore 트랜잭션](https://firebase.google.com/docs/firestore/manage-data/transactions)으로 저장한다. 클라이언트 DB 쓰기는 차단한다. 중복 콜백은 같은 결과를 반환한다.
2. 외부 acknowledge/지급 완료 호출은 DB 트랜잭션 밖에서 수행하고 미완료 상태를 보존해 재시도한다. 토스는 저장 성공 후에만 지급 성공을 반환하고, 미결 주문 복구 후 completeProductGrant를 호출한다. [토스 지급·복구](https://developers-apps-in-toss.toss.im/guide/monetization/in-app-payment)
3. 취소 내역/주문 상태를 정기·재접속 시 재검증한다. 알림은 조회 계기로만 사용한다. 확정 환불은 해당 거래 권한을 회수하고 다른 유효 구매가 있는지 재계산한다.
4. 재설치 시 플랫폼 구매 내역·인증된 소유자·원장을 대조해 복구한다. 로컬 플래그로 지급하지 않는다. 플랫폼 간 이전·진행도 복구는 제외한다.

## 장애·비용·착수 조건

- 장애 시 새 지급/구매는 보류하고 재구매를 유도하지 않는다. 게임 진행은 유지한다. 기존 검증 권한은 서명된 로컬 캐시로 유지하고 재접속 시 갱신한다. 오프라인 중 환불 반영 지연은 수용한다.
- 요청 기반 과금, 최소 인스턴스 0·최대 2를 초기안으로 삼고 요청률·시간·재시도·DB 조회·로그를 제한한다. 토큰/개인키는 로그·Git에 남기지 않는다. 최소 IAM 권한과 인증서 교체 경보를 둔다.
- 월 예산·경보 담당·구매 일시중단 절차를 배포 전에 확정한다. **알림 전용 예산은 비용 상한이 아니다.** Spend cap 지원 범위와 지연도 별도 확인한다. [비용 가드](https://docs.cloud.google.com/billing/docs/how-to/budgets)
- **GCP 계정/프로젝트·결제 계정·IAM 준비가 필요하다.** 인증서·상품·실제 SDK 도입·소유권 연결·중복/환불/재설치/장애 검증은 미완료다.
- 기술 계획만 작성했다. 코드·배포·과금·실결제는 미실행, Excel/콘텐츠는 변경하지 않았다.
