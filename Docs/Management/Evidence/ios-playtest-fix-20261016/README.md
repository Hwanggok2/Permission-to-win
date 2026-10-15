# iPhone 카카오톡 외부 테스트 결함 대응 — 2026-10-16

상태: 클라이언트 수정·검증, WebGL 재빌드, 동일 외부 주소 갱신과 데스크톱 브라우저 검증 완료. **실제 iPhone 카카오톡 재시험은 대기 중**이다. 아래 재현 기록은 수정 전 배포판이다.

- 사용자 기기: iPhone, 카카오톡 내장 브라우저. iOS/카카오톡 버전은 미제공.
- 오류: `Loading FSB failed for audio clip "TypingKey".` 반복 및 `EncodingError: Decoding failed` JavaScript 경고. 사용자 첨부 화면으로 확인했다.
- 레이아웃: 모바일 웹뷰에서 제목 위 공백이 크고 대국판/조작부가 하단에 몰린다. 기존 배포판의 데스크톱 Chromium을 390×664 CSS 픽셀로 줄여서 비슷한 배치를 재현했다.
- 기존 타이핑 WAV는 mono 44.1kHz PCM, 약 35ms(1,544샘플). 짧은 AAC 변환/디코딩 문제가 유력한 원인이다. Windows AAC 변환에서 앞부분 음원 손실 가능성도 확인 대상이다.
- 기존 브라우저 검사: innerWidth/Height 390×664, visualViewport 390×664·offsetTop 0, canvas CSS 390×664 / buffer 585×996. 제목 중심은 상단 약 176px. 브라우저 도구막대만으로 설명할 수 없는 앱 내 공백이다.
- 입력 후 로비→첫 스테이지→타이핑 대사 경로를 실행했으나 데스크톱 Chromium에서는 같은 오디오 오류가 재현되지 않았다. 이 결과를 iPhone 통과로 간주하지 않는다.

수정 전 증거: [로비 390×664](before-lobby-390x664.png).

중간 오디오 검사: Unity FSBTool로 변환한 9개 AAC 후보와 원본 WAV 3개를 localhost 전용 검사 페이지에서 버튼 입력으로 디코딩했다. Windows Chromium 155는 12개 모두 성공했으나, TypingKey의 원본/뒤 무음 후보 peak는 약 0.0986으로 원본 WAV 0.5500보다 감소했다. 앞뒤 무음 후보는 0.5448이었다. ButtonClick도 0.0381 → 0.2795로 보존됐다(원본 WAV 0.2800). [브라우저 원시 결과](chromium-codec-probe.json). 이는 실제 Web Audio 디코딩·신호 검사이며 사람 청취나 iOS 호환성 검증은 아니다.

검증할 범위: 짧은 효과음의 원본 보존과 변환 후 신호, 실제 웹뷰 크기와 Unity 안전영역 일치, 일반 브라우저와 Toss 호스트 여백 분리, 저장된 프리팹의 화면 배치, 새 WebGL 빌드와 동일 테스트 주소의 UI/오류 확인. 실제 iPhone 카카오톡에서 재시험하기 전에는 해당 환경 해결을 확정하지 않는다.

클라이언트 인계: [수정·검사 결과](../../../../Client/Evidence/web-audio-fsb-20261016/verification.json). 최신 결과 기준 Unity 23/23, 웹뷰 계산 10/10 통과. 첫 Unity 검사에서 오디오 PCM 허용오차 관련 2건 실패 후 원본 바이트 보존 검사와 16-bit 양자화 비교를 수정하여 재검사 5/5 통과했다. 실패 이력을 삭제하지 않았다.

수정 내용: TypingKey/ButtonClick 원본을 보존하고 앞 2,048샘플 보호 무음·뒤 무음을 둔 0.2초 복사본을 저장 프리팹에 연결했다. 실제 Unity WebGL 임포트 AAC에서도 원본 대비 peak 약 99%/100%를 유지했다. 일반 웹뷰에 무조건 더하던 64px 호스트 여백을 제거하고, 명시적 호스트/미리보기에서는 보존했다. visualViewport의 실제 표시 크기를 따라 캔버스를 맞추고 추가 축소를 제거했으며 버튼 최소 크기를 CSS 픽셀로 계산한다.

Unity 캡처에서 390×664 대국 제목 중심 y≈111px, 판 너비≈337px, 하단 버튼 표시를 확인했다. DPR 1/1.5/2와 390×844, 454×768, 1080×1920, Toss 미리보기는 [화면 증거](../../../../Client/Evidence/web-audio-fsb-20261016/screens/)에 있다. 웹 실제 렌더링과 구분한다.

빌드 완료: 2026-10-16 01:34 KST. Unity 6000.3.15f1 WebGL 개발 빌드 **오류 0·경고 0**, 46.745초. 클라이언트 동결 목록 532개 SHA-256은 빌드 전후 모두 일치했다. [빌드 결과](build-summary.txt), [공개 파일 18개 해시](published-files.json), [검증 수치](build-verification.json).

배포: [기존 친구 테스트 주소](https://shares-country-steel-fridge.trycloudflare.com/)를 새 빌드 전용 스냅샷으로 교체했다. 게임 루트·JS·WASM·데이터·BGM 출처 고지는 HTTP 200, 비공개 문서·`.env`·폴더 목록은 404. 공개 host-layout.js와 새 빌드 해시가 일치한다. [HTTP 검사](public-http.json). PC·서버·터널이 켜진 동안만 사용 가능하며 광고·결제와 앱 출시 상태는 변경하지 않았다.

공개 주소 브라우저 검증: Windows Chromium에서 실제 입력으로 로비 → 스테이지 상세 → 시작 대사 → 열 미리보기 → 놓기 → AI 응수 → 다시하기 확인 → 대국 초기화를 확인했다. 390×664 및 390×844 CSS 화면에서 캔버스가 표시 영역과 일치하고, 판·하단 버튼이 보인다. 390×664에서 제목 중심 y≈111px, 판 너비≈337px로 Unity 캡처와 같은 배치다. 게임 페이지의 warning/error 로그는 0개다. [브라우저 결과](browser-verification.json), [로비](after-lobby-390x664.png), [대국 390×664](after-match-390x664.png), [대국 390×844](after-match-390x844.png), [재도전 확인창](retry-confirm-390x664.png).

기기 재시험: 카카오톡에서 기존 게임 창을 닫고 같은 주소를 다시 열어 시작 대사·착수·다시하기 중 FSB/EncodingError가 없는지, 상하단 UI가 잘리지 않는지 확인한다. 현재 브라우저 결과는 iOS WebKit 디코더 통과나 사람 청취를 뜻하지 않는다.

공식 기술 근거(2026-10-16 확인): [Unity 6.3 Web Audio](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-audio.html), [Unity 담당자의 짧은 AAC 및 Windows priming 설명](https://discussions.unity.com/t/loading-fsb-failed-for-audio/796691/13), [앞부분 소리 손실 설명](https://discussions.unity.com/t/loading-fsb-failed-for-audio/796691/16).
