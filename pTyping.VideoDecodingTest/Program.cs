using FFmpeg.AutoGen;
using Silk.NET.GLFW;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static FFmpeg.AutoGen.ffmpeg;

namespace pTyping.VideoDecodingTest;

public class Program {
    private const string vert_shader_source = @"#version 150
in vec3 vertex;
in vec2 texCoord0;
uniform mat4 mvpMatrix;
out vec2 texCoord;
void main() {
	gl_Position = mvpMatrix * vec4(vertex, 1.0);
	texCoord = texCoord0;
}
";

    private const string frag_shader_source = @"#version 150
uniform sampler2D frameTex;
in vec2 texCoord;
out vec4 fragColor;
void main() {
	fragColor = texture(frameTex, texCoord);
}
";

    // #define BUFFER_OFFSET(i) ((char *)null + (i))

    public static unsafe void* BUFFER_OFFSET(int v) => (void*)(0 + v);

    // app data structure
    private unsafe struct AppData {
        public AVFormatContext* fmt_ctx;
        public int              stream_idx;
        public AVStream*        video_stream;
        public AVCodecContext*  codec_ctx;
        public AVCodec*         decoder;
        public AVPacket*        packet;
        public AVFrame*         av_frame;
        public AVFrame*         gl_frame;

        public       SwsContext* conv_ctx;
        public       uint        vao;
        public       uint        vert_buf;
        public       uint        elem_buf;
        public       uint        frame_tex;
        public       uint        program;
        public fixed uint        attribs[2];
        public fixed uint        uniforms[2];

        public AppData() {
            this.fmt_ctx      = null;
            this.stream_idx   = -1;
            this.video_stream = null;
            this.codec_ctx    = null;
            this.decoder      = null;
            this.packet       = null;
            this.av_frame     = null;
            this.gl_frame     = null;
            this.conv_ctx     = null;
            this.vao          = 0;
            this.vert_buf     = 0;
            this.elem_buf     = 0;
            this.frame_tex    = 0;
            this.program      = 0;
        }
    }

    private static        GL            gl;
    private static        Glfw          glfw;
    private static unsafe WindowHandle* window;

    // clean up the app data structure
    private static unsafe void clearAppData(AppData* data) {
        if (data->av_frame  != null) av_free(data->av_frame);
        if (data->gl_frame  != null) av_free(data->gl_frame);
        if (data->packet    != null) av_free(data->packet);
        if (data->codec_ctx != null) avcodec_close(data->codec_ctx);
        if (data->fmt_ctx   != null) avformat_free_context(data->fmt_ctx);
        gl.DeleteVertexArrays(1, &data->vao);
        gl.DeleteBuffers(1, &data->vert_buf);
        gl.DeleteBuffers(1, &data->elem_buf);
        gl.DeleteTextures(1, &data->frame_tex);
    }

