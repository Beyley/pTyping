using System;
using System.Runtime.InteropServices;
using FFmpeg.AutoGen;
using pTyping.Engine;

namespace pTyping.Graphics.Drawables;

public class VideoDrawable {
    private readonly IntPtr          _formatContextPtr;
    private readonly GCHandle        _streamHandle;
    private readonly AVFormatContext _formatContext;
    private readonly int             _videoStream;
    private readonly AVStream        _stream;
    private readonly AVCodecContext  _codecCtx;
    private readonly IntPtr          _codecContextPtr;

    public unsafe VideoDrawable(byte[] bytes) {
        this._streamHandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        IntPtr ptr = this._streamHandle.AddrOfPinnedObject();

        string path = "memory:" + ptr + "|" + bytes.Length;

        if (AVFormat.av_open_input_file(out this._formatContextPtr, path, IntPtr.Zero, bytes.Length, IntPtr.Zero) != 0)
            throw new Exception("Unable to open input file with avformat!");

        if (AVFormat.av_find_stream_info(this._formatContextPtr) < 0)
            throw new Exception("Unable to find stream info");

        AVFormat.dump_format(this._formatContextPtr, 0, path, 0);

        this._formatContext = (AVFormatContext)Marshal.PtrToStructure(this._formatContextPtr, typeof(AVFormatContext))!;

        this._videoStream = -1;
        uint nbStreams = this._formatContext.nb_streams;

        for (int i = 0; i < nbStreams; i++) {
            AVStream avStream = (AVStream)Marshal.PtrToStructure((IntPtr)this._formatContext.streams[i], typeof(AVStream))!;
            //if this line breaks, check priv_data!!! its supposed to be `codec`
            AVCodecContext codec = (AVCodecContext)Marshal.PtrToStructure((IntPtr)avStream.priv_data, typeof(AVCodecContext))!;

            if (codec.codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO) {
                this._videoStream     = i;
                this._stream          = avStream;
                this._codecCtx        = codec;
                this._codecContextPtr = (IntPtr)this._stream.priv_data;
                break;
            }
        }
        if (this._videoStream == -1)
            throw new Exception("couldn't find video stream");
    }
}
