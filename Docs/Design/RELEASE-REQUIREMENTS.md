# 출시·등급·광고·결제·권리 요구사항

확인일: **2026-10-14 (Asia/Seoul)**. 대상: 원스토어 대한민국 Android 출시 → 같은 게임의 앱인토스 Unity WebGL 출시. 게임 범위는 [총괄 계약](../Management/CONTRACT.md)의 7×6 중력식 4목, 로컬 AI, 30스테이지다.

**목표 변경:** 사용자 추가 요청으로 광고 제거 1회 구매와 구매 검증·복구·환불용 최소 서버를 계획한다. 현 구현의 IAP 미도입과 구분하며 플랫폼별 조건은 12절을 따른다. 광고 없는 원스토어 버전에는 광고 제거 상품을 판매하지 않는다.

이 문서는 공개된 공식 원문을 실제로 열어 조사한 체크리스트다. 계정 내부 승인 상태, 법률상 개별 사업자 자격, 실제 빌드·광고 동작을 확인한 보고서는 아니다. 아래의 완료 증거가 없으면 출시 준비를 완료로 표시하지 않는다. 정책은 제출 당일 다시 확인한다.

**최종 인계 판정: 코어 프로토타입과 개발 빌드 단계이며 출시 완료가 아니다.** 이 역할의 정책 조사·기존 사용 자산 증거 정리·고지 원문 준비는 완료했다. 최종 shader/WebGL template 수정 이후의 런타임 재시험과 재빌드 고지 검증은 Client의 최종 해시를 받은 총괄/QA가 닫는다. 계정 정보 없이 끝낼 기술 작업과 외부 의존 차단 항목은 11절에 구분했다.

## 1. 현재 상태와 일정 판단

| 항목 | 10/14 현재 근거/상태 | 다음 완료 증거 |
| --- | --- | --- |
| 원스토어 판매자 계정 / 앱인토스 워크스페이스 | 사용자 준비 완료 응답을 총괄이 전달함. 이 세션에서 로그인 검증하지 않음 | 앱별 등록 화면과 담당자 접근권한 |
| 게임 범위 | 총괄 CONTRACT.md로 확정 | 실제 빌드·테스트 결과는 Client/QA 담당 |
| 앱별 ID / 서명 / 광고 ID | 미확인 | Android package name, Toss appName, 키 보관 책임자, 지면별 ID와 승인 상태 |
| 등급·원스토어 공개 URL | 미확보 | 한국 연령등급·등급번호/일자·IARC ID 및 실제 공개 상품 URL |
| 앱인토스 동일성·등급정보 등록 | 미수행 | 콘솔 등록 결과와 원스토어/앱인토스 게임 화면 각 2장 |
| 라이선스 | 현재 자산·프리팹·패키지 및 신규 SoftUI 도형 출처 기록. 최종 개발 APK 고지 8개 원본 일치, 고지 묶음 전문 유지와 총괄의 본문30 WebGL 스크롤/닫기 재검사 확인. [대장](../../Licence/ASSET-REGISTER.csv)은 빌드 해시로 고정한 스냅샷 | 재빌드/SDK 추가 후 재검사, 실제 기기 열람, 사용자 Unity 이용 자격·정확한 크레딧 |
| 광고 수익화 | 플랫폼 정책 조사만 완료 | SDK 실연결, 승인, 유효 노출, 완료 보상, 정산 자격 각각 확인 |
| 광고 제거 IAP | 목표에 추가; 상품 구성·공식 지원 조건 조사. 상품/가격 등록·SDK·서버·결제 시험 미수행 | 12절의 권한 검증·지급·복구·환불 및 실환경 증거 |

