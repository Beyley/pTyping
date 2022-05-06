namespace pTyping.Engine;

public static partial class AVFormat {

    /// <summary>
    /// </summary>
    public enum CodecId {
        /// <summary>
        /// </summary>
        CodecIdNone,
        /// <summary>
        /// </summary>
        CodecIdMpeg1Video,
        /// <summary>
        /// </summary>
        CodecIdMpeg2Video,/* prefered ID for MPEG Video 1 or 2 decoding */
        /// <summary>
        /// </summary>
        CodecIdMpeg2VideoXvmc,
        /// <summary>
        /// </summary>
        CodecIdH261,
        /// <summary>
        /// </summary>
        CodecIdH263,
        /// <summary>
        /// </summary>
        CodecIdRv10,
        /// <summary>
        /// </summary>
        CodecIdRv20,
        /// <summary>
        /// </summary>
        CodecIdMjpeg,
        /// <summary>
        /// </summary>
        CodecIdMjpegb,
        /// <summary>
        /// </summary>
        CodecIdLjpeg,
        /// <summary>
        /// </summary>
        CodecIdSp5X,
        /// <summary>
        /// </summary>
        CodecIdJpegls,
        /// <summary>
        /// </summary>
        CodecIdMpeg4,
        /// <summary>
        /// </summary>
        CodecIdRawvideo,
        /// <summary>
        /// </summary>
        CodecIdMsmpeg4V1,
        /// <summary>
        /// </summary>
        CodecIdMsmpeg4V2,
        /// <summary>
        /// </summary>
        CodecIdMsmpeg4V3,
        /// <summary>
        /// </summary>
        CodecIdWmv1,
        /// <summary>
        ///     /
        /// </summary>
        CodecIdWmv2,
        /// <summary>
        /// </summary>
        CodecIdH263P,
        /// <summary>
        /// </summary>
        CodecIdH263I,
        /// <summary>
        /// </summary>
        CodecIdFlv1,
        /// <summary>
        /// </summary>
        CodecIdSvq1,
        /// <summary>
        /// </summary>
        CodecIdSvq3,
        /// <summary>
        /// </summary>
        CodecIdDvvideo,
        /// <summary>
        /// </summary>
        CodecIdHuffyuv,
        /// <summary>
        /// </summary>
        CodecIdCyuv,
        /// <summary>
        /// </summary>
        CodecIdH264,
        /// <summary>
        /// </summary>
        CodecIdIndeo3,
        /// <summary>
        /// </summary>
        CodecIdVp3,
        /// <summary>
        /// </summary>
        CodecIdTheora,
        /// <summary>
        /// </summary>
        CodecIdAsv1,
        /// <summary>
        /// </summary>
        CodecIdAsv2,
        /// <summary>
        /// </summary>
        CodecIdFfv1,
        /// <summary>
        /// </summary>
        CodecId4Xm,
        /// <summary>
        /// </summary>
        CodecIdVcr1,
        /// <summary>
        /// </summary>
        CodecIdCljr,
        /// <summary>
        /// </summary>
        CodecIdMdec,
        /// <summary>
        /// </summary>
        CodecIdRoq,
        /// <summary>
        /// </summary>
        CodecIdInterplayVideo,
        /// <summary>
        /// </summary>
        CodecIdXanWc3,
        /// <summary>
        /// </summary>
        CodecIdXanWc4,
        /// <summary>
        /// </summary>
        CodecIdRpza,
        /// <summary>
        /// </summary>
        CodecIdCinepak,
        /// <summary>
        /// </summary>
        CodecIdWsVqa,
        /// <summary>
        /// </summary>
        CodecIdMsrle,
        /// <summary>
        /// </summary>
        CodecIdMsvideo1,
        /// <summary>
        /// </summary>
        CodecIdIdcin,
        /// <summary>
        /// </summary>
        CodecId8Bps,
        /// <summary>
        /// </summary>
        CodecIdSmc,
        /// <summary>
        /// </summary>
        CodecIdFlic,
        /// <summary>
        /// </summary>
        CodecIdTruemotion1,
        /// <summary>
        /// </summary>
        CodecIdVmdvideo,
        /// <summary>
        /// </summary>
        CodecIdMszh,
        /// <summary>
        /// </summary>
        CodecIdZlib,
        /// <summary>
        /// </summary>
        CodecIdQtrle,
        /// <summary>
        /// </summary>
        CodecIdSnow,
        /// <summary>
        /// </summary>
        CodecIdTscc,
        /// <summary>
        /// </summary>
        CodecIdUlti,
        /// <summary>
        /// </summary>
        CodecIdQdraw,
        /// <summary>
        /// </summary>
        CodecIdVixl,
        /// <summary>
        /// </summary>
        CodecIdQpeg,
        /// <summary>
        /// </summary>
        CodecIdXvid,
        /// <summary>
        /// </summary>
        CodecIdPng,
        /// <summary>
        /// </summary>
        CodecIdPpm,
        /// <summary>
        /// </summary>
        CodecIdPbm,
        /// <summary>
        /// </summary>
        CodecIdPgm,
        /// <summary>
        /// </summary>
        CodecIdPgmyuv,
        /// <summary>
        /// </summary>
        CodecIdPam,
        /// <summary>
        /// </summary>
        CodecIdFfvhuff,
        /// <summary>
        /// </summary>
        CodecIdRv30,
        /// <summary>
        /// </summary>
        CodecIdRv40,
        /// <summary>
        /// </summary>
        CodecIdVc1,
        /// <summary>
        /// </summary>
        CodecIdWmv3,
        /// <summary>
        /// </summary>
        CodecIdLoco,
        /// <summary>
        /// </summary>
        CodecIdWnv1,
        /// <summary>
        /// </summary>
        CodecIdAasc,
        /// <summary>
        /// </summary>
        CodecIdIndeo2,
        /// <summary>
        /// </summary>
        CodecIdFraps,
        /// <summary>
        /// </summary>
        CodecIdTruemotion2,
        /// <summary>
        /// </summary>
        CodecIdBmp,
        /// <summary>
        /// </summary>
        CodecIdCscd,
        /// <summary>
        /// </summary>
        CodecIdMmvideo,
        /// <summary>
        /// </summary>
        CodecIdZmbv,
        /// <summary>
        /// </summary>
        CodecIdAvs,
        /// <summary>
        /// </summary>
        CodecIdSmackvideo,
        /// <summary>
        /// </summary>
        CodecIdNuv,
        /// <summary>
        /// </summary>
        CodecIdKmvc,
        /// <summary>
        /// </summary>
        CodecIdFlashsv,
        /// <summary>
        /// </summary>
        CodecIdCavs,
        /// <summary>
        /// </summary>
        CodecIdJpeg2000,
        /// <summary>
        ///     /
        /// </summary>
        CodecIdVmnc,
        /// <summary>
        /// </summary>
        CodecIdVp5,
        /// <summary>
        /// </summary>
        CodecIdVp6,
        /// <summary>
        /// </summary>
        CodecIdVp6F,

