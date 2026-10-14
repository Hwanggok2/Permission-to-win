# 판단 근거

확인 기준 2026-10-14. 가격 3,900원·광고 간격·서버 예산·재미 목표는 StageGame의 기획값이며 플랫폼 의무나 매출 전망이 아니다.

| 내용 | 근거 |
| --- | --- |
| 게임 선택·경쟁 기능·차별화 가설 | [기획 비교](../Docs/Design/DECISION.md) |
| 원스토어 영구성 관리형 상품·acknowledge·복구 | [상품 유형](https://onestore-dev.gitbook.io/dev/tools/billing/v21/ov), [SDK 처리](https://onestore-dev.gitbook.io/dev/tools/billing/v21/sdk) |
| 원스토어 서버 구매 상태·취소·구매 확인 | [서버 API](https://onestore-dev.gitbook.io/dev/tools/billing/v21/serverapi) |
| 토스 IAP·복구·비용·연동 조건 | [최신 검토와 공식 링크](../Docs/Design/RELEASE-REQUIREMENTS.md) |
| Cloud Run 사용량 과금·리전별 비용 | [공식 가격](https://cloud.google.com/run/pricing) |
| Firestore 저장/조회 비용 | [공식 가격](https://cloud.google.com/firestore/pricing) |
| 예산 알림이 자동 지출 차단을 뜻하지 않음 | [Cloud Billing 예산](https://docs.cloud.google.com/billing/docs/how-to/budgets) |
| 원스토어 단독 등록 시 AdMob 심사 제약 | [지원 스토어 안내](https://support.google.com/admob/answer/9989980) |
| 실행 검증·남은 출시 항목 | [QA 현황](../Docs/QA/STATUS.md), [빌드 기록](../Client/IMPLEMENTATION.md) |
| 자산·SDK 라이선스 | [사용 자산 대장](../Licence/ASSET-REGISTER.csv) |

`assets/gameplay.png`는 `Client/Evidence/match.png`를 그대로 복사한 실제 Unity PlayMode 렌더다. 실기기 사진이 아니다. `assets/architecture.svg`는 이번 서버 계획을 직접 그린 설명도이며 배포 완료 증거가 아니다.

`assets/overview-desktop.jpg`와 `overview-preview.jpg`는 이 요약 HTML의 브라우저 캡처다. 1280/390/320px 폭에서 확인했고, 문서 가로 넘침·이미지 로딩 실패·관찰된 콘솔 오류는 없었다. 구성도는 좁은 화면에서 가로 스크롤한다. 내부 파일 링크와 원본 플레이 이미지 해시도 확인했다.
