# 광고 제거 설명 문구 간소화

2026-10-18 사용자 추가 수정: **전면·배너 광고가 영구적으로 사라집니다**.

- Excel `Strings!C499` (`ad_offer_permanent` 한국어)에서 “구매하면 ”만 제거했다. 모든 시트의 다른 값·셀 서식·행/열 크기·고정창·유효성 등 보존 비교를 통과했다. 영어 문구는 그대로다.
- exporter 재생성 및 `--check` 통과. Client Resources와 저장된 `AdRemovalOffer` 프리팹, 독립 HTML 시안과 현재 설계/계약 문구에 반영했다.
- Unity 에디터 프리팹 적용·검증 성공. [Excel 렌더](excel-copy.png) 및 실제 브라우저 [수정 시안](preview.png)의 문구·배치를 확인했다. 동작 코드·광고 조건은 바꾸지 않았고 기존40개 동작 검사를 이번 문구 변경으로 재실행하지 않았다.
- 웹 빌드·배포 커밋 상태는 아래 후속 기록을 따른다. 광고/IAP 미연동 기본값과 사용자 직접 푸시 방식을 유지한다.

기존 구현의 [40/40·11화면 검증](../ad-offer-implementation-20261018/README.md)은 이전 문구 시점의 기록이다.

최종 WebGL 빌드 성공: 오류0·경고0,91,695,842bytes,27.186초. [빌드 결과](build-WebGL.txt). 배포 커밋 **c181eff**를 준비했고 기존483cad9의 패널·광고 조건·첫 안내 수정도 포함한다.23파일 복사·해시 대조 완료, Pages 작업 폴더는 깨끗하다. 에이전트 푸시·새 커밋의 공개 반영 확인은 하지 않았다. [배포 파일](published-files.json) · [소스 해시](source-files.json).

## 사용자 푸시 후 공개 확인

2026-10-18 사용자 푸시 완료 안내 이후 **c181eff**의 [Pages 작업39128678351](https://github.com/Hwanggok2/Permission-to-win/actions/runs/39128678351) 성공과 공개23파일 SHA-256/크기 일치를 확인했다. [대조 결과](public-deployment-verification.json). [공개 게임](https://hwanggok2.github.io/Permission-to-win/)을 다시 로드해 [로비](public-lobby.png)와 [멀티 메뉴](public-multiplayer.png)의 실제 표시·진입·닫기를 확인했다.

따라서 최신 문구·첫 안내 수정·멀티 UI·문시온 이미지가 포함된 동일 빌드가 공개됐다. 광고/IAP 미연동으로 판매 홍보는 공개판에서 숨겨지며 새 문구가 공개 화면에 팝업으로 표시되는 것을 확인한 것은 아니다. 새 실제 대전·결제·광고 SDK·실기기 검사는 수행하지 않았다.