        /* various pcm "codecs" */
        /// <summary>
        /// </summary>
        CodecIdPcmS16Le = 0x10000,
        /// <summary>
        /// </summary>
        CodecIdPcmS16Be,
        /// <summary>
        /// </summary>
        CodecIdPcmU16Le,
        /// <summary>
        /// </summary>
        CodecIdPcmU16Be,
        /// <summary>
        /// </summary>
        CodecIdPcmS8,
        /// <summary>
        /// </summary>
        CodecIdPcmU8,
        /// <summary>
        /// </summary>
        CodecIdPcmMulaw,
        /// <summary>
        /// </summary>
        CodecIdPcmAlaw,
        /// <summary>
        /// </summary>
        CodecIdPcmS32Le,
        /// <summary>
        /// </summary>
        CodecIdPcmS32Be,
        /// <summary>
        /// </summary>
        CodecIdPcmU32Le,
        /// <summary>
        /// </summary>
        CodecIdPcmU32Be,
        /// <summary>
        /// </summary>
        CodecIdPcmS24Le,
        /// <summary>
        /// </summary>
        CodecIdPcmS24Be,
        /// <summary>
        /// </summary>
        CodecIdPcmU24Le,
        /// <summary>
        /// </summary>
        CodecIdPcmU24Be,
        /// <summary>
        /// </summary>
        CodecIdPcmS24Daud,