    public static unsafe int Main(string[] args) {
        Console.WriteLine("Av version info: " + av_version_info());

        if (args.Length < 1) {
            Console.WriteLine("You need to provide a filename!");
            return -1;
        }

        avformat_network_init();

        AppData data = new();

        // open video
        if (avformat_open_input(&data.fmt_ctx, args[0], null, null) < 0) {
            Console.WriteLine("failed to open input");
            return -1;
            // clearAppData(&data);
        }

        // find stream info
        if (avformat_find_stream_info(data.fmt_ctx, null) < 0) {
            Console.WriteLine("failed to get stream info");
            return -1;
            // clearAppData(&data);
        }

        // dump debug info
        av_dump_format(data.fmt_ctx, 0, args[0], 0);

        Console.WriteLine($"Amount of streams: {data.fmt_ctx->nb_streams}");

        for (int i = 0; i < data.fmt_ctx->nb_streams; ++i) {
            Console.WriteLine($"stream: {data.fmt_ctx->streams[i]->codecpar->codec_type} i:{i}");
            if (data.fmt_ctx->streams[i]->codecpar->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO) {
                Console.WriteLine($"found stream: {data.fmt_ctx->streams[i]->codecpar->codec_type} i:{i}");

                data.stream_idx = i;
                break;
            }
        }

        if (data.stream_idx == -1) {
            Console.WriteLine("failed to find video stream");
            clearAppData(&data);
            return -1;
        }

        data.video_stream = data.fmt_ctx->streams[data.stream_idx];
        data.codec_ctx    = avcodec_alloc_context3(null);
        avcodec_parameters_to_context(data.codec_ctx, data.video_stream->codecpar);

        // find the decoder
        data.decoder = avcodec_find_decoder(data.codec_ctx->codec_id);
        if (data.decoder == null) {
            Console.WriteLine("failed to find decoder");
            clearAppData(&data);
            return -1;
        }

        // open the decoder
        if (avcodec_open2(data.codec_ctx, data.decoder, null) < 0) {
            Console.WriteLine("failed to open codec");
            clearAppData(&data);
            return -1;
        }

        // allocate the video frames
        data.av_frame = av_frame_alloc();
        data.gl_frame = av_frame_alloc();
        int   size            = av_image_get_buffer_size(AVPixelFormat.AV_PIX_FMT_RGB24, data.codec_ctx->width, data.codec_ctx->height, 1);
        byte* internal_buffer = (byte*)av_malloc((ulong)(size * sizeof(byte)));

        byte_ptrArray4 data_array4     = new();
        int_array4     lineSize_array4 = new();
        data_array4.UpdateFrom(data.gl_frame->data);
        lineSize_array4.UpdateFrom(data.gl_frame->linesize);

        av_image_fill_arrays(ref data_array4, ref lineSize_array4, internal_buffer, AVPixelFormat.AV_PIX_FMT_RGB24, data.codec_ctx->width, data.codec_ctx->height, 1);
        data.gl_frame->data.UpdateFrom(data_array4);
        data.gl_frame->linesize.UpdateFrom(lineSize_array4);
        data.packet = av_packet_alloc();

        if (data.packet == null) {
            Console.WriteLine("Alloccing packet failed");
            return -1;
        }

        glfw = Glfw.GetApi();

        // initialize glfw
        if (!glfw.Init()) {
            Console.WriteLine("glfw failed to init");
            glfw.Terminate();
            clearAppData(&data);
            return -1;
        }

        // open a window
        float aspect     = data.codec_ctx->width / (float)data.codec_ctx->height;
        int   adj_width  = (int)(aspect * 300);
        int   adj_height = 300;
        glfw.WindowHint(WindowHintInt.ContextVersionMajor,     3);
        glfw.WindowHint(WindowHintInt.ContextVersionMinor,     2);
        glfw.WindowHint(WindowHintBool.OpenGLForwardCompat,    true);
        glfw.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
        window = glfw.CreateWindow(adj_width, adj_height, "Title", null, null);
        if (window == null) {
            Console.WriteLine("failed to open window");
            glfw.Terminate();
            clearAppData(&data);
            return -1;
        }

        glfw.MakeContextCurrent(window);

        gl = GL.GetApi(s => glfw.GetProcAddress(s));

        // initialize opengl
        gl.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        gl.Enable(EnableCap.Texture2D);

        // initialize shaders
        if (!buildProgram(&data)) {
            Console.WriteLine("failed to initialize shaders");
            glfw.Terminate();
            clearAppData(&data);
            return -1;
        }
        gl.UseProgram(data.program);

        // initialize renderable
        gl.GenVertexArrays(1, &data.vao);
        gl.BindVertexArray(data.vao);

        gl.GenBuffers(1, &data.vert_buf);
        gl.BindBuffer(GLEnum.ArrayBuffer, data.vert_buf);
        float[] quad = {
            -1.0f, 1.0f, 0.0f, 0.0f, 0.0f, -1.0f, -1.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f, 0.0f, 1.0f, 0.0f
        };

        gl.BufferData<float>(GLEnum.ArrayBuffer, quad, GLEnum.StaticDraw);
        gl.VertexAttribPointer(data.attribs[VERTICES], 3, GLEnum.Float, false, 20, BUFFER_OFFSET(0));
        gl.EnableVertexAttribArray(data.attribs[VERTICES]);
        gl.VertexAttribPointer(data.attribs[TEX_COORDS], 2, GLEnum.Float, false, 20, BUFFER_OFFSET(12));
        gl.EnableVertexAttribArray(data.attribs[TEX_COORDS]);
        gl.GenBuffers(1, &data.elem_buf);
        gl.BindBuffer(GLEnum.ElementArrayBuffer, data.elem_buf);
        byte[] elem = {
            0, 1, 2, 0, 2, 3
        };
        gl.BufferData<byte>(GLEnum.ElementArrayBuffer, (nuint)elem.Length, elem, GLEnum.StaticDraw);
        gl.BindVertexArray(0);

        gl.ActiveTexture(TextureUnit.Texture0);
        gl.GenTextures(1, &data.frame_tex);
        gl.BindTexture(TextureTarget.Texture2D, data.frame_tex);
        gl.PixelStore(GLEnum.UnpackAlignment, 1);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS,                   (int)GLEnum.Repeat);
        gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT,                   (int)GLEnum.Repeat);
        gl.TexParameter(GLEnum.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(GLEnum.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexImage2D(
        TextureTarget.Texture2D,
        0,
        InternalFormat.Rgb,
        (uint)data.codec_ctx->width,
        (uint)data.codec_ctx->height,
        0,
        PixelFormat.Rgb,
        PixelType.UnsignedByte,
        null
        );
        gl.Uniform1((int)data.uniforms[FRAME_TEX], 0);

        Matrix4X4<float> mvp = Matrix4X4.CreateOrthographicOffCenter(-1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f);
        gl.UniformMatrix4((int)data.uniforms[MVP_MATRIX], 1, false, (float*)&mvp);

        bool running = true;

        // run the application mainloop
        while (readFrame(&data) && running) {
            running = glfw.GetKey(window, Keys.Escape) == 0 && glfw.GetWindowAttrib(window, WindowAttributeGetter.Focused);
            drawFrame(&data);

            int width, height;
            glfw.GetWindowSize(window, out width, out height);

            gl.Viewport(0, 0, (uint)width, (uint)height);
        }

        Console.WriteLine("Cleaning up!");

        avformat_close_input(&data.fmt_ctx);

        // clean up
        clearAppData(&data);
        glfw.Terminate();

        return 0;
    }

    // draw frame in opengl context
    private static unsafe void drawFrame(AppData* data) {
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.BindTexture(TextureTarget.Texture2D, data->frame_tex);
        gl.BindVertexArray(data->vao);
        gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedByte, BUFFER_OFFSET(0));
        gl.BindVertexArray(0);
        glfw.SwapBuffers(window);
    }

    private static unsafe bool buildShader(string shader_source, out uint shader, GLEnum type) {
        int size = shader_source.Length;

        shader = gl.CreateShader(type);

        gl.ShaderSource(shader, shader_source);
        gl.CompileShader(shader);
        int status;
        gl.GetShader(shader, ShaderParameterName.CompileStatus, &status);
        if (status != 1) {
            string log = gl.GetShaderInfoLog(shader);
            Console.WriteLine($"Failed to compile shader: {log}");
            return false;
        }

        return true;
    }

    // initialize shaders
    private static unsafe bool buildProgram(AppData* data) {
        uint v_shader, f_shader;
        if (!buildShader(vert_shader_source, out v_shader, GLEnum.VertexShader)) {
            Console.WriteLine("failed to build vertex shader");
            return false;
        }

        if (!buildShader(frag_shader_source, out f_shader, GLEnum.FragmentShader)) {
            Console.WriteLine("failed to build fragment shader");
            return false;
        }

        data->program = gl.CreateProgram();

        gl.AttachShader(data->program, v_shader);
        gl.AttachShader(data->program, f_shader);
        gl.LinkProgram(data->program);
        int status;
        gl.GetProgram(data->program, GLEnum.LinkStatus, &status);
        if (status != 1) {
            Console.WriteLine($"failed to link program: {gl.GetProgramInfoLog(data->program)}");
            return false;
        }

        data->uniforms[MVP_MATRIX] = (uint)gl.GetUniformLocation(data->program, "mvpMatrix");
        data->uniforms[FRAME_TEX]  = (uint)gl.GetUniformLocation(data->program, "frameTex");

        data->attribs[VERTICES]   = (uint)gl.GetAttribLocation(data->program, "vertex");
        data->attribs[TEX_COORDS] = (uint)gl.GetAttribLocation(data->program, "texCoord0");

        return true;
    }

    private static unsafe bool readFrame(AppData* data) {
        do {
            if (av_read_frame(data->fmt_ctx, data->packet) < 0) {
                av_packet_unref(data->packet);
                return false;
            }

            if (data->packet->stream_index == data->stream_idx) {
                bool frame_finished = false;

                // if(avcodec_send_packet(data->codec_ctx, data->packet) < 0) {
                // if (avcodec_decode_video2(data->codec_ctx, data->av_frame, &frame_finished, data->packet) < 0) {
                // av_packet_unref(data->packet);
                // return false;
                // }


                int response;
                if (data->codec_ctx->codec_type == AVMediaType.AVMEDIA_TYPE_VIDEO || data->codec_ctx->codec_type == AVMediaType.AVMEDIA_TYPE_AUDIO) {
                    response = avcodec_send_packet(data->codec_ctx, data->packet);
                    if (response < 0 && response != AVERROR(EAGAIN) && response != AVERROR_EOF) {} else {
                        if (response >= 0)
                            data->packet->size = 0;
                        response = avcodec_receive_frame(data->codec_ctx, data->av_frame);
                        if (response >= 0)
                            frame_finished = true;
                        //if (response == AVERROR(EAGAIN) || response == AVERROR_EOF)
                        //response = 0;
                    }
                }

                if (frame_finished) {
                    if (data->conv_ctx == null)
                        data->conv_ctx = sws_getContext(
                        data->codec_ctx->width,
                        data->codec_ctx->height,
                        data->codec_ctx->pix_fmt,
                        data->codec_ctx->width,
                        data->codec_ctx->height,
                        AVPixelFormat.AV_PIX_FMT_RGB24,
                        SWS_BICUBIC,
                        null,
                        null,
                        null
                        );

                    sws_scale(
                    data->conv_ctx,
                    data->av_frame->data,
                    data->av_frame->linesize,
                    0,
                    data->codec_ctx->height,
                    data->gl_frame->data,
                    data->gl_frame->linesize
                    );

                    gl.TexSubImage2D(
                    TextureTarget.Texture2D,
                    0,
                    0,
                    0,
                    (uint)data->codec_ctx->width,
                    (uint)data->codec_ctx->height,
                    PixelFormat.Rgb,
                    PixelType.UnsignedByte,
                    data->gl_frame->data[0]
                    );
                }
            }

            av_packet_unref(data->packet);
        } while (data->packet->stream_index != data->stream_idx);

        return true;
    }
    private const int VERTICES   = 0;
    private const int TEX_COORDS = 1;
    private const int MVP_MATRIX = 0;
    private const int FRAME_TEX  = 1;
}
