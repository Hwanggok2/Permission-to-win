# WebGL 짧은 오디오·모바일 여백 수정 — 2026-10-16

상태: **Client 소스 동결, 프리팹 저장 완료, Unity 종료. 총괄 WebGL 재빌드 가능.** `source-freeze.json`은 Assets/Packages/ProjectSettings 532파일의 SHA-256이다. Client에서 새 플레이어 빌드·배포·커밋·푸시는 하지 않았다. 현재 빌드 19파일은 시작 시점과 동일하다.

후속: 위 보존 검증과 Client 배치 종료 후 총괄이 동결본 인수를 확인하고 WebGL 빌드를 시작했다. 이후 프로젝트 잠금·빌드 산출물 변경은 총괄 작업일 수 있다. Client는 Assets/Packages/ProjectSettings를 더 수정하지 않는다.

## 적용

- 원본 TypingKey 35ms, ButtonClick 55ms는 유지했다. 앞에 2048개의 0 샘플(44.1kHz에서 약 46.44ms), 뒤에 보호 무음을 넣은 0.2초 `TypingKeyAacSafe.wav`, `ButtonClickAacSafe.wav`를 별도로 생성했다. 실제 소리의 PCM 바이트는 원본 그대로다. RankAppRoot의 직렬화된 두 참조와 RankUiBuilder의 생성 경로를 변경했다. `Tools/PrepareWebAudio.py`로 재현하며 다른 내용의 기존 파일은 덮어쓰지 않는다.
- AAC 인코더 초기 구간이 원래의 짧은 타격음을 크게 손상시켰다. 끝 무음만 추가해도 실제 소리는 복원되지 않았다. 앞 보호 무음을 사용한 두 효과음은 재생 시작 후 약 46ms에 들린다. 대사 40ms 간격·음량·음악 재생 로직은 유지했다. 돌 소리와 BGM 원본/참조는 유지했다.
- 일반 브라우저/카카오 웹뷰에서 무조건 더하던 호스트용 64 CSS px를 제거했다. 실제 `setInsets()` 또는 `hostPreview=1`을 사용하면 기존 Toss 여백과 실제 controlsBottom을 적용한다. OS inset은 최대값으로 합쳐 중복 합산하지 않는다.
- `visualViewport`의 폭·높이·offset을 플레이어 컨테이너에 반영하고 resize/scroll에 갱신한다. 툴바가 이미 줄인 viewport를 다시 inset으로 빼지 않는다.
- CanvasScaler Expand 적용 후 SafeAreaFitter가 폭 배율을 1 이하로 제한해 한 번 더 작아지던 구간을 수정했다. RankMatchFitter의 최소 터치 크기는 framebuffer가 아닌 CSS 픽셀로 계산한다. 저장된 1080×1920 배치·상단 216 설계 단위 여백·하단 여백·아트는 그대로다.

## 검증

- Unity 고유 검사 **23/23** 최종 통과. 첫 실행 `unity-tests.xml`은 19통과/2실패였다. 실패는 임포트 PCM에 원본과 완전히 같은 샘플을 요구한 검사로, 임포트 과정의 미세한 ringing을 검출했다. 검사를 원본 WAV 바이트 보존과 임포트 후 소리 크기로 분리했고 `unity-audio-tests.xml` **5/5**에서 두 실패 항목·기존 자산 검사·추가 참조/임포트 검사 모두 통과했다. 중복 제거한 항목별 최종 결과는 `verification.json`에 있다. 미실행을 통과로 합산하지 않았다.
- 실제 active target **WebGL**, 실제 Unity AudioImporter 결과 **AAC**를 `AssetDatabaseExperimental.GetArtifactPaths()`로 추출했다. `unity-import/*.resource`가 실제 MP4/AAC 스트림이다. 단순 WAV 브라우저 재생만 검사한 것이 아니다. FFmpeg로 그 결과를 디코딩한 값은 `codec-report.json`이다.

| 음원 | 원본 peak | 수정 전 Unity AAC peak | 보호 무음 적용 AAC peak | 에너지 보존 |
|---|---:|---:|---:|---:|
| TypingKey | 0.5500 | 0.0986 | 0.5448 | 99.58% |
| ButtonClick | 0.2800 | 0.0381 | 0.2795 | 99.66% |
| GoStonePlaceHeavy (유지) | 0.6500 | 0.6504 | 변경 없음 | 기존 AAC 약 98.43% |

