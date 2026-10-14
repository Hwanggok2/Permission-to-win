# StageGame

짧은 AI 대전 스테이지와 광고 수익화를 목표로 하는 Unity 게임 프로젝트.

- 개발 기간: 2026-10-14 ~ 2026-10-20 (KST), 제출 목표: 2026-10-21.
- 플랫폼 순서: 원스토어 Android → 등급 증빙 확보 → 앱인토스 WebGL.
- 종목: 7×6 중력식 4목, 오프라인 AI 전술 스테이지 30개. 규칙/데이터는 [확정 계약](Docs/Management/CONTRACT.md).
- 원칙: 프리팹 UI, Excel 원본 게임데이터, 오프라인 AI, 플랫폼별 광고 어댑터.
- 현재 상태/일정: [총괄 보드](Docs/Management/STATUS.md).

## 실행과 검증

개발 APK와 로컬 브라우저 실행은 [플레이테스트 안내](Docs/Management/PLAYTEST.md)를 따른다.

Unity Hub에서 `Client`를 Unity 6000.3.15f1로 열고 `Assets/StageGame/Scenes/Main.unity`를 실행한다. 화면·보드·공통 버튼은 저장된 프리팹이며, 자세한 빌드 명령과 검증 범위는 [클라이언트 기록](Client/IMPLEMENTATION.md)에 있다.

게임데이터 원본은 `GameData/GameData_MVP.xlsx`다. 수정 후 [내보내기 안내](GameData/README.md)에 따라 JSON을 검증·생성하고 Client에 반영한다. [QA 현황](Docs/QA/STATUS.md)은 자동 검사와 실제 기기 검증을 구분한다.

현재는 개발용 프로토타입이다. 실제 광고 SDK·토스 `.ait`·릴리스 서명·스토어 심사는 완료되지 않았다. 재미/수익성은 [사람 플레이 검증 계획](Docs/Management/VALIDATION.md)에 따라 확인한다.

| 경로 | 역할 |
| --- | --- |
| Client | Unity 프로젝트, 프리팹/씬, 게임 로직, 플랫폼 연동 |
| Server | 서버 필요성 및 향후 API 범위 |
| GameData | Excel 원본, 내보내기 도구, 데이터 검증 |
| Licence | 자산/SDK 출처, 사용 조건, 권리 검토 |
| Docs/Design | 게임 후보 비교, 시장성 가설, 출시 요건 |
| Docs/QA | QA 계획, 결함, 실제 검증 증거 |
| Docs/Management | 확정 범위, 공통 계약, 일정, 위험/진행 현황 |

스토어 심사·계정 승인·광고 승인 완료일은 개발 완료와 별개이며 보장하지 않는다.
