# 판단 근거

**수익 가정 갱신 2026-10-15:** [수익 예상·목표](Revenue/README.md)에 공식 광고 계산식·정산/IAP 수수료·수취 일정 변경 공지와 확인일을 기록했다. eCPM·노출·구매율은 공식 통계가 아니라 계획 변수다. 아래 2026-10-14 항목은 기존 조사 이력이다.

확인 기준 2026-10-14. 가격 3,900원·광고 간격·재미 목표는 StageGame의 기획값이며 플랫폼 의무나 매출 전망이 아니다. 서버는 사용자 요청으로 무료 플랜을 기준으로 변경했다.

| 내용 | 근거 |
| --- | --- |
| 게임 선택·경쟁 기능·차별화 가설 | [기획 비교](../Docs/Design/DECISION.md) |
| 원스토어 영구성 관리형 상품·acknowledge·복구 | [상품 유형](https://onestore-dev.gitbook.io/dev/tools/billing/v21/ov), [SDK 처리](https://onestore-dev.gitbook.io/dev/tools/billing/v21/sdk) |
| 원스토어 서버 구매 상태·취소·구매 확인 | [서버 API](https://onestore-dev.gitbook.io/dev/tools/billing/v21/serverapi) |
| 토스 IAP·복구·비용·연동 조건 | [최신 검토와 공식 링크](../Docs/Design/RELEASE-REQUIREMENTS.md) |
| Workers Free의 무료 요청·CPU 한도 | [Workers 가격](https://developers.cloudflare.com/workers/platform/pricing/), [제한](https://developers.cloudflare.com/workers/platform/limits/) |
| D1 무료 한도·소진 시 쿼리 오류 | [D1 가격](https://developers.cloudflare.com/d1/platform/pricing/) |
| 토스 서버 인증에 사용할 outbound mTLS 기능·대상 제한 | [Workers mTLS](https://developers.cloudflare.com/workers/runtime-apis/bindings/mtls/), [모든 Workers 고객 대상 제공 발표](https://blog.cloudflare.com/mtls-workers/) — 실제 토스 인증서 연결은 별도 검증 |
| 원스토어 단독 등록 시 AdMob 심사 제약 | [지원 스토어 안내](https://support.google.com/admob/answer/9989980) |
| 실행 검증·남은 출시 항목 | [QA 현황](../Docs/QA/STATUS.md), [빌드 기록](../Client/IMPLEMENTATION.md) |
| 자산·SDK 라이선스 | [사용 자산 대장](../Licence/ASSET-REGISTER.csv) |
| 토스 상단 조작 영역·CSS 좌표·별도 여백 | [공식 Safe Area](https://developers-apps-in-toss.toss.im/documentation/common/screen/safe-area), [조회 API](https://developers-apps-in-toss.toss.im/documentation/sdk/domains-api/safearea/getsafeareainsets), [조사 및 자체 64px 설계값의 한계](../Docs/Design/HOST-UI.md) |
| 게임 내비게이션·전체 화면 요구 | [공식 내비게이션](https://developers-apps-in-toss.toss.im/documentation/common/navigationbar), [게임 출시 가이드](https://developers-apps-in-toss.toss.im/checklist/app-game.html) |

현재 요약의 `assets/settings-host.jpg`와 `gameplay-host.jpg`는 [총괄 브라우저 증거](../Docs/Management/Evidence/host-ui-browser-20261014/README.md)의 `final-ko-settings-390.jpg`와 `ko-match-390.jpg`를 그대로 복사했다. 상단은 테스트용 모의 표시이며 실제 토스 화면이 아니다. 대국 캡처 이후 최종 변경은 라이선스 본문의 글자 크기 확대다.

이전 화면 비교용 `assets/gameplay.png`, `settings-ko.png`, `match-en.png`, `settings-browser.jpg`도 보존한다. `assets/architecture.svg`는 서버 계획을 직접 그린 설명도이며 배포 완료 증거가 아니다.

`assets/overview-desktop.jpg`와 `overview-preview.jpg`는 이 요약 HTML의 브라우저 캡처다. 1280/390/320px 폭에서 확인했고, 문서 가로 넘침·이미지 로딩 실패·관찰된 콘솔 오류는 없었다. 구성도는 좁은 화면에서 가로 스크롤한다. 내부 파일 링크와 원본 플레이 이미지 해시도 확인했다.

앞선 다국어 개발 단계의 언어 전환·복원과 크기 변경 증거는 [이전 브라우저 기록](../Docs/Management/Evidence/localization-browser-20261014/README.md)에 보존한다. 최신 상단/그래픽 검증과 빌드 해시는 위 호스트 UI 기록을 따른다.
