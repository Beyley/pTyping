using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;

namespace pTyping.Engine;

public static partial class AVFormat {
    public const int MAX_STREAMS = 20;

    [DllImport("libavformat")]
    public static extern int av_open_input_file(
        [Out] out IntPtr formatContext, [MarshalAs(UnmanagedType.LPStr)] string filename, IntPtr avInputFormat, int bufferSize, IntPtr avFormatParameters
    );

    [DllImport("libavformat")]
    public static extern int av_find_stream_info(IntPtr avFormatContext);

    [DllImport("libavformat")]
    public static extern void dump_format(IntPtr avFormatContext, int index, [MarshalAs(UnmanagedType.LPWStr)] string url, int isOutput);

    /// <summary>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct AVFormatContextA {
        /// <summary>
        /// </summary>
        public IntPtr pAVClass;// set by av_alloc_format_context

        /// <summary>
        /// </summary>
        public IntPtr pAVInputFormat;// can only be iformat or oformat, not both at the same time

        /// <summary>
        /// </summary>
        public IntPtr pAVOutputFormat;

        //[MarshalAs(UnmanagedType.FunctionPtr)]
        //AnonymousCallback priv_data;

        /// <summary>
        /// </summary>
        public IntPtr priv_data;

        /// <summary>
        /// </summary>
        // #if AVFORMAT_VERSION_52
        public IntPtr pb;// ByteIOContext
        // #else
        // public ByteIOContext pb;
        // #endif

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int nb_streams;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_STREAMS)] public IntPtr[] streams;// AVStream

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)] public byte[] filename;// input or output filename

        /* stream info */

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I8)] public long timestamp;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] title;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] author;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] copyright;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] comment;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] album;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int year;// ID3 year, 0 if none

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int tract;// track number, 0 if none

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] genre;// ID3 genre

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int ctx_flags;// format specific flags, see AVFMTCTX_xx

        /* This buffer is only needed when packets were already buffered but
        not decoded, for example to get the codec parameters in mpeg
        streams */

        /// <summary>
        /// </summary>
        public IntPtr packet_buffer;// AVPacketList

        /* decoding: position of the first frame of the component, in
        AV_TIME_BASE fractional seconds. NEVER set this value directly:
        it is deduced from the AVStream values. */

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I8)] public long start_time;

        /* decoding: duration of the stream, in AV_TIME_BASE fractional
        seconds. NEVER set this value directly: it is deduced from the
        AVStream values. */

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I8)] public long duration;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I8)] public long file_size;// decoding: total file size. 0 if unknown

        /* decoding: total stream bitrate in bit/s, 0 if not
        available. Never set it directly if the file_size and the
        duration are known as ffmpeg can compute it automatically. */

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int bit_rate;

        /* av_read_frame() support */

        /// <summary>
        /// </summary>
        public IntPtr cur_st;// AVStream

        /// <summary>
        /// </summary>
        public IntPtr cur_ptr;// uint8_t

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int cur_len;

        /// <summary>
        /// </summary>
        public AVPacket cur_pkt;// AVPacket

        /* av_seek_frame() support */

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I8)] public long data_offset;// offset of the first packet

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int index_built;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int mux_rate;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int packet_size;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int preload;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int max_delay;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int loop_output;

        // number of times to loop output in formats that support it

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int flags;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int loop_input;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public uint probesize;

        // decoding: size of data to probe; encoding unused

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int max_analyze_duration;

        /// <summary>
        /// </summary>
        public IntPtr key;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.I4)] public int keylen;

        /// <summary>
        /// </summary>
        [MarshalAs(UnmanagedType.U4)] public uint nb_programs;

        /// <summary>
        /// </summary>
        public IntPtr programs;

        /// <summary>
        /// </summary>
        public CodecId video_codec_id;

        /// <summary>
        /// </summary>
        public CodecId audio_codec_id;

        /// <summary>
        /// </summary>
        public CodecId subtitle_codec_id;
    }
}
