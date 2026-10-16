# 이겨도 됩니까? — 글꼴 적용

2026-10-16 사용자 요청으로 **Pretendard**를 제목과 기본 UI 글꼴로 적용했다. 로비·대국·버튼·설정·도움말·라이선스 화면의 저장된 프리팹에 연결했고, 웹 템플릿의 로딩 제목과 안내에도 로컬 WOFF2를 사용한다. 기존 글자 크기·굵기·색·배치는 유지했다.

**NeoDunggeunmo Pro**는 도트 글꼴이 필요한 연출에 사용할 수 있도록 프로젝트에 추가했다. 현재 UI에는 Pretendard를 사용하며, 네오둥근모를 임의로 혼용하지 않았다. Noto Sans CJK KR는 특수문자용 대체 글꼴로 보존했다. 현재 문자열 데이터의 문자 464종은 Pretendard 자체에서 모두 지원한다.

SimpleGame은 읽기만 했다. `Assets/Font`에는 두 글꼴의 TMP SDF 변환본만 있고 원본 TTF/OTF가 없어, 현재 `UnityEngine.UI.Text`에 맞는 공식 원본을 사용했다. Pretendard는 v1.3.9 배포 ZIP의 Regular TTF·Regular/Bold WOFF2, 네오둥근모는 v1.020 TTF다. 변환·서브셋·글꼴 파일 수정은 하지 않았다.

두 글꼴은 SIL Open Font License 1.1로 배포된다. 배포 버전에 대응하는 전문을 `Licence/ThirdParty`와 `Client/Assets/StreamingAssets/Notices`에 보존하고, 웹 글꼴 폴더에도 Pretendard 고지를 동봉했다. 출처·버전·파일별 SHA-256·사용 범위는 [폰트 대장](../../Licence/FONTS-20261016.json)과 [자산 대장](../../Licence/ASSET-REGISTER.csv)에 기록했다. 공식 출처: [Pretendard](https://github.com/orioncactus/pretendard/releases/tag/v1.3.9), [NeoDunggeunmo Pro](https://github.com/neodgm/neodgm-pro/releases/tag/v1.020).

게임 내 설정의 라이선스 화면에는 각 폰트의 저작권·예약명 고지를 모두 표시하고, 공통 OFL 약관 전문은 한 번 표시한다. 원문을 통째로 반복하면 기존 uGUI Text의 65,000 정점 제한을 넘는 문제가 확인돼 중복만 합쳤다. 약관이 공백을 제외하고 동일한지 확인한 후에만 합치며, 개별 배포 고지는 원문 그대로 유지한다. Unity와 Python 고지 생성 결과도 일치한다.

검증: 기존 프리팹 31개 중 24개에서 글꼴 참조 81개만 변경됐으며 다른 직렬화 속성 변화는 없다. 재적용은 변경 0개. 최종 Unity 12/12 통과, 한국어·영어 1080·390·320px 화면 78장 캡처. 로비·대국·도움말·고지창 표본을 시각 확인했다. [검증 기록](../../Client/Evidence/fonts-20261016/verification.json), [한국어 로비](../../Client/Evidence/fonts-20261016/screens-final/ko-390-lobby.png).

Unity 소스·저장 프리팹·웹 템플릿 적용과 에디터 검증 후, 2026-10-16 사용자 요청으로 WebGL 빌드와 공개 배포까지 완료했다. 사용자 푸시 `3d6c2a9`, Pages 성공·공개 파일23/23 일치·공개 브라우저 검증을 확인했다. [배포 기록](../Management/Evidence/title-font-web-20261016/README.md). 새 APK와 실제 기기 검증은 실행하지 않았다.