- Unity 소스 임포트/메타 보존, 저장된 참조, 실제 Play 중 버튼음·돌 소리·대사 타이핑/음소거·모달과 로비 전환 중 음악 유지 등을 검사했다. BGM은 126초가 넘는 AAC/CompressedInMemory로 짧은 클립 문제 대상이 아니며 파일과 메타는 유지했다.
- 웹뷰 어댑터·브리지 **10/10** Node VM 검사. 네 viewport, visualViewport 크기/offset 변경, fallback, SDK/preview inset, 비정상 값, 8-float WebGL ABI를 검사했다. 실제 브라우저 검사가 아닌 순수 로직 검사다.
- **27개 Unity 화면 캡처**: 390×664, 390×844, 454×768, 1080×1920 로비/대국 한영. 390×664는 DPR 1/1.5/2, Toss 390×844는 DPR 1/1.5/2. RenderTexture에 실제 Expand 공식을 적용한 검사이며 실제 아이폰 캡처로 표시하지 않는다. 모든 일반 viewport에서 하단 버튼이 들어오며 DPR 간 위치/크기가 유지된다. `screens/viewport-metrics.json` 참조.
- 390×664: 대국 제목 중심 y=110.92 CSS px, 보드 폭337.36, 최소 버튼49.29, 하단 버튼의 바닥19.73. 로비 제목 y=115.79. 기존 총괄 브라우저 측정은 대국 약170/로비176이므로 약60px 상향이다. 전후는 다른 렌더 환경의 비교이므로 새 WebGL에서 재확인이 필요하다.
- 시작 snapshot과 비교하여 기존 오디오/메타·아트·게임 데이터·Core 코드·씬·다른 프리팹·기존 빌드가 모두 동일함을 확인했다. RankAppRoot만 오디오 참조 저장 대상으로 변경했다. 직전 전 레이팅 조정 결과도 그대로 유지했다.

## 다음 검증: 총괄 빌드 및 아이폰

1. 동결본에서 기존 StageGamePortrait 템플릿으로 WebGL 재빌드. 새 framework/data 및 JS 브리지 8개 인자를 함께 배포한다. 기존 HTML/JS 또는 Unity data가 섞이지 않게 캐시를 갱신한다.
2. 동일 URL의 새 빌드에서 일반 웹뷰 `hostActive=false`, 실제 visualViewport와 canvas CSS 크기 일치, 390×664/454×768·DPR 1.5/2의 제목/보드/하단 버튼을 확인한다. 명시적 Toss preview inset은 별도로 확인한다.
3. **실제 아이폰 카카오톡/Safari**에서 첫 탭 이후 타이핑/버튼/착수음, 로비↔대국 BGM, 배경 복귀를 재생한다. `Loading FSB failed for TypingKey` 및 `EncodingError: Decoding failed` 재발 여부를 확인한다. 이 기기 검증은 아직 미실행이며 해결 확정으로 기록하지 않는다.

로컬 file:// 검사 페이지는 도구 URL 정책으로 열리지 않았고 우회하지 않았다. 총괄이 독립적으로 제공한 Windows Chromium 디코드 증거는 `Docs/Management/Evidence/ios-playtest-fix-20261016/chromium-codec-probe.json`이다. 그것도 iOS 검증은 아니다.

## 근거 및 재현

- Unity WebGL은 AAC와 브라우저 decodeAudioData를 사용한다: [Unity 6000.3 Web audio](https://docs.unity3d.com/6000.3/Documentation/Manual/webgl-audio.html), 확인 2026-10-16.
- Windows AAC의 초기 샘플/짧은 클립 문제와 끝 무음만으로 실제 음원이 손상될 수 있다는 Unity 직원 설명: [Loading FSB failed for audio](https://discussions.unity.com/t/loading-fsb-failed-for-audio/796691), 2026-03-10/12 답변, 확인 2026-10-16. 이 설명과 실제 변환 측정을 근거로 앞 보호 무음을 선택했다.
- `prepare_probe.py`/`codec/`는 Unity 설치본 FSBTool64 AAC 비교 실험. `verify_audio.py`는 실제 Unity import artifact를 디코딩하고 원본 보존·attack/energy를 검증한다. FFmpeg는 Git 제외 Library/AudioCodecVerifier의 imageio-ffmpeg 0.6.0 패키지로만 사용했다.
- 재현 도구: `Tools/PrepareWebAudio.py`, `Tools/VerifyWebViewport.cjs`, `RankMobileViewportTests`, `RankSoundAssetTests`, `WebAudioImportAuditTests`. Unity 캡처 출력은 `STAGEGAME_CAPTURE_DIRECTORY=Evidence/web-audio-fsb-20261016/screens`로 지정했다.