        /* various adpcm codecs */
        /// <summary>
        /// </summary>
        CodecIdAdpcmImaQt = 0x11000,
        /// <summary>
        /// </summary>
        CodecIdAdpcmImaWav,
        /// <summary>
        /// </summary>
        CodecIdAdpcmImaDk3,
        /// <summary>
        /// </summary>
        CodecIdAdpcmImaDk4,
        /// <summary>
        /// </summary>
        CodecIdAdpcmImaWs,
        /// <summary>
        /// </summary>
        CodecIdAdpcmImaSmjpeg,
        /// <summary>
        /// </summary>
        CodecIdAdpcmMs,
        /// <summary>
        /// </summary>
        CodecIdAdpcm4Xm,
        /// <summary>
        /// </summary>
        CodecIdAdpcmXa,
        /// <summary>
        /// </summary>
        CodecIdAdpcmAdx,
        /// <summary>
        /// </summary>
        CodecIdAdpcmEa,
        /// <summary>
        /// </summary>
        CodecIdAdpcmG726,
        /// <summary>
        /// </summary>
        CodecIdAdpcmCt,
        /// <summary>
        /// </summary>
        CodecIdAdpcmSwf,
        /// <summary>
        /// </summary>
        CodecIdAdpcmYamaha,
        /// <summary>
        /// </summary>
        CodecIdAdpcmSbpro4,
        /// <summary>
        /// </summary>
        CodecIdAdpcmSbpro3,
        /// <summary>
        /// </summary>
        CodecIdAdpcmSbpro2,

        /* AMR */
        /// <summary>
        /// </summary>
        CodecIdAmrNb = 0x12000,
        /// <summary>
        /// </summary>
        CodecIdAmrWb,

        /* RealAudio codecs*/
        /// <summary>
        /// </summary>
        CodecIdRa144 = 0x13000,
        /// <summary>
        /// </summary>
        CodecIdRa288,

        /* various DPCM codecs */
        /// <summary>
        /// </summary>
        CodecIdRoqDpcm = 0x14000,
        /// <summary>
        /// </summary>
        CodecIdInterplayDpcm,
        /// <summary>
        /// </summary>
        CodecIdXanDpcm,
        /// <summary>
        /// </summary>
        CodecIdSolDpcm,

        /// <summary>
        /// </summary>
        CodecIdMp2 = 0x15000,
        /// <summary>
        /// </summary>
        CodecIdMp3,/* prefered ID for MPEG Audio layer 1, 2 or3 decoding */
        /// <summary>
        /// </summary>
        CodecIdAac,
        /// <summary>
        /// </summary>
        CodecIdMpeg4Aac,
        /// <summary>
        /// </summary>
        CodecIdAc3,
        /// <summary>
        /// </summary>
        CodecIdDts,
        /// <summary>
        /// </summary>
        CodecIdVorbis,
        /// <summary>
        /// </summary>
        CodecIdDvaudio,
        /// <summary>
        /// </summary>
        CodecIdWmav1,
        /// <summary>
        /// </summary>
        CodecIdWmav2,
        /// <summary>
        /// </summary>
        CodecIdMace3,
        /// <summary>
        /// </summary>
        CodecIdMace6,
        /// <summary>
        /// </summary>
        CodecIdVmdaudio,
        /// <summary>
        /// </summary>
        CodecIdSonic,
        /// <summary>
        /// </summary>
        CodecIdSonicLs,
        /// <summary>
        /// </summary>
        CodecIdFlac,
        /// <summary>
        /// </summary>
        CodecIdMp3Adu,
        /// <summary>
        /// </summary>
        CodecIdMp3On4,
        /// <summary>
        /// </summary>
        CodecIdShorten,
        /// <summary>
        /// </summary>
        CodecIdAlac,
        /// <summary>
        /// </summary>
        CodecIdWestwoodSnd1,
        /// <summary>
        /// </summary>
        CodecIdGsm,
        /// <summary>
        /// </summary>
        CodecIdQdm2,
        /// <summary>
        /// </summary>
        CodecIdCook,
        /// <summary>
        /// </summary>
        CodecIdTruespeech,
        /// <summary>
        /// </summary>
        CodecIdTta,
        /// <summary>
        /// </summary>
        CodecIdSmackaudio,
        /// <summary>
        /// </summary>
        CodecIdQcelp,

        /* subtitle codecs */
        /// <summary>
        /// </summary>
        CodecIdDvdSubtitle = 0x17000,
        /// <summary>
        /// </summary>
        CodecIdDvbSubtitle,

        /// <summary>
        /// </summary>
        CodecIdMpeg2Ts = 0x20000
        /* _FAKE_ codec to indicate a raw MPEG2 transport
stream (only used by libavformat) */
    }

}
