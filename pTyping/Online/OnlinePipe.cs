using System;

namespace pTyping.Online;

public class OnlinePipeMessage {
    /// <summary>
    ///     The target of the message, -1 to send to all
    /// </summary>
    public long Target;
    /// <summary>
    ///     The source of the message
    /// </summary>
    public long Source;
    /// <summary>
    ///     The data of the message
    /// </summary>
    public byte[] Data;
}

public abstract class OnlinePipe {
    public abstract void Connect();
    public abstract void Disconnect();

    public abstract void SendMessage(OnlinePipeMessage message);

    public event EventHandler<OnlinePipeMessage> MessageReceived;
    public event EventHandler                    PipeConnected;
    public event EventHandler                    PipeDisconnected;

    protected void InvokeMessageRecieved(OnlinePipeMessage message) {
        this.MessageReceived?.Invoke(this, message);
    }
    protected void InvokePipeConnected() {
        this.PipeConnected?.Invoke(this, EventArgs.Empty);
    }
    protected void InvokePipeDisconnected() {
        this.PipeDisconnected?.Invoke(this, EventArgs.Empty);
    }
}
