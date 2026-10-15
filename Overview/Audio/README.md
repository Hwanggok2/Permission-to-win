# 대국 BGM: Easy Lemon

**2026-10-16 효과음 호환성 수정:** iPhone 카카오톡 FSB/EncodingError 제보에 대응해 TypingKey와 ButtonClick에 원본을 보존한 AAC 보호 복사본을 연결하고 동일 테스트 주소를 갱신했다. Easy Lemon 원본·음량·라이선스는 유지한다. 공개 주소 대사·재도전 시 게임 오류 로그는 없지만 실제 iPhone 재시험은 대기 중이다. [검증 기록](../../Docs/Management/Evidence/ios-playtest-fix-20261016/README.md).

**2026-10-16 후속 검증:** 사용자 승인으로 최신 WebGL에 포함하여 [외부 테스트 배포](../../Docs/Management/PLAYTEST.md)했다. 실제 공개 주소에서 배경음악 켜짐→꺼짐→켜짐 UI를 확인했다. 사람 청취·음량 체감·루프 경계 검증과 구분한다. 아래 ‘웹 빌드 미실행’은 10/15 당시 상태다.

확인일: **2026-10-15**. 사용자가 **Easy Lemon / Kevin MacLeod**를 선택했다. **게임 소스·저장된 프리팹 연결 완료, 에디터 검사 95/95 통과. 웹 빌드 미실행.**

대국 중 기본 음량 **12%**로 반복 재생한다. 설정에서 효과음과 별도로 배경음악을 켜고 끄며 선택을 저장한다. 로비에서는 중지하고 일시정지·광고·백그라운드 전환 때 잠시 멈춘다. 끈 상태는 복귀 후에도 유지한다. [채택곡과 이전 후보 듣기](index.html).

설정의 기존 **라이선스** 화면에 곡명·저작자·원본 링크·CC BY 4.0 링크·Unity 압축 및 낮은 음량 반복 사용을 고지한다. MP3 원본은 변경하지 않았다. [채택 자산 기록](../../Licence/EASY-LEMON.json).

아래는 선정 당시의 후보 기록이다. 런타임에는 Easy Lemon 한 곡만 포함한다.

| 분위기 | 곡 / 공식 청취 페이지 | 작곡가 정보 | 사용 후보 |
| --- | --- | --- | --- |
| 평화로운 | [Easy Lemon](https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1200076) | 82 BPM, 2:06, 기타·마림바 | 기본 대국 1순위 |
| 평화롭고 밝은 | [Carefree](https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1400037) | 96 BPM, 3:25, 우쿨렐레·기타 | 로비 또는 밝은 대국 |
| 신나는 | [Bit Quest](https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1500073) | 100 BPM, 3:12, 8비트 신스·타악기 | 도트 스타일과 비교 |
| 잔잔한 | [Dreams Become Real](https://incompetech.com/music/royalty-free/index.html?isrc=USUAN1500027) | 60 BPM, 6:50, 피아노·신스, 차분하고 무게 있는 분위기 | 조용한 대국 대안 |

## 상업 이용 조건

네 곡 모두 Kevin MacLeod의 공식 개별 곡 페이지에서 **CC BY 4.0**으로 배포하는 음원이다. 해당 라이선스는 조건 준수 시 영리적 복제·배포·변경을 허용하므로, 광고와 광고 제거 IAP가 있는 게임의 BGM 후보로 사용할 수 있다. 무료 사용은 저작권이 없다는 뜻이 아니다. [CC 공식 요약](https://creativecommons.org/licenses/by/4.0/deed.ko), [이용허락 전문](https://creativecommons.org/licenses/by/4.0/legalcode.en), [작곡가 라이선스 안내](https://incompetech.com/music/royalty-free/licenses/)

- 게임 **설정 → 라이선스 → 음악**에 실제 사용하는 곡명, Kevin MacLeod, 원본 링크, CC BY 4.0 링크와 적용한 편집 내역을 표시한다. 작곡가 FAQ가 게임 설정의 크레딧 화면을 안내한다. [공식 FAQ](https://incompetech.com/music/royalty-free/faq.html)
- 원본 MP3를 직접 편집하지 않았고, Unity 임포트 압축과 낮은 음량의 반복 재생을 변경·사용 내역에 기록했다. 추후 루프 편집·페이드를 추가하면 고지도 갱신한다. 음악에 CC 허용 범위를 제한하는 별도 약관이나 DRM을 적용하지 않는다.
- 실제 채택 곡만 런타임 자산과 배포 고지에 포함한다. 출처를 표시할 수 없는 별도 활용은 무료 CC 조건으로 처리하지 않는다.
- [ATTRIBUTION.txt](ATTRIBUTION.txt)는 청취 페이지의 네 곡 고지다. 게임에는 Easy Lemon 전용 고지를 넣는다. 원본 출처·ISRC·다운로드 해시는 [candidates.json](candidates.json), 당시 공식 문서 사본은 `evidence/`에 둔다.

## 확인 범위

1. Excel Strings 423~424행에 한국어·영어 BGM 설정 문구를 추가하고 기존 셀·서식 보존 및 JSON 일치를 확인했다.
2. Unity의 저장된 별도 AudioSource와 설정 프리팹을 사용한다. 착수 효과음 설정은 유지한다.
3. 사람의 반복 청취, 루프 경계, 실제 기기 음량은 다음 플레이 테스트에서 확인한다. 무봉제 루프로 편집한 음원은 아니다.
4. WebGL의 최초 사용자 입력 후 오디오 시작과 실제 광고 SDK 중 음소거는 승인 후 플랫폼 테스트 대상이다.

에디터 검사 **95/95 통과**, 실패·건너뜀 0건. 이 중 음악 전용 6개 검사는 기존 저장 기본값·ON/OFF 복원, 별도 AudioSource/원본 해시/라이선스, 실제 AudioSource의 재생·정지·일시정지, 광고 실패·타임아웃·늦은 콜백을 확인했다. [검사 결과](../../Client/Evidence/easy-lemon-tests.xml).

1080×1920 한국어와 320×568 영어 에디터 캡처에서 음악 버튼·언어·효과음·라이선스 항목이 겹치지 않는 것을 확인했다. [한국어 설정](../../Client/Evidence/easy-lemon-20261015/ko-1080-music-on.png) · [영어 설정](../../Client/Evidence/easy-lemon-20261015/en-320-music-off.png).

원본 MP3와 게임 MP3의 SHA-256 일치, 런타임 MP3는 Easy Lemon 한 곡, 표시용 LegalNotices의 곡 고지 포함을 별도로 대조했다. Excel의 기존 셀·서식은 그대로이고 JSON 두 복사본도 일치한다. [Excel 검증 기록](evidence/integration-20261015/workbook-validation.json).

새 WebGL/Android 빌드·실제 광고 SDK·실기기·사람 청취는 하지 않았다. 에디터 검사와 캡처를 이 검증들의 증거로 간주하지 않는다.