**1주 제작과 두 플랫폼 승인 완료를 구분한다.** 원스토어는 검증에 영업일 최대 5일, 예외 시 추가 소요를 안내한다. 앱인토스 현재 FAQ는 출시 검수/재검수 3~5영업일, 주말·공휴일 제외를 안내한다. 순차 경로에서는 원스토어 공개·등급 확인이 앱인토스 제출의 선행 조건이다. 10/21까지 내부 제작과 제출 준비를 마무리하는 일정은 세울 수 있지만 승인·정산 개시는 외부 의존성이다. [원스토어 검증 요청](https://onestore-dev.gitbook.io/dev/docs/apps/request-for-review), [앱인토스 FAQ](https://developers-apps-in-toss.toss.im/guide/faq.md)

## 2. 원스토어 등록과 등급분류

**사업 자격은 별도로 확인한다.** 게임산업법 제25조는 게임제작업/게임배급업 등록과 예외를 규정한다. 판매자 계정·Toss 워크스페이스·일반 사업자등록·게임업 등록은 서로 다른 확인 항목이다. 이 광고 수익형 게임에 적용되는 등록, 예외, 통신판매 등 추가 신고 필요 여부는 실제 사업 형태를 바탕으로 관할 담당자에게 확인하고 증빙을 준비한다. 무료 설치·IAP 없음만으로 등록 예외라고 단정하지 않는다. [게임산업법 제25조](https://law.go.kr/lsLinkCommonInfo.do?chrClsCd=010202&lsJoLnkSeq=1025570287), [법제처 게임 제작·제공 안내](https://www.easylaw.go.kr/CSP/CnpClsMain.laf?ccfNo=1&cciNo=2&cnpClsNo=1&csmSeq=2858&popMenu=ov)

### 필요한 순서

1. 준비된 판매자 계정에서 앱별 정보와 대한민국 배포 설정을 만든다. 독자 게임명, 등록자명, 문의 이메일, 설명, 아이콘·스크린샷, 개인정보 고지, 권리 보유 자료를 준비한다. 실제 항목·이미지 규격은 제출 콘솔에서 확인한다.
2. IARC 연령등급 인증을 진행한다. 기존 인증이 있으면 인증 ID를 등록할 수 있고, 신규는 콘텐츠 평가 설문을 작성한다. **전체이용가는 목표이지 사전 확정 등급이 아니다.** 광고·채팅·구매·사행성 등 질문은 실제 제출 빌드와 일치하게 답한다. [원스토어 연령등급](https://onestore-dev.gitbook.io/dev/docs/apps/product/common-info/age-rating)
3. 서명한 Android 바이너리와 지원 단말을 등록한다. 공식 가이드는 APK/AAB를 지원하며 APK→AAB 전환은 가능하지만 AAB→APK 전환은 불가하다고 안내한다. 서명 옵션을 사용자와 정하고 키를 보관한다. 기존 앱 업데이트에는 서명 연속성이 필요하다. [바이너리 가이드](https://onestore-dev.gitbook.io/dev/docs/apps/product/android/binary)
4. 출시 버전의 실제 한 판·저장·뒤로가기·설정·광고 실패를 검사하고 검증 요청한다. 원스토어는 바이너리 변경 등에도 검증을 요구하며, 시스템 검사와 정책 검사를 진행한다. [검증 절차](https://onestore-dev.gitbook.io/dev/docs/review)
5. 승인 후 실제로 판매/공개 상태가 되었는지 확인한다. '즉시 적용', '직접 적용', '예약 적용'은 서로 다르다. 심사 승인만 있고 미공개이면 앱인토스에 필요한 실제 출시 URL을 확보한 것이 아니다. [배포 옵션](https://onestore-dev.gitbook.io/dev/docs/apps/request-for-review)
6. 공개 상품 URL, 출시 빌드 식별값/해시, 국내 연령등급과 분류일·번호, IARC 인증 ID, 등록자명, 내용정보 표시를 보관한다. GRAC 자체등급분류 조회와 콘솔의 값이 다르면 다음 플랫폼에 임의 입력하지 않는다.

원스토어는 2024-09-24부터 IARC 도입을 공지했다. 2019년의 재사용 불가 공지를 현재 절차에 그대로 적용하지 않는다. 현재 IARC/앱인토스의 신규 유통 정보 절차를 함께 따른다. [IARC 도입 공식 공지](https://dev.onestore.net/devpoc/support/news/noticeView.omp?noticeId=33441)

**분류 거부(RC)나 청소년이용불가가 나오면 일정 재산정이 필요하다.** 원스토어 FAQ는 RC에 대해 GRAC 신청·발급 후 번호 등록을 안내한다. 답변을 조작해 낮은 등급을 얻지 않는다. 실행 불가, 타 앱마켓 유도, 개인정보 고지/동의 누락도 반려 사유로 안내된다. [원스토어 검증 FAQ](https://onestore-dev.gitbook.io/dev/help/faq/review)

### 원스토어 광고의 별도 장애물

**원스토어 출시만으로 AdMob 정상 광고 송출 심사가 가능하다고 가정하면 안 된다.** 확인한 Google 공식 지원 스토어 목록은 Google Play/Apple 및 Amazon·OPPO·Samsung Galaxy·VIVO·Xiaomi이며 ONE store가 없다. 미지원 스토어에만 등록된 앱은 준비 심사를 받을 수 없고 광고 송출이 제한된다고 명시한다. 이는 '광고가 한 건도 절대 안 뜬다'는 뜻이 아니라 정상 수익화 완료를 증명할 수 없다는 뜻이다. [AdMob 앱 설정 공식 문서](https://support.google.com/admob/answer/9989980?hl=en)

원스토어용 광고 네트워크는 아직 확정되지 않았다. 원스토어 개발문서 인덱스에 인앱 광고/ONE AdMax 관련 진입점은 확인했으나 해당 상세 IAA 페이지는 열기에 실패했다. ONE AdMax의 Unity 버전·배너/전면/리워드 지원과 가입·심사·정산 조건을 확인한 것으로 기록하지 않는다. 출시 담당이 실제 계정에서 확인한 지원 네트워크를 선택하고, SDK 권리와 실기기 동작을 검증해야 한다. 추가 스토어 출시를 자동으로 범위에 넣지 않는다.

총괄은 **원스토어를 우선 등급 증빙 경로, 앱인토스를 핵심 광고 수익화 경로**로 정했다. 원스토어 광고 네트워크가 준비되지 않으면 첫 공개 버전의 무광고 범위를 실제 제출 메타데이터와 일치시키고, 후속 광고 추가 시 필요한 변경 심사를 확인한다. 모의 광고를 수익화 완료나 라이브 광고로 표현하지 않는다. 앱인토스 광고는 아래의 별도 플랫폼 경로를 사용한다.

## 3. 앱인토스 등급 증빙과 동일한 게임 여부

공식 등급 안내는 GRAC 증명서 경로와 스토어 등급 경로를 설명한다. 스토어 경로에서는 **실제 출시된 게임 URL**로 등급을 확인하며 원스토어도 예시에 포함된다. IARC ID만 발급받고 출시하지 않은 상태는 이 경로의 완료가 아니다. 연령등급은 게임 접속 브릿지 뷰에 표시한다. [게임 등급분류 공식 안내](https://toss.im/apps-in-toss/blog/game_rating_classification)

자체등급분류 게임의 앱인토스 신규 유통에는 콘솔의 추가 게임물 정보가 필요하다. 다음 자료를 원스토어 정보와 일치하게 준비한다. [자체등급분류 게임물 정보 공식 안내](https://toss.im/apps-in-toss/blog/self-rated_game_distribution)

| 준비 자료 | 보관/확인 기준 |
| --- | --- |
| 사업자 또는 개인 기본정보, 등록자명 | 원래 등급 받은 상품의 등록자와 일치. 차이가 있으면 사유·권리승계 증빙 |
| 자체등급분류사업자, 분류일자·분류번호, 이용등급·내용정보 | GRAC 조회 결과와 원스토어 자료 대조. IARC 인증 ID와 국내 등급번호를 혼동하지 않음 |
| 원스토어와 앱인토스 게임 화면 각각 2장 | 실제 같은 콘텐츠/게임임을 확인할 수 있는 장면. 총 4장; 임의 목업 금지 |
| 대표자 인감/사인 이미지 | 사용자가 직접 제공. 공개 Git/문서에 포함하지 않음 |
| 선정성·폭력성 등 내용정보가 있는 경우의 해당 장면 | 실제 등급 결정 요인에 맞는 증빙 |

토스 안내에 따르면 입력 정보는 토스가 GRAC에 전달한다. 단순히 '다른 스토어에서 심사받음'이라는 문구로 대체하지 않는다. 새 게임이 GRAC에서 바로 검색되지 않는 경우 공개/연계 지연의 소요일은 확인되지 않았다. 등록 당시 게임물명·기간을 점검하고 플랫폼/GRAC에 확인할 준비를 한다.

**플랫폼별 광고 SDK 차이와 콘텐츠 동일성은 별개로 관리한다.** 게임명·핵심 규칙·스테이지·콘텐츠 등급 요소는 일치시키고, Android/WebGL의 기능 차이를 제출 메모에 설명한다. 이식 시 달라진 내용이 재등급/내용수정신고 대상인지 출시 담당이 확인한다.

### 웹보드 해당 여부는 미확정

앱인토스 서비스 오픈 정책은 웹보드 조건에서 체스·장기 등을 예시로 들며 게임머니 베팅 구조, GRAC 등급, 성인 인증 등을 별도로 설명한다. 같은 사이트의 일반 게임 등급 안내는 스토어 등급 경로를 허용한다. **베팅·게임머니 없는 로컬 AI 전술 4목이 웹보드 분류에 해당하는지** 이 공개 문구만으로 단정하지 않는다. 플랫폼에 게임 규칙과 수익 구조를 제시해 확인할 항목이다. 해당한다면 일반 스토어 증빙 경로만으로 출시 가능하다고 약속하지 않는다. [서비스 오픈 정책의 조건부 서비스](https://developers-apps-in-toss.toss.im/intro/guide.md)

## 4. Unity WebGL 지원과 검수 기준

공식 Unity SDK가 존재한다. Unity 2021.3 이상을 지원하고 Unity 6 이상을 권장하며 C# API와 JavaScript 브리지를 제공한다. 프로젝트 계약의 Unity 6000.3.15f1은 이 표기 범위 안에 있지만 **실제 호환·빌드 성공은 별도 검증**이다. 일반 배포는 stable release 태그를 사용하며 beta/beta-perf는 파일럿용이다. 정확한 설치 버전/태그/의존성은 실제 적용 시 고정한다. [Unity 연동 문서](https://developers-apps-in-toss.toss.im/unity/intro/overview.html), [공식 SDK 저장소](https://github.com/toss/apps-in-toss-unity-sdk)

### 확인한 stable SDK와 미확정 권리 조건

**2026-10-14 추가 조사에서 공식 GitHub API의 최신 stable은 `release/v3.8.0`으로 확인했다.** 공개 시각은 2026-10-14 05:21:42 UTC, 한국 시각 14:21:42이며 draft/prerelease 모두 false다. 웹 검색에 보인 v3.5.0 목록 및 시작 문서의 v3.0.3 예시와 구분했다. 확인한 버전은 도입 후보이며 StageGame에 설치하거나 테스트한 버전은 아니다. [공식 latest API](https://api.github.com/repos/toss/apps-in-toss-unity-sdk/releases/latest), [확인 증거](../../Licence/TOSS-SDK-EVIDENCE.json)

| 항목 | 해당 태그에서 직접 확인한 값 |
| --- | --- |
| UPM 패키지 | `im.toss.apps-in-toss-unity-sdk` / `3.8.0` |
| 설치 ref | `https://github.com/toss/apps-in-toss-unity-sdk.git#release/v3.8.0` |
| 실제 태그 대상 커밋 | `18814fe5b5a2af5cd2a11a7a5bae53403fafa15e` — annotated tag 객체 SHA와 구분 |
| Unity 선언 | `unity: 2021.3`, `unityRelease: 45f1`, engine `>=2021.3` |
| UPM 직접 의존성 | `com.unity.nuget.newtonsoft-json: 3.0.2` |
| 웹 런타임 선언 | `@apps-in-toss/web-framework: 3.8.0`, `bridge-core: 2.10.9`, `web-analytics: 2.10.9` |
| 웹 개발 의존 선언 | `@apps-in-toss/devtools: 3.8.0`, `typescript: 7.0.2`, `vite: 8.3.4` |
| SDK 관리 도구 | `NODE_VERSION: 24.13.0`, `PNPM_VERSION: 11.6.0`; packageManager도 `pnpm@11.6.0` |
| 내부 sdk-policy | minVersion `2.4.0`, deniedVersions에 `3.0.0`. 기술 버전 정책이며 사용권/출시 승인 문서가 아님 |

근거는 고정 커밋의 [UPM metadata](https://raw.githubusercontent.com/toss/apps-in-toss-unity-sdk/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/package.json), [웹 package metadata](https://raw.githubusercontent.com/toss/apps-in-toss-unity-sdk/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/WebGLTemplates/AITTemplate/BuildConfig~/package.json), [Node 버전 상수](https://raw.githubusercontent.com/toss/apps-in-toss-unity-sdk/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/Editor/AITNodeJSDownloader.cs), [pnpm 버전 상수](https://raw.githubusercontent.com/toss/apps-in-toss-unity-sdk/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/Editor/AITPackageManagerHelper.cs), [sdk-policy](https://raw.githubusercontent.com/toss/apps-in-toss-unity-sdk/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e/sdk-policy.json)다. npm의 최종 resolved 의존성과 개별 라이선스는 실제 설치 후 lock/패키지 원문으로 추가 검토한다.

**라이선스 조건은 여전히 미확정이다.** 해당 커밋의 Git tree 515개 항목을 재귀 조회했고 응답은 truncated=false였다. LICENSE/LICENCE/NOTICE/COPYING/COPYRIGHT/TERMS/legal 이름 검색에는 일치 파일이 없었고, 위 UPM·웹 package.json에도 `license` 필드가 없다. 이 결과는 모든 소스 헤더/운영 계약에 허가가 없다는 판정은 아니지만, MIT 또는 자유 재배포 조건을 확정할 근거도 아니다. [실제 tree](https://api.github.com/repos/toss/apps-in-toss-unity-sdk/git/trees/18814fe5b5a2af5cd2a11a7a5bae53403fafa15e?recursive=1)

공식 설치 안내에 따른 기술 도입 준비와 별개로, 출시 담당은 이 버전의 **상업적 미니앱 임베딩, 필요한 수정, 배포/소스 공개 범위, 고지 의무**를 규정하는 적용 계약·콘솔 약관 또는 권리자의 공식 안내를 확보한다. 별도 유료 계약이 반드시 필요하다거나 사용이 금지됐다고 추정하지 않는다. 이 세션은 SDK를 설치하거나 약관을 대신 수락하거나 문의를 발송하지 않았다.

### 도입·권한·패키징 선행정보

| 단계 | 계정 없이 준비할 범위 | 실제 실행/출시에 필요한 사항 |
| --- | --- | --- |
| 설치 및 도구 | 고정 태그·커밋·의존 버전 확인은 완료. Client에서 UPM resolve/컴파일, WebGL 모듈, Git/npm 접근 및 SDK 도구 설치 검증 가능 | SDK는 자체 Node/pnpm을 내려받아 `~/.ait-unity-sdk/`를 사용. 다운로드/실행 환경과 npm 의존성 고지를 확인하며 시스템 Node 존재만으로 준비 완료 처리하지 않음 |
| 구성 | 앱 설정 스키마와 빈 값/오류 처리를 준비하고 실제 정보와 개발용 값을 구분 | `AIT Configuration`의 플랫폼 앱 ID/appName, 표시 이름·버전·색·아이콘 URL, 필요한 권한 목록. 카메라/연락처/위치/로그인 기능을 이 로컬 게임에 자동 추가하지 않음 |
| 로컬 패키징 | WebGL 중간 산출물과 `.ait` 결과를 구분하고 테스트 경로 준비 | 공식 시작 안내는 앱 ID를 필수 UI 설정으로 설명하지만, 빌드 내부 문서는 일부 직접 호출이 빈 설정으로도 패키지를 만들 수 있다고 설명. 파일 생성 자체가 앱 등록·유효 설정·제출 가능성의 증거는 아님 |
| 템플릿 이식 | 현재 `StageGamePortrait`의 반응형 의도를 유지할 변경점을 정리 | SDK는 AITTemplate을 사용하므로 현재 HTML이 그대로 적용된다고 가정하지 않음. 커스텀 코드는 공식 USER 영역에 이식하고 최종 `ait-build/dist/`를 재검증 |
| 테스트/업로드 | Editor·브라우저 Mock에서 실패/콜백 경로 준비 | 실제 앱 권한/배포 키 또는 권한 있는 콘솔 업로드, 토스 실기기 QR 테스트 필요. `ait deploy`는 테스트 환경 업로드이며 콘솔 검수·공개를 대체하지 않음 |
| 개인정보/광고 | SDK 추가 전후 데이터 흐름 차이와 no-fill 처리 준비 | 자동 런타임 로그와 web-analytics 의존성을 검토하고 처리방침에 반영. 플랫폼 앱/광고 지면 승인·보상 이벤트·정산은 별도 검증 |

위 절차의 근거: [시작 및 앱 구성](https://developers-apps-in-toss.toss.im/documentation/unity/first-steps/getting-started), [도구·패키징 구조](https://developers-apps-in-toss.toss.im/documentation/unity/build/build-process), [템플릿 사용자 영역](https://developers-apps-in-toss.toss.im/documentation/unity/build/build-customization), [테스트와 배포 프로필](https://developers-apps-in-toss.toss.im/documentation/unity/build/build-profiles). SDK 설치와 개인정보 처리목록 확정은 함께 진행한다. [SDK 이벤트 로깅](https://developers-apps-in-toss.toss.im/documentation/unity/add-features/metrics)은 WebGL에서 장면·오류·수명주기·프레임 지연 등의 이벤트를 자동 전송한다고 설명하므로, SDK를 넣은 뒤에도 무조건 '분석/로그 수집 없음'으로 표시하지 않는다.

앱인토스는 단순 WebGL 웹주소 제출과 구분되는 `.ait` 번들 업로드·테스트·검수 경로를 제공한다. 업로드 시 생성되는 테스트 스킴/QR로 실제 토스 앱에서 검사하며, 공개 `intoss://` 진입은 정식 출시 이후다. FAQ상 번들 크기는 압축 해제 기준 100MB 이하다. [테스트 가이드](https://developers-apps-in-toss.toss.im/guide/operation/toss), [FAQ](https://developers-apps-in-toss.toss.im/guide/faq.md)

다음은 공식 게임 출시 체크리스트에서 이 MVP에 직접 적용할 부분을 추린 것이다. 새 목표의 결제에는 12절을 추가 적용하며 웹보드 해당 여부는 별도 확인한다. [게임 출시 가이드](https://developers-apps-in-toss.toss.im/checklist/app-game.html)

| 검수 항목 | 제출 전 필요한 증거 |
| --- | --- |
| 첫 화면 10초 이내, 인터랙션 2초 이상 지연 없음 | Android·iOS 토스 실기기에서 기기/OS/앱버전/네트워크와 측정 결과 |
| 닫기·Android 뒤로가기·종료 확인, Safe Area, 풀스크린/세로모드 | 작은 화면·노치 영역 포함 화면 녹화 |
| 진입 즉시 로그인/동의 바텀시트나 강제 행동 유도 없음 | 첫 실행/재진입 사용 흐름 |
| 음소거 제어, 백그라운드 즉시 오디오 중지와 복귀 | 게임·광고·백그라운드 전환 검사 |
| 사용자 식별자 확인 및 진행도 유지 | 현재 사용자 식별키 API 적용 범위 확인과 종료/재진입 저장 검사 |
| HTTPS, 외부 코드 실행·자사 사이트 임베드 제한 | 네트워크/번들 검토; 원스토어 APK 설치 유도 링크 없음 |
| 실제 광고·사전 로드·복귀·보상·상하단 배너 | 개발 중 테스트 ID 검사와 제출용 실지면 검사를 별도로 기록 |

로컬 저장만으로 재설치·기기 변경·웹뷰 저장소 정리에 대한 복구를 보장하지 않는다. 새 목표의 유료 구매 권한은 진행도와 분리해 복구해야 하며, 플랫폼 사용자와 주문의 검증 가능한 연결을 구현자가 확인한다. 식별키가 있다는 사실을 곧바로 익명/개인정보 아님으로 해석하지 않는다.

## 5. 앱인토스 광고·사업자·정산

### 허용되는 광고와 구현 방식

앱인토스 정책은 플랫폼이 제공하는 전면형·보상형·배너 광고만 허용하고 외부 광고 네트워크의 직접 연동을 금지한다. 따라서 원스토어 Android의 네이티브 광고 SDK를 WebGL에 그대로 옮기지 않는다. [서비스 오픈 정책](https://developers-apps-in-toss.toss.im/intro/guide.md)

- 전면/리워드는 공식 로드/표시 이벤트 흐름을 유지한다. 리워드는 `userEarnedReward` 등 실제 SDK의 보상 획득 이벤트에서만 지급하며 `dismissed`는 보상 증거가 아니다. 중복 지급 방지·콜백 정리·로드/표시 실패 경로를 검사한다. [전면/보상형 개발 가이드](https://developers-apps-in-toss.toss.im/documentation/common/monetization/iaa/interstitial-rewarded-ad.md)
- WebView 배너는 공식 `TossAds`가 DOM 요소에 부착하는 방식이다. 문서는 토스 5.241.0 이상 지원, `isSupported()` 검사와 초기화·제거·no-fill 처리를 안내한다. Unity 캔버스 바깥 WebView 호스트 공간과 연결이 필요하다는 구현 판단이며, C# 한 줄로 배너가 된다고 확인한 것은 아니다. [WebView 배너 가이드](https://developers-apps-in-toss.toss.im/documentation/common/monetization/iaa/web-banner.md)
- 광고를 게임 버튼처럼 꾸미거나 조작부에 붙여 오클릭을 유도하지 않는다. SDK 이벤트·노출/클릭을 변조하거나 주기적 강제 refresh로 성과를 만들지 않는다. 클릭 자체에 보상을 주지 않는다. 개발자는 자신의 실광고를 눌러 테스트하지 않으며 개발 검증은 테스트 ID로 한다. [광고 운영 정책](https://developers-apps-in-toss.toss.im/guide/monetization/in-app-ad.md)

**첫 3판 제외 / 최소 2판 AND 90초 / 로비·결과 배너 / 힌트 1회**는 StageGame 시험값이다. 정책상 의무 간격이나 수익이 입증된 최적값이 아니다. no-fill이면 결과/재도전이 정상 진행돼야 하며 힌트는 제공되지 않음을 안내한다. 보상을 광고 없이 획득했다고 조작하거나 광고를 반복 호출해 진행을 잠그지 않는다.

위 광고 흐름은 광고 제거 미구매자 기준이다. 유료 권한이 유효한 이용자는 광고 없이 판당 힌트 1회를 제공하고 실제 광고 보상 이벤트와 구분한다(12절).

### 사업자 등록 유예와 문서 간 차이

현재 사업자 등록 상세 페이지는 **인앱 광고 ID 발급·수익화의 등록 유예**를 안내한다. 워크스페이스 누적 예상수익 5천원 미만이면 사업자/정산 등록을 유예할 수 있고, 5천원 도달 후 5영업일 안에 등록과 승인을 끝내지 않으면 미니앱이 일시 미노출될 수 있다. 같은 문서 하단의 일반 목록과 FAQ/광고 소개에는 사업자 필요 문구가 남아 있다. 따라서 '사업자 없어도 영구 운영 가능' 또는 '광고 ID 발급 자체가 언제나 불가' 어느 쪽도 단정하지 않는다. 사용자의 워크스페이스에 적용되는 유예 여부와 실제 정산 자격을 콘솔에서 확인한다. [사업자 등록 상세 가이드](https://developers-apps-in-toss.toss.im/prepare/register-business.html)

사업자로 운영할 경우 업종 일치, 사업자 서류, 정산 계좌 사본, 세금 관련 이메일/증빙 등 본인 유형에 맞는 자료를 사용자가 준비한다. 광고 소개 문서는 정산정보 검토 평균 2~3영업일, 광고 그룹의 구글 등록 최대 2시간을 안내하지만 송출·승인·입금을 보장하는 SLA로 취급하지 않는다. [광고 설정](https://developers-apps-in-toss.toss.im/ads/intro.html)

정산 상세 가이드는 광고 운영비와 앱인토스 수수료를 구분한다. 확인 당시 운영비 약 30% 수준, 앱인토스 수수료 15% 및 그 수수료 면제 프로모션을 설명한다. **면제는 모든 비용이 0이라는 의미가 아니다.** 적용 기간·계약·과세 유형은 계정에서 다시 확인하고, 대시보드 eCPM/수익이 어느 공제 단계 기준인지 파악해 중복 차감하지 않는다. 문서 예시 금액을 StageGame 예상 매출로 사용하지 않는다. [정산 가이드](https://developers-apps-in-toss.toss.im/guide/settlement)

## 6. 개인정보와 사용자 고지

게임·AI·진행은 로컬에 두고 별도 회원가입·연락처·위치 권한은 요구하지 않는다. 새 목표의 최소 구매 서버에는 플랫폼 식별자와 주문·권한·환불 기록이 추가된다. 광고 SDK, Toss 식별키, 오류/접속 로그를 포함한 실제 버전·설정·통신과 보유기간을 확인해 고지를 갱신해야 하며, '개인정보를 전혀 수집하지 않는다'고 표시하지 않는다.

| 데이터 흐름 | MVP 의도 | 출시 전 확인/조치 |
| --- | --- | --- |
| 스테이지 해금, 설정, 광고 억제 상태 | 기기/플랫폼 로컬 저장 | 삭제 방법, 보존 범위, 재설치/기기 변경 시 제한 안내 |
| Toss 사용자 식별키 | 플랫폼 검수 요구에 맞춰 최소 적용 검토 | 취득 목적, 저장 위치, 다른 정보 결합 가능성, 삭제 방법 |
| 광고 식별자·기기정보·IP·광고 이벤트 | 채택 SDK가 처리할 수 있음; 현시점 수집 확정 아님 | 제공자별 실제 수집 목록·목적·보유·위탁/제3자·국외이전 해당 여부와 적법 근거 |
| 분석·크래시 로그 | MVP에 무조건 추가하지 않음 | 도입할 때 개인정보 마스킹·접근권한·보존기간 확인 |
| 고객 문의 | 문의 이메일로 필요한 사안만 | 운영자 연락처, 문의 삭제/보존기준, 권리행사 절차 |

개인정보 보호법 제30조는 처리목적·보유기간·제공/위탁(해당 시)·파기·정보주체 권리·담당 연락처 등을 담은 처리방침을 쉽게 공개하도록 요구한다. 허위 운영자명/기간으로 템플릿을 채우지 않는다. [현재 시행 제30조 원문](https://law.go.kr/lsLinkCommonInfo.do?chrClsCd=010202&lsJoLnkSeq=1029332499)

원스토어는 수집 상품의 최소 권한·구체적 고지·명시적 동의와 처리방침을 요구하며, 아동 정보 수집 시 법정대리인 동의 등도 명시한다. 전체이용가 콘텐츠라는 사실과 아동 데이터 처리/광고 대상 정책은 별개다. 원스토어 이용 대상과 SDK의 연령 설정을 정하고, 필요 동의 전 SDK 동작을 확인한다. [원스토어 개인정보·보안 가이드](https://onestore-dev.gitbook.io/dev/docs/review/one-store-review-guideline/undefined-2)

국외이전·맞춤형 광고·아동 해당 여부는 실제 선택 SDK와 이용대상에 따라 확인할 사항이다. 미구현 개인정보 처리방침 URL, 작동하지 않는 삭제 요청 채널, 검증하지 않은 '데이터 미수집' 표기는 제출하지 않는다. 개인정보·서명 이미지·계정 서류·키는 공개 저장소에 넣지 않는다.

## 7. 코드·이미지·폰트·음원 권리

원스토어는 직접 제작하거나 적법한 사용권을 확보한 콘텐츠만 허용하고 타인의 상표·특허 등 침해를 금지한다. 전통적 규칙이나 널리 알려진 장르라는 이유만으로 타 앱의 코드·AI·국면집·설명문·아이콘·보드 외관을 복사할 수 있다는 결론을 내리지 않는다. [원스토어 지식재산권 가이드](https://onestore-dev.gitbook.io/dev/docs/review/one-store-review-guideline/undefined-4)

| 대상 | 적용 기준 | 아직 필요한 증거 |
| --- | --- | --- |
| 자체 코드·국면·문구·도형 | 독립 제작 이력 유지. 외주/기여자 사용 범위 확인 | 작성자·변경 이력과 외부 차용 없음 또는 허락 증거 |
| 게임명·로고·마케팅 | 독자 브랜드. CONNECT 4·OTHELLO 등 타사 브랜드를 출시명/로고로 채택하지 않음 | 최종 이름의 KIPRIS 등 상표 확인 및 필요 시 권리 검토 |
| Unity Editor/패키지 | 설치 자체가 모든 자산의 재배포 허가를 뜻하지 않음 | 사용자 라이선스 자격, 실제 패키지 버전별 LICENSE/NOTICE |
| 앱인토스 Unity SDK | stable `release/v3.8.0`의 실제 커밋·의존성 확인. **해당 tree의 LICENSE/NOTICE 및 package license 필드는 발견되지 않아 배포 조건 미확정** | 4절의 고정 버전 증거와 적용 계약/권리자 안내, 도입 후 실제 의존성 LICENSE/NOTICE. 공개 GitHub라는 이유로 MIT로 기록 금지 |
| 한국어 폰트 | 현재 Noto Sans CJK KR Regular 2.004 및 OFL 1.1 확인 | 아래 실제 파일 증거 및 배포본 고지 점검. RFN/수정 조건 유지 |
| 효과음·음악·아이콘 | 직접 제작 또는 파일별 상업 사용권 확인 | 제작 이력/영수증/라이선스, 크레딧·수정/재배포 조건 |
| Unity Asset Store 자산 | 무료도 사용조건 확인. 표준 EULA와 개별 제공자 약관을 구분 | 구매 계정·라이선스 좌석/범위, 제한 자산 여부. 원본 자산 단독 재배포 금지 검토 |
| AI 생성 자산 | 현재 반입 없음. 사용 도구 약관과 입력물 권리 확인 | 생성 이력, 상업 이용 조건, 유사성/상표·초상 확인, 플랫폼 고지 해당 여부 |

OFL은 글꼴 재배포·번들링을 허용하는 조건과 저작권/라이선스 유지, 예약 글꼴 이름 등의 제한을 둔다. [OFL 공식 원문](https://openfontlicense.org/open-font-license-official-text/)

실제 확인한 폰트 증거:

- 현재 폰트는 `Client/Assets/StageGame/Fonts/NotoSansCJKkr-Regular.otf`, Version 2.004다. [공식 OTF](https://raw.githubusercontent.com/notofonts/noto-cjk/main/Sans/OTF/Korean/NotoSansCJKkr-Regular.otf) 16,433,112바이트와 로컬 파일의 SHA-256이 `6BCB2A0703AA137E874FC2DFFA85F6C21BA9A67FA329E81B8C801663AF7E992A`로 정확히 일치했다. name 0의 저작권은 Adobe 2014–2021이고 name 13은 OFL 1.1이다. 이전 TTF는 사용중으로 기록하지 않는다.
- 기존 `NotoSansKR-OFL.txt`는 [Google Fonts OFL](https://raw.githubusercontent.com/google/fonts/main/ofl/notosanskr/OFL.txt)과 바이트가 같았다. OFL 본문은 [Noto CJK Sans/LICENSE](https://raw.githubusercontent.com/notofonts/noto-cjk/main/Sans/LICENSE)와 공백을 제외하면 같지만, Google Fonts 파일은 RFN 'Source'를 머리말에 추가한다. 해당 OTF name 0과 Sans/LICENSE에는 그 RFN 표기가 없어 다른 배포본의 머리말을 이 파일의 고유 조건으로 확정하지 않았다.
- [정합 고지 준비본](../../Licence/ThirdParty/NotoSansCJKkr-OFL.txt)은 OTF의 실제 저작권과 공식 Sans/LICENSE의 원문을 결합했다. 공식 LICENSE 본문 4,301바이트의 SHA-256은 `6A73F9541C2DE74158C0E7CF6B0A58EF774F5A780BF191F2D7EC9CC53EFE2BF2`이며 결합 후에도 그대로 유지됨을 검증했다. Client 사본과 중간 개발 APK의 포함도 확인했다.
- 최종 확인한 `SOURCES.txt`는 현재 OTF의 LICENSE 주소와 파일명으로 갱신됐다. `Resources/LegalNotices.txt`에서 정합 고지와 UI 연결 코드를 읽었다. 마지막 재빌드 포함 여부와 Android/앱인토스에서 전문을 끝까지 읽을 수 있는지는 총괄/QA의 최종 검사 항목이다.

기존 사용 자산·패키지의 증분 점검도 완료했다. `Disc.png`의 원형 픽셀과 `Move.wav`의 2,205개 PCM 샘플은 `ProjectBuilder`의 생성식과 대조했다. 저장 프리팹·런타임 코드·생성 도구·Excel 및 패키지 9개의 실제 경로/버전/해시를 대장에 기록했다. 전체 코드의 독점성·법률상 무침해를 보증한 감사는 아니다.

uGUI 2.0.0과 Test Framework 1.6.0은 동봉 LICENSE의 [Unity Companion License](https://unity.com/legal/licenses/unity-companion-license), Custom NUnit 2.0.5는 [Unity Package Distribution License](https://unity.com/legal/licenses/unity-package-distribution-license)와 별도 NUnit 3.5 MIT 고지를 구분했다. 내장 모듈에 개별 LICENSE가 없다는 이유로 MIT/UCL을 추정하지 않았다. [UCL 준비본](../../Licence/ThirdParty/Unity-Companion-License.txt), [uGUI 저작권 준비본](../../Licence/ThirdParty/uGUI-LICENSE.md)을 마련했고, Client의 UCL 전문도 준비본과 공백·문장부호 정규화 후 본문이 같음을 확인했다. 최종 배포 고지와 [Unity Software Terms 2.12](https://unity.com/legal/editor-terms-of-service/software)의 크레딧 표기는 별도 확인한다.

Asset Store는 표준 EULA 외 별도 제공자 조건이 있을 수 있으므로 실제 항목별 검토가 필요하다. 본 세션에서는 자산을 검색·구매·다운로드하지 않았다. [Unity Asset Store 약관](https://unity.com/legal/as-terms)

[ASSET-REGISTER.csv](../../Licence/ASSET-REGISTER.csv)는 실제 확인한 파일/패키지와 미도입 SDK를 구분한다. `PLANNED_NOT_IMPORTED`는 사용 허가 또는 실제 반입의 증거가 아니다. 각 해시는 점검 당시 파일을 식별하며 이후 변경까지 승인하지 않는다. `APK-NOTICE-001`은 2026-10-14 본문30 최종 개발 APK(`05E70261…B3EE4B8D`)의 고지 8개를 원본과 바이트 단위로 대조한 증거다. 새 도형 2개는 외부 이미지 입력 없는 생성식과 실제 픽셀을 비교했고, 기존 폰트·Disc는 보존됐다. 총괄의 최종 WebGL 본문 스크롤/닫기 증거는 `CAPTURE-003`과 [HOST-UI.md](HOST-UI.md)에 연결했다. 이전 본문24 기능 시험과 최종 본문30 재검사를 구분하며, 실제 토스·기기 검증을 대신하지 않는다. 재빌드 시 해시·고지 검사를 갱신하고 출처 불명 항목을 임의로 자체 제작으로 바꾸지 않는다.

## 8. 사용자/출시 담당 준비물과 확인 질문

계정/워크스페이스는 준비됐다는 응답을 받았으므로 재가입을 요구하지 않는다. 아래의 **앱 단위 정보**가 남아 있다.

- 최종 상점명·등록자명·문의 이메일, package name/Toss appName, 사업자 운영 여부·등록 유형, 게임제작/배급업 등록 적용·증빙 및 정산 계좌/증빙. 문서에 실제 민감정보를 적기보다 보관 책임자와 확인 상태만 남긴다.
- Android 서명키 생성/보관 책임자와 백업. 원스토어 서명 옵션, 앱별 접근권한, 사용자 소유 광고 계정/지면 ID. 비밀값을 이 문서나 채팅에 붙여넣을 필요는 없다.
- 실기기 Android 및 iOS 토스 테스트, 광고 지면 승인, 데이터 처리목록, 개인정보 처리방침·지원 URL, 최종 화면자료.
- 원스토어 실제 출시 URL·등급 증빙·GRAC 조회 정보·동일성 스크린샷·대표자 서명 자료.
- 폰트·절차 생성 효과음·현재 패키지와 새 SoftUI 도형의 출처 증거를 대장에 확보했다. 현재 개발 빌드의 APK 고지 포함 및 WebGL 전문 스크롤/닫기를 확인했다. 이후 도입할 SDK 조건과 실제 배포본·기기 고지 표시, 사용자 Unity 라이선스 자격과 최종 브랜드 권리는 별도 확인한다.

출시 담당이 플랫폼에 확인할 구체적 항목(이 세션에서 문의 전송하지 않음):

1. '베팅·현금성 보상·게임머니·PvP가 없는 7×6 로컬 AI 4목 전술 스테이지'가 앱인토스 웹보드 조건 대상인지, 원스토어 공개 등급 경로를 적용할 수 있는지.
2. 새로 공개된 원스토어 게임이 GRAC 검색에 반영되기 전 앱인토스가 수용하는 증빙이 무엇인지. IARC ID/스크린샷만으로 충분하다고 임의 추정하지 않음.
3. 현재 워크스페이스의 광고 사업자 등록 유예 적용 여부, 정산 승인과 실제 송출의 조건. 원스토어 Android 광고 네트워크의 독립 출시 지원 여부.

## 9. 제출/공개 전 완료 증거

| 관문 | 책임 | 필요한 증거 | 현재 |
| --- | --- | --- | --- |
| 게임·콘텐츠 | Client/GameData/QA | 30국면 합법성, 정상 승패·저장, 사람 플레이 시간/재도전 관찰 | 자동/독립 실행 증거는 [QA 상태](../QA/STATUS.md)에 있음. 기기·사람 검증 완료와 구분 |
| 빌드 | Client + 총괄/QA | 마지막 변경을 담은 Android 제출 빌드 / `.ait`, 설치·로딩·오디오·복귀 | 개발 Android/WebGL 빌드 증거 있음. QA-005 수정 재시험 및 최종 빌드 인수 필요 |
| 권리 | 각 자산 작성자 + 총괄 | 실제 사용 항목의 레지스터·LICENSE/NOTICE·권한 | 기존 사용분 증거/NOTICE 준비 완료. 마지막 변경분·최종 고지·사용자 이용 자격은 인수 항목 |
| 개인정보 | 총괄/사업자 | 실제 SDK와 일치하는 처리목록·고지·필요 동의 | 미완료 |
| 원스토어 등급/출시 | 사용자/총괄 | 승인·공개 URL·연령등급·번호/분류일 | 미확보 |
| 앱인토스 등급 | 사용자/총괄 | 웹보드 여부 확인, 자체등급분류 정보/동일성 접수 | 미완료 |
| 광고 | 플랫폼 연동 담당 + 사용자 | 테스트 ID 검증, 실제 지면 승인/송출, 완료 1회 지급, no-fill 진행 | 미완료 |
| 광고 제거 IAP | Client/Server + 사용자/QA | 판매 대상 버전의 실제 광고, 사업자/정산·상품 승인, 가격·지급·복구·환불·검증 증거(12절) | 기획/정책 조사만 완료 |
| 정산 | 사용자/사업자 | 적용 약관·사업자/정산 승인 또는 유예의 조건 확인 | 미확인 |
| 공개 | 총괄 | 제출 자료·실기기 QA·정책상 누락 없음, 승인된 버전 선택 | 미수행 |

## 10. 문서의 최신성·접근 제한

- 위 링크는 모두 2026-10-14에 공식 원문을 열어 내용 확인한 근거다. 날짜가 오래된 게시물은 게시일과 현재 상세 가이드를 구분했다. 본문상 '열기 실패'로 표시한 ONE AdMax 상세 페이지는 예외이며 근거로 채택하지 않았다.
- 앱인토스 구 소개 페이지의 2~3영업일 안내와 현재 FAQ의 3~5영업일 안내가 다르므로 일정은 더 긴 현재 FAQ를 사용한다. 어느 쪽도 개별 앱 승인일을 보장하지 않는다.
- FAQ의 광고 두 형식 설명과 달리 현재 배너 전용 API·오픈 정책은 배너도 명시한다. 구 FAQ 요약만으로 배너 미지원이라고 결론 내리지 않는다.
- 사업자 등록은 일반 '필수' 문구와 구체적인 광고 유예 조항이 공존하므로 위 5절과 실제 콘솔 조건을 함께 확인한다.
- 공식 Unity SDK 저장소가 공개라는 점과 개별 버전 코드의 라이선스 조건은 별개다. 검색에 나타난 다른 Toss 저장소의 MIT 표기를 이 SDK에 적용하지 않았다.
- 출처를 읽은 것과 제출/승인/송출을 실행한 것을 구분한다. 이 역할 세션은 지정 문서·라이선스 증거 파일만 작성했고 SDK 설치, 외부 문의·계정 변경·심사 제출·커밋·배포를 하지 않았다.

## 11. 최종 차단 항목과 독립 진행 가능 범위

기준은 이 문서 인계 시점이다. 최신 실행 상태는 [QA 상태](../QA/STATUS.md), 결함은 [QA 결함](../QA/DEFECTS.md), 제출 문구는 [제출 초안](../Management/SUBMISSION-DRAFT.md)을 따른다. 아래에서 '계정 정보 없이 진행 가능'은 미완료 작업을 계정 대기로 돌리지 않는다는 뜻이며, 이 역할이 Client/QA 작업까지 수행했다는 뜻은 아니다.

### A. 앱 ID·개발사 정보 없이 이번 제작 작업에서 닫을 항목

| ID | 필수 작업 / 현재 구분 | 닫는 증거와 담당 |
| --- | --- | --- |
| L-01 | **WebGL 렌더링·반응형 수정 재시험.** QA-005의 magenta/UI 미표시 실패를 빌드 성공만으로 닫지 않음 | Client 최종 산출물 해시, 총괄의 실제 브라우저 진입·로비/대국/결과·좁고 넓은 화면·입력/저장 재시험, QA 결함 종료 |
| L-02 | **마지막 변경을 포함한 회귀·개발 빌드 동결.** shader/template/고지 UI 변경 뒤 기존 통과가 그대로 유효한 범위를 구분 | Client/QA가 영향 범위를 재시험하고 코드·데이터·빌드 해시를 연결. Android 개발 빌드 검증에는 스토어 앱 ID가 필요하지 않음 |
| L-03 | **현재 개발 빌드 고지 마감 확인.** 본문30 최종 APK 고지 8개 원본 일치, Noto OFL/uGUI/UCL 전문 유지, 신규 도형 출처·해시와 총괄 WebGL 스크롤/닫기 재검사 완료 | [HOST-UI.md](HOST-UI.md)·대장의 고정 해시 및 총괄 `final-*` 증거 기준. 실제 배포본·기기 열람, 적용 Unity 크레딧은 별도이며 재빌드/SDK 추가 시 다시 검사 |
| L-04 | **스토어 제출물의 비개인 초안 확보.** 설명 초안, 직접 도형 SVG/1024·512 PNG 아이콘, Client의 실제 개발 화면 캡처가 있음 | [아이콘 생성 원본](../Management/StoreAssets/generate_icon.py), [SVG 초안](../Management/StoreAssets/icon-draft.svg), [개발 대국 캡처](../../Client/Evidence/webgl-browser-match.jpg)의 경로·해시를 대장에 기록. 최종 브랜드/게임명·콘솔 규격 미확정이며 아이콘 APK 반영으로 기록하지 않음. 마지막 정상 빌드에 맞는 제출 캡처 선정은 root/QA |
| L-05 | **개인정보·등급 설문의 사실 자료.** 6절의 데이터 흐름표와 등급 확인 항목은 준비됨 | 현 구현의 저장·권한·통신·SDK 목록을 실제 빌드와 대조. 운영자/연락처가 없는 문서는 초안이며 게시 완료로 표시하지 않음. SDK가 바뀌면 재검토 |
| L-06 | **앱인토스 이식의 계정 비의존 부분.** stable v3.8.0의 커밋·도구·의존성·패키징 요구 조사 완료(4절). 일반 WebGL 빌드와 `.ait`/토스 실연동은 다름 | Client의 실제 버전 채택/설치·컴파일·템플릿 이식·Mock 오류 경로 검증은 미완료 기술 작업. 앱 ID 부재로 이 준비 전체를 대기 처리하지 않음; 미확정 SDK 권리 근거는 E-07에서 분리 |
| L-07 | **광고 제거 IAP의 계정 비의존 설계/구현.** 상품안과 플랫폼 제약 조사 완료(12절) | Client/Server의 플랫폼별 권한·중복 방지·복구/환불 상태 처리, 구매자의 광고 차단/무료 힌트, 무광고 원스토어 판매 차단. Mock 증거와 실제 결제 증거 분리 |

L-01~03은 현재 Client/root/QA의 마감 작업이다. L-04의 초안 확보와 L-06의 공개 버전·도구·의존성 조사는 완료했지만 최종 결과물 검증과 SDK의 실제 이식은 남아 있어 '계정만 받으면 바로 출시'라고 판단하지 않는다. L-05도 SDK를 실제 도입한 뒤에는 데이터 흐름과 고지를 다시 확정해야 한다. Android 무광고 첫 공개를 선택한 범위에서는 **Android 광고 SDK 미도입만으로 원스토어 등급용 출시를 차단하지 않는다**. 이 경우 광고 기능·메타데이터도 무광고 실제 동작에 맞춰야 한다. 앱인토스 광고 수익화 목표의 미완료는 별도로 유지한다.

### B. 사용자 정보·실환경·외부 결정을 받아야 닫을 항목

| ID | 차단 대상 | 필요한 입력/외부 증거 |
| --- | --- | --- |
| E-01 | 원스토어/앱인토스 최종 등록·서명·공개 문서 | 최종 게임명, 등록자/운영자와 문의 연락처, Android package name·Toss appName, 서명 방식/보관 책임자, 해당 계정의 앱 권한. 비밀키 자체를 문서/채팅에 요구하지 않음 |
| E-02 | 사업·권리·정산 자격 | 실제 운영 형태에 따른 게임업 등록 적용/증빙, 사용자 Unity 이용 자격, 최종 이름/브랜드 확인, 개인정보 담당·공개 지원/처리방침 URL. 계정 준비 완료로 대체 불가 |
| E-03 | 등급 경로와 공개 | 앱인토스의 해당 게임 웹보드 분류 확인, 원스토어 실제 등급·번호/일자·공개 URL, 동일 게임 정보/화면/대표자 자료, 토스 접수·심사 결과. 설문 초안이 등급 판정은 아님 |
| E-04 | 앱인토스 실제 광고·정산 | 실제 앱/지면 등록과 승인 상태, 해당 워크스페이스의 사업자/정산 또는 유예 적용 확인, 공식 테스트 광고 및 제출용 실연동 검사. 모의 광고 결과로 충족 불가 |
| E-05 | 기기 적합성·사용자 경험 | Android 실제 설치 기기 및 Android/iOS 토스 앱 환경에서 수명주기·입력·성능·고지·광고 검사, 실제 사람 플레이 관찰. 데스크톱·대체 구현 테스트로 대체 불가; 기기/테스터 확보 전에는 대기 상태 명시 |
| E-06 | 최종 공개 | 서명한 제출물·등급·개인정보·권리·실환경 검사 충족 후 플랫폼 검수 및 공개 상태. 검수 기간과 승인 결과는 외부 의존 |
| E-07 | Toss SDK 사용 조건 확정 | 실제 도입할 버전의 상업적 임베딩·수정·배포·소스 공개·고지 범위를 정한 적용 계약 또는 권리자 공식 안내. v3.8.0의 공개 소스/태그 확인으로 이 조건을 임의 승인하지 않음 |
| E-08 | 광고 제거 상품 판매 개시 | 토스 IAP 사업자·정산/약관, 플랫폼별 상품 ID·실제 가격/승인, 검증 API 인증 준비, 동일 플랫폼 구매 복구 및 환불 실환경 검사. 무광고 원스토어 버전은 판매 대상에서 제외 |

두 플랫폼의 계정/워크스페이스를 다시 만들 필요는 없다. 기존 코어 범위와 달리 목표에는 광고 제거 IAP와 최소 구매 서버가 추가됐다. 자체 회원가입·랭킹·추가 스토어는 포함하지 않는다. GitHub 공개 업로드의 권한/인증 문제는 총괄의 별도 작업이며, 스토어 출시 요건과 혼동하지 않는다. 이 문서와 대장의 완료는 코어 프로토타입 또는 개발 APK를 출시 완료로 승격시키지 않는다.

## 12. 광고 제거 1회 구매(IAP) — 2026-10-14 추가

**제품안:** 한 번 구매하면 이 게임의 전면·배너 광고를 제거하고 힌트를 판당 1회 광고 없이 제공한다. 구독·재화·무제한 힌트가 아니다. **3,900원은 소비자가 제안이며 등록/검증된 가격이 아니다.** 토스와 원스토어의 SKU·권한·복구를 분리하고 플랫폼 간 이전을 약속하지 않는다. **광고 SDK가 없고 제거할 광고도 없는 원스토어 버전은 이 상품을 판매하지 않는다.** 해당 버전의 구매 UI·상품 노출·결제 호출을 막고 제출 설명도 일치시킨다.

### 지원·지급·복구·환불

| 구분 | 앱인토스 Unity WebGL | 원스토어 Android |
| --- | --- | --- |
| 영구형 지원 | 비소모품으로 광고 제거를 명시. IAP 연동 전 사업자 등록·약관·정산 정보 필요. 광고 등록 유예와 다름. 공급가 400~1,400,000원, 10원 단위 입력 후 VAT 포함 판매가 계산 [상품/환불 안내](https://developers-apps-in-toss.toss.im/iap/intro.html) | 관리형 `INAPP`을 **소비하지 않고** 구매 확인하여 영구형으로 사용. 비소모품에 `consume` 호출 금지. 검증·지급 후 `acknowledge`; **3일 이내 확인하지 않으면 자동 환불** [SDK 가이드](https://onestore-dev.gitbook.io/dev/tools/billing/v21/sdk) |
| 정상 지급 | `createOneTimePurchaseOrder`의 `processProductGrant`에서 주문별 권한 저장·지급 성공 후에만 `true`. 오류/취소/대기·이미 보유 상태를 구분. 기능 종료 시 cleanup. 구버전 지원 여부 확인 [단건 구매 API](https://developers-apps-in-toss.toss.im/documentation/sdk/domains-api/iap/iap.createonetimepurchaseorder) | SDK 구매 응답의 서명 검증(SHA512withRSA)과 서버 구매 상태 확인. 보유 여부·중복 토큰 검사 후 권한을 저장하고 구매 확인 재시도. 서명만으로 최신 환불 상태를 보장하지 않음 [보안 가이드](https://onestore-dev.gitbook.io/dev/tools/billing/v21/test) |
| 복구 | 미결 주문은 `getPendingOrders` → 검증/중복 없는 지급 → `completeProductGrant`. 이것은 이미 지급된 영구 권한의 재설치 복구와 별개 [연동 흐름](https://developers-apps-in-toss.toss.im/iap/intro.html). 완료/환불 조회 API는 `hasNext/nextKey`가 있어도 **웹 SDK는 첫 페이지만 반환**하므로 전 이력 복구로 가정 금지 [완료/환불 API](https://developers-apps-in-toss.toss.im/documentation/sdk/domains-api/iap/iap.getcompletedorrefundedorders) | 시작/복귀와 '구매 복구'에서 `queryPurchasesAsync(INAPP)`로 미소비 구매를 재조회. 네트워크 단절·다른 기기에서 누락된 콜백도 처리. 같은 원스토어 계정의 재설치/기기 변경을 실기기로 검증 [SDK 가이드](https://onestore-dev.gitbook.io/dev/tools/billing/v21/sdk) |
| 환불/취소 | Android는 토스 요청·파트너 처리 후 Google 최종 결정, iOS는 Apple이 판단하며 파트너는 상태 확인. 환불 **요청**만으로 권한 회수하지 않음 [환불 안내](https://developers-apps-in-toss.toss.im/iap/intro.html). mTLS 서버 API로 주문의 최신 상태 재확인 [결제 상태 API](https://developers-apps-in-toss.toss.im/api/iap) | OAuth 서버 API의 `getPurchaseDetails`·`getVoidedPurchases`로 취소 확인. 취소 이력은 조회 기간/페이지 누락 없이 수집 [서버 API](https://onestore-dev.gitbook.io/dev/tools/billing/v21/serverapi). PNS는 서명 검증 후 재조회 계기로 사용; 알림 지연/유실이 있어 단독 원장으로 쓰지 않음 [PNS](https://onestore-dev.gitbook.io/dev/tools/billing/v21/pns) |

**서버 범위는 프로젝트 설계 결정이다.** 토스 안내는 서버 또는 앱 저장소 지급을 설명하지만, 이 프로젝트는 영구 권한의 복구·환불을 위해 최소 서버 원장을 계획한다. 플랫폼·상품·주문/구매 토큰·구매자 연결·권한 상태·검증 시각·지급 확인 상태를 저장하고 재전송을 중복 처리하지 않는다. 구매자와 주문의 소유 관계를 검증하는 경로는 플랫폼별로 확정해야 하며, 클라이언트가 보낸 식별자/주문번호만으로 타인의 구매를 연결하면 안 된다. 토스 특정 주문 상태 API만으로 구매자 소유 관계까지 증명된다고 간주하지 않는다. mTLS 개인키와 원스토어 OAuth client secret은 서버에만 보관한다.

재설치 시 서버 원장을 찾을 수 있는 안정적인 플랫폼 사용자 연결과 공식 구매 복구를 시험하기 전에는 '기기 변경도 자동 복구'를 표시하지 않는다. 특히 토스 첫 페이지 밖의 주문 복구와 Android↔iOS 동일 사용자 동작은 미확인이다. `getPendingOrders` 세부 페이지의 '결제 미완료' 설명은 개요의 '결제 완료·지급 미완료' 설명과 달라, 목록에 있다는 이유만으로 지급하지 않고 최신 결제 상태를 검증한다. [미결 API](https://developers-apps-in-toss.toss.im/documentation/sdk/domains-api/iap/iap.getpendingorders)

### 가격·비용·도입 권리

- **토스:** 정산 문서는 앱마켓 공급가 기준 15%(수익 증가 시 30% 가능), 토스 결제액 기준 5%와 수수료 VAT를 구분한다. CBT 중 토스 0% 문구도 있으나 현재 계정 적용 여부는 미확인이다. 보수적으로 표준 수수료로 계획하고 적용 계약으로 확정한다. Android/iOS 정산 예시가 달라 판매가의 단순 20% 차감으로 순수익을 확정하지 않는다. 광고 수수료 면제를 IAP에 대입하지 않는다. [토스 정산](https://developers-apps-in-toss.toss.im/guide/settlement)
- **원스토어 대한민국 Android:** 기본 사용자 결제액 20%, 원스토어 제공 할인액에는 50% 규정, 금액/수수료 VAT 포함. 개발자 합산 월 거래액 US$5,000 이하 등 조건 충족 시 기본 수수료율의 50% 감면이 가능하나 실제 자격은 미확인이다. 구독 10%나 ONEplay 웹게임 30%를 이 비소모성 Android 상품에 적용하지 않는다. [국내 수수료](https://onestore-dev.gitbook.io/dev/docs/payment/service_fee/korea)
- 양쪽 UI는 조회된 실제 상품명·통화·판매가·혜택으로 결제를 안내한다. 3,900원을 고정 표시한 채 다른 가격으로 결제하지 않는다. 환불·운영/서버비·세금 및 광고 감소를 별도 반영하며 현재 매출 검증치는 없다.
- Toss Unity SDK의 권리 미확정 사항은 4절/E-07을 유지한다. 원스토어 공식 Unity 플러그인은 확인일 [v1.3.4](https://github.com/ONE-store/unity_plugins/releases/tag/v1.3.4), Java IAP 21.04.00을 안내한다. 해당 태그의 [LICENSE](https://github.com/ONE-store/unity_plugins/blob/v1.3.4/LICENSE) 원문을 공식 GitHub API로 확인했으며 Apache-2.0이다. Maven SDK·EDM4U 등 의존물 전체의 조건 확인을 대체하지 않는다. 실제 채택 버전·LICENSE/NOTICE·Unity 6000.3 호환은 도입 시 점검한다. [Unity 연동](https://onestore-dev.gitbook.io/dev/tools/billing/v21/unity). **이 세션은 SDK를 설치하지 않았다.**

### 판매 전 닫을 증거

1. 플랫폼별 앱/SKU·사업자/정산·실제 가격·상품 설명·SDK/의존물 권리 및 구매용 데이터 처리목록 확정. 제거할 광고가 없는 버전에서는 구매 경로가 열리지 않음.
2. 구매 성공·취소·실패·대기, 지급 직전 종료/네트워크 단절, 콜백 중복, 이미 보유, 구매 확인 재시도, 재설치/기기 변경 복구, 계정 변경, 환불 후 권한 갱신을 실환경에서 확인. 원스토어는 배포 OS의 샌드박스 시험을 요구하며 상용 시험은 실제 과금 가능성이 있음 [테스트 조건](https://onestore-dev.gitbook.io/dev/tools/billing/v21/test).
3. 권한 유효 시 배너 제거와 모든 광고 로드/표시·늦은 콜백 차단, 광고 없이 힌트 1회/판, 중복 구매 방지를 확인. 검증 서버 장애/조회 실패를 환불로 간주하지 않고 마지막 검증 권한의 처리와 재시도 정책을 정한다. 신규 권한은 미검증 성공으로 만들지 않는다.

상품/계정 등록·약관 동의·서버 배포·실제 결제/환불은 수행하지 않았다. 위 내용은 기획·공식 문서 조사이며 구현·심사 통과 증거가 아니다.
