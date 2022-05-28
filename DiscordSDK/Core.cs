using System.Runtime.InteropServices;
using System.Text;

namespace DiscordSDK;

public enum Result {
    Ok                              = 0,
    ServiceUnavailable              = 1,
    InvalidVersion                  = 2,
    LockFailed                      = 3,
    InternalError                   = 4,
    InvalidPayload                  = 5,
    InvalidCommand                  = 6,
    InvalidPermissions              = 7,
    NotFetched                      = 8,
    NotFound                        = 9,
    Conflict                        = 10,
    InvalidSecret                   = 11,
    InvalidJoinSecret               = 12,
    NoEligibleActivity              = 13,
    InvalidInvite                   = 14,
    NotAuthenticated                = 15,
    InvalidAccessToken              = 16,
    ApplicationMismatch             = 17,
    InvalidDataUrl                  = 18,
    InvalidBase64                   = 19,
    NotFiltered                     = 20,
    LobbyFull                       = 21,
    InvalidLobbySecret              = 22,
    InvalidFilename                 = 23,
    InvalidFileSize                 = 24,
    InvalidEntitlement              = 25,
    NotInstalled                    = 26,
    NotRunning                      = 27,
    InsufficientBuffer              = 28,
    PurchaseCanceled                = 29,
    InvalidGuild                    = 30,
    InvalidEvent                    = 31,
    InvalidChannel                  = 32,
    InvalidOrigin                   = 33,
    RateLimited                     = 34,
    OAuth2Error                     = 35,
    SelectChannelTimeout            = 36,
    GetGuildTimeout                 = 37,
    SelectVoiceForceRequired        = 38,
    CaptureShortcutAlreadyListening = 39,
    UnauthorizedForAchievement      = 40,
    InvalidGiftCode                 = 41,
    PurchaseError                   = 42,
    TransactionAborted              = 43,
    DrawingInitFailed               = 44
}

public enum CreateFlags {
    Default          = 0,
    NoRequireDiscord = 1
}

public enum LogLevel {
    Error = 1,
    Warn,
    Info,
    Debug
}

public enum UserFlag {
    Partner         = 2,
    HypeSquadEvents = 4,
    HypeSquadHouse1 = 64,
    HypeSquadHouse2 = 128,
    HypeSquadHouse3 = 256
}

public enum PremiumType {
    None  = 0,
    Tier1 = 1,
    Tier2 = 2
}

public enum ImageType {
    User
}

public enum ActivityPartyPrivacy {
    Private = 0,
    Public  = 1
}

public enum ActivityType {
    Playing,
    Streaming,
    Listening,
    Watching
}

public enum ActivityActionType {
    Join = 1,
    Spectate
}

public enum ActivitySupportedPlatformFlags {
    Desktop = 1,
    Android = 2,
    iOS     = 4
}

public enum ActivityJoinRequestReply {
    No,
    Yes,
    Ignore
}

public enum Status {
    Offline      = 0,
    Online       = 1,
    Idle         = 2,
    DoNotDisturb = 3
}

public enum RelationshipType {
    None,
    Friend,
    Blocked,
    PendingIncoming,
    PendingOutgoing,
    Implicit
}

public enum LobbyType {
    Private = 1,
    Public
}

public enum LobbySearchComparison {
    LessThanOrEqual = -2,
    LessThan,
    Equal,
    GreaterThan,
    GreaterThanOrEqual,
    NotEqual
}

public enum LobbySearchCast {
    String = 1,
    Number
}

public enum LobbySearchDistance {
    Local,
    Default,
    Extended,
    Global
}

public enum KeyVariant {
    Normal,
    Right,
    Left
}

public enum MouseButton {
    Left,
    Middle,
    Right
}

public enum EntitlementType {
    Purchase = 1,
    PremiumSubscription,
    DeveloperGift,
    TestModePurchase,
    FreePurchase,
    UserGift,
    PremiumPurchase
}

public enum SkuType {
    Application = 1,
    DLC,
    Consumable,
    Bundle
}

public enum InputModeType {
    VoiceActivity = 0,
    PushToTalk
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct User {
    public long Id;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Username;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
    public string Discriminator;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Avatar;

    public bool Bot;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct OAuth2Token {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string AccessToken;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
    public string Scopes;

    public long Expires;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public partial struct ImageHandle {
    public ImageType Type;

    public long Id;

    public uint Size;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ImageDimensions {
    public uint Width;

    public uint Height;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ActivityTimestamps {
    public long Start;

    public long End;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ActivityAssets {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string LargeImage;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string LargeText;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string SmallImage;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string SmallText;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct PartySize {
    public int CurrentSize;

    public int MaxSize;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ActivityParty {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Id;

    public PartySize Size;

    public ActivityPartyPrivacy Privacy;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ActivitySecrets {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Match;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Join;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Spectate;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Activity {
    public ActivityType Type;

    public long ApplicationId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Name;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string State;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Details;

    public ActivityTimestamps Timestamps;

    public ActivityAssets Assets;

    public ActivityParty Party;

    public ActivitySecrets Secrets;

    public bool Instance;

    public uint SupportedPlatforms;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Presence {
    public Status Status;

    public Activity Activity;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Relationship {
    public RelationshipType Type;

    public User User;

    public Presence Presence;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Lobby {
    public long Id;

    public LobbyType Type;

    public long OwnerId;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string Secret;

    public uint Capacity;

    public bool Locked;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct ImeUnderline {
    public int From;

    public int To;

    public uint Color;

    public uint BackgroundColor;

    public bool Thick;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Rect {
    public int Left;

    public int Top;

    public int Right;

    public int Bottom;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct FileStat {
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
    public string Filename;

    public ulong Size;

    public ulong LastModified;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Entitlement {
    public long Id;

    public EntitlementType Type;

    public long SkuId;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct SkuPrice {
    public uint Amount;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string Currency;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct Sku {
    public long Id;

    public SkuType Type;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Name;

    public SkuPrice Price;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct InputMode {
    public InputModeType Type;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string Shortcut;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct UserAchievement {
    public long UserId;

    public long AchievementId;

    public byte PercentComplete;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string UnlockedAt;
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct LobbyTransaction {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetTypeMethod(IntPtr methodsPtr, LobbyType type);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetOwnerMethod(IntPtr methodsPtr, long ownerId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetCapacityMethod(IntPtr methodsPtr, uint capacity);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetLockedMethod(IntPtr methodsPtr, bool locked);

        internal SetTypeMethod SetType;

        internal SetOwnerMethod SetOwner;

        internal SetCapacityMethod SetCapacity;

        internal SetMetadataMethod SetMetadata;

        internal DeleteMetadataMethod DeleteMetadata;

        internal SetLockedMethod SetLocked;
    }

    internal IntPtr MethodsPtr;

    internal object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public void SetType(LobbyType type) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.SetType(this.MethodsPtr, type);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void SetOwner(long ownerId) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.SetOwner(this.MethodsPtr, ownerId);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void SetCapacity(uint capacity) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.SetCapacity(this.MethodsPtr, capacity);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void SetMetadata(string key, string value) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.SetMetadata(this.MethodsPtr, key, value);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void DeleteMetadata(string key) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.DeleteMetadata(this.MethodsPtr, key);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void SetLocked(bool locked) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.SetLocked(this.MethodsPtr, locked);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct LobbyMemberTransaction {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result DeleteMetadataMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key);

        internal SetMetadataMethod SetMetadata;

        internal DeleteMetadataMethod DeleteMetadata;
    }

    internal IntPtr MethodsPtr;

    internal object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public void SetMetadata(string key, string value) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.SetMetadata(this.MethodsPtr, key, value);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void DeleteMetadata(string key) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.DeleteMetadata(this.MethodsPtr, key);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }
}

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct LobbySearchQuery {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result FilterMethod(
            IntPtr                                  methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, LobbySearchComparison comparison, LobbySearchCast cast,
            [MarshalAs(UnmanagedType.LPStr)] string value
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SortMethod(
            IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string key, LobbySearchCast cast, [MarshalAs(UnmanagedType.LPStr)] string value
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result LimitMethod(IntPtr methodsPtr, uint limit);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result DistanceMethod(IntPtr methodsPtr, LobbySearchDistance distance);

        internal FilterMethod Filter;

        internal SortMethod Sort;

        internal LimitMethod Limit;

        internal DistanceMethod Distance;
    }

    internal IntPtr MethodsPtr;

    internal object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public void Filter(string key, LobbySearchComparison comparison, LobbySearchCast cast, string value) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.Filter(this.MethodsPtr, key, comparison, cast, value);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void Sort(string key, LobbySearchCast cast, string value) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.Sort(this.MethodsPtr, key, cast, value);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void Limit(uint limit) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.Limit(this.MethodsPtr, limit);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }

    public void Distance(LobbySearchDistance distance) {
        if (this.MethodsPtr != IntPtr.Zero) {
            Result res = this.Methods.Distance(this.MethodsPtr, distance);
            if (res != Result.Ok)
                throw new ResultException(res);
        }
    }
}

public class ResultException : Exception {
    public readonly Result Result;

    public ResultException(Result result) : base(result.ToString()) {}
}

public class Discord : IDisposable {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {}

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DestroyHandler(IntPtr MethodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result RunCallbacksMethod(IntPtr methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetLogHookCallback(IntPtr ptr, LogLevel level, [MarshalAs(UnmanagedType.LPStr)] string message);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetLogHookMethod(IntPtr methodsPtr, LogLevel minLevel, IntPtr callbackData, SetLogHookCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetApplicationManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetUserManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetImageManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetActivityManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetRelationshipManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetLobbyManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetNetworkManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetOverlayManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStorageManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetStoreManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetVoiceManagerMethod(IntPtr discordPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate IntPtr GetAchievementManagerMethod(IntPtr discordPtr);

        internal DestroyHandler Destroy;

        internal RunCallbacksMethod RunCallbacks;

        internal SetLogHookMethod SetLogHook;

        internal GetApplicationManagerMethod GetApplicationManager;

        internal GetUserManagerMethod GetUserManager;

        internal GetImageManagerMethod GetImageManager;

        internal GetActivityManagerMethod GetActivityManager;

        internal GetRelationshipManagerMethod GetRelationshipManager;

        internal GetLobbyManagerMethod GetLobbyManager;

        internal GetNetworkManagerMethod GetNetworkManager;

        internal GetOverlayManagerMethod GetOverlayManager;

        internal GetStorageManagerMethod GetStorageManager;

        internal GetStoreManagerMethod GetStoreManager;

        internal GetVoiceManagerMethod GetVoiceManager;

        internal GetAchievementManagerMethod GetAchievementManager;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFICreateParams {
        internal long ClientId;

        internal ulong Flags;

        internal IntPtr Events;

        internal IntPtr EventData;

        internal IntPtr ApplicationEvents;

        internal uint ApplicationVersion;

        internal IntPtr UserEvents;

        internal uint UserVersion;

        internal IntPtr ImageEvents;

        internal uint ImageVersion;

        internal IntPtr ActivityEvents;

        internal uint ActivityVersion;

        internal IntPtr RelationshipEvents;

        internal uint RelationshipVersion;

        internal IntPtr LobbyEvents;

        internal uint LobbyVersion;

        internal IntPtr NetworkEvents;

        internal uint NetworkVersion;

        internal IntPtr OverlayEvents;

        internal uint OverlayVersion;

        internal IntPtr StorageEvents;

        internal uint StorageVersion;

        internal IntPtr StoreEvents;

        internal uint StoreVersion;

        internal IntPtr VoiceEvents;

        internal uint VoiceVersion;

        internal IntPtr AchievementEvents;

        internal uint AchievementVersion;
    }

    [DllImport(Constants.DllName, ExactSpelling = true, CallingConvention = CallingConvention.Winapi)]
    private static extern Result DiscordCreate(uint version, ref FFICreateParams createParams, out IntPtr manager);

    public delegate void SetLogHookHandler(LogLevel level, string message);

    private GCHandle SelfHandle;

    private readonly IntPtr EventsPtr;

    private readonly FFIEvents Events;

    private readonly IntPtr ApplicationEventsPtr;

    private ApplicationManager.FFIEvents ApplicationEvents;

    internal ApplicationManager ApplicationManagerInstance;

    private readonly IntPtr UserEventsPtr;

    private UserManager.FFIEvents UserEvents;

    internal UserManager UserManagerInstance;

    private readonly IntPtr ImageEventsPtr;

    private ImageManager.FFIEvents ImageEvents;

    internal ImageManager ImageManagerInstance;

    private readonly IntPtr ActivityEventsPtr;

    private ActivityManager.FFIEvents ActivityEvents;

    internal ActivityManager ActivityManagerInstance;

    private readonly IntPtr RelationshipEventsPtr;

    private RelationshipManager.FFIEvents RelationshipEvents;

    internal RelationshipManager RelationshipManagerInstance;

    private readonly IntPtr LobbyEventsPtr;

    private LobbyManager.FFIEvents LobbyEvents;

    internal LobbyManager LobbyManagerInstance;

    private readonly IntPtr NetworkEventsPtr;

    private NetworkManager.FFIEvents NetworkEvents;

    internal NetworkManager NetworkManagerInstance;

    private readonly IntPtr OverlayEventsPtr;

    private OverlayManager.FFIEvents OverlayEvents;

    internal OverlayManager OverlayManagerInstance;

    private readonly IntPtr StorageEventsPtr;

    private StorageManager.FFIEvents StorageEvents;

    internal StorageManager StorageManagerInstance;

    private readonly IntPtr StoreEventsPtr;

    private StoreManager.FFIEvents StoreEvents;

    internal StoreManager StoreManagerInstance;

    private readonly IntPtr VoiceEventsPtr;

    private VoiceManager.FFIEvents VoiceEvents;

    internal VoiceManager VoiceManagerInstance;

    private readonly IntPtr AchievementEventsPtr;

    private AchievementManager.FFIEvents AchievementEvents;

    internal AchievementManager AchievementManagerInstance;

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    private GCHandle? setLogHook;

    public Discord(long clientId, ulong flags) {
        FFICreateParams createParams;
        createParams.ClientId            = clientId;
        createParams.Flags               = flags;
        this.Events                      = new FFIEvents();
        this.EventsPtr                   = Marshal.AllocHGlobal(Marshal.SizeOf(this.Events));
        createParams.Events              = this.EventsPtr;
        this.SelfHandle                  = GCHandle.Alloc(this);
        createParams.EventData           = GCHandle.ToIntPtr(this.SelfHandle);
        this.ApplicationEvents           = new ApplicationManager.FFIEvents();
        this.ApplicationEventsPtr        = Marshal.AllocHGlobal(Marshal.SizeOf(this.ApplicationEvents));
        createParams.ApplicationEvents   = this.ApplicationEventsPtr;
        createParams.ApplicationVersion  = 1;
        this.UserEvents                  = new UserManager.FFIEvents();
        this.UserEventsPtr               = Marshal.AllocHGlobal(Marshal.SizeOf(this.UserEvents));
        createParams.UserEvents          = this.UserEventsPtr;
        createParams.UserVersion         = 1;
        this.ImageEvents                 = new ImageManager.FFIEvents();
        this.ImageEventsPtr              = Marshal.AllocHGlobal(Marshal.SizeOf(this.ImageEvents));
        createParams.ImageEvents         = this.ImageEventsPtr;
        createParams.ImageVersion        = 1;
        this.ActivityEvents              = new ActivityManager.FFIEvents();
        this.ActivityEventsPtr           = Marshal.AllocHGlobal(Marshal.SizeOf(this.ActivityEvents));
        createParams.ActivityEvents      = this.ActivityEventsPtr;
        createParams.ActivityVersion     = 1;
        this.RelationshipEvents          = new RelationshipManager.FFIEvents();
        this.RelationshipEventsPtr       = Marshal.AllocHGlobal(Marshal.SizeOf(this.RelationshipEvents));
        createParams.RelationshipEvents  = this.RelationshipEventsPtr;
        createParams.RelationshipVersion = 1;
        this.LobbyEvents                 = new LobbyManager.FFIEvents();
        this.LobbyEventsPtr              = Marshal.AllocHGlobal(Marshal.SizeOf(this.LobbyEvents));
        createParams.LobbyEvents         = this.LobbyEventsPtr;
        createParams.LobbyVersion        = 1;
        this.NetworkEvents               = new NetworkManager.FFIEvents();
        this.NetworkEventsPtr            = Marshal.AllocHGlobal(Marshal.SizeOf(this.NetworkEvents));
        createParams.NetworkEvents       = this.NetworkEventsPtr;
        createParams.NetworkVersion      = 1;
        this.OverlayEvents               = new OverlayManager.FFIEvents();
        this.OverlayEventsPtr            = Marshal.AllocHGlobal(Marshal.SizeOf(this.OverlayEvents));
        createParams.OverlayEvents       = this.OverlayEventsPtr;
        createParams.OverlayVersion      = 2;
        this.StorageEvents               = new StorageManager.FFIEvents();
        this.StorageEventsPtr            = Marshal.AllocHGlobal(Marshal.SizeOf(this.StorageEvents));
        createParams.StorageEvents       = this.StorageEventsPtr;
        createParams.StorageVersion      = 1;
        this.StoreEvents                 = new StoreManager.FFIEvents();
        this.StoreEventsPtr              = Marshal.AllocHGlobal(Marshal.SizeOf(this.StoreEvents));
        createParams.StoreEvents         = this.StoreEventsPtr;
        createParams.StoreVersion        = 1;
        this.VoiceEvents                 = new VoiceManager.FFIEvents();
        this.VoiceEventsPtr              = Marshal.AllocHGlobal(Marshal.SizeOf(this.VoiceEvents));
        createParams.VoiceEvents         = this.VoiceEventsPtr;
        createParams.VoiceVersion        = 1;
        this.AchievementEvents           = new AchievementManager.FFIEvents();
        this.AchievementEventsPtr        = Marshal.AllocHGlobal(Marshal.SizeOf(this.AchievementEvents));
        createParams.AchievementEvents   = this.AchievementEventsPtr;
        createParams.AchievementVersion  = 1;
        this.InitEvents(this.EventsPtr, ref this.Events);
        Result result = DiscordCreate(3, ref createParams, out this.MethodsPtr);
        if (result != Result.Ok) {
            this.Dispose();
            throw new ResultException(result);
        }
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public void Dispose() {
        if (this.MethodsPtr != IntPtr.Zero)
            this.Methods.Destroy(this.MethodsPtr);
        this.SelfHandle.Free();
        Marshal.FreeHGlobal(this.EventsPtr);
        Marshal.FreeHGlobal(this.ApplicationEventsPtr);
        Marshal.FreeHGlobal(this.UserEventsPtr);
        Marshal.FreeHGlobal(this.ImageEventsPtr);
        Marshal.FreeHGlobal(this.ActivityEventsPtr);
        Marshal.FreeHGlobal(this.RelationshipEventsPtr);
        Marshal.FreeHGlobal(this.LobbyEventsPtr);
        Marshal.FreeHGlobal(this.NetworkEventsPtr);
        Marshal.FreeHGlobal(this.OverlayEventsPtr);
        Marshal.FreeHGlobal(this.StorageEventsPtr);
        Marshal.FreeHGlobal(this.StoreEventsPtr);
        Marshal.FreeHGlobal(this.VoiceEventsPtr);
        Marshal.FreeHGlobal(this.AchievementEventsPtr);
        if (this.setLogHook.HasValue)
            this.setLogHook.Value.Free();
    }

    public void RunCallbacks() {
        Result res = this.Methods.RunCallbacks(this.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    [MonoPInvokeCallback]
    private static void SetLogHookCallbackImpl(IntPtr ptr, LogLevel level, string message) {
        GCHandle          h        = GCHandle.FromIntPtr(ptr);
        SetLogHookHandler callback = (SetLogHookHandler)h.Target;
        callback(level, message);
    }

    public void SetLogHook(LogLevel minLevel, SetLogHookHandler callback) {
        if (this.setLogHook.HasValue)
            this.setLogHook.Value.Free();
        this.setLogHook = GCHandle.Alloc(callback);
        this.Methods.SetLogHook(this.MethodsPtr, minLevel, GCHandle.ToIntPtr(this.setLogHook.Value), SetLogHookCallbackImpl);
    }

    public ApplicationManager GetApplicationManager() {
        if (this.ApplicationManagerInstance == null)
            this.ApplicationManagerInstance = new ApplicationManager(
            this.Methods.GetApplicationManager(this.MethodsPtr),
            this.ApplicationEventsPtr,
            ref this.ApplicationEvents
            );
        return this.ApplicationManagerInstance;
    }

    public UserManager GetUserManager() {
        if (this.UserManagerInstance == null)
            this.UserManagerInstance = new UserManager(this.Methods.GetUserManager(this.MethodsPtr), this.UserEventsPtr, ref this.UserEvents);
        return this.UserManagerInstance;
    }

    public ImageManager GetImageManager() {
        if (this.ImageManagerInstance == null)
            this.ImageManagerInstance = new ImageManager(this.Methods.GetImageManager(this.MethodsPtr), this.ImageEventsPtr, ref this.ImageEvents);
        return this.ImageManagerInstance;
    }

    public ActivityManager GetActivityManager() {
        if (this.ActivityManagerInstance == null)
            this.ActivityManagerInstance = new ActivityManager(this.Methods.GetActivityManager(this.MethodsPtr), this.ActivityEventsPtr, ref this.ActivityEvents);
        return this.ActivityManagerInstance;
    }

    public RelationshipManager GetRelationshipManager() {
        if (this.RelationshipManagerInstance == null)
            this.RelationshipManagerInstance = new RelationshipManager(
            this.Methods.GetRelationshipManager(this.MethodsPtr),
            this.RelationshipEventsPtr,
            ref this.RelationshipEvents
            );
        return this.RelationshipManagerInstance;
    }

    public LobbyManager GetLobbyManager() {
        if (this.LobbyManagerInstance == null)
            this.LobbyManagerInstance = new LobbyManager(this.Methods.GetLobbyManager(this.MethodsPtr), this.LobbyEventsPtr, ref this.LobbyEvents);
        return this.LobbyManagerInstance;
    }

    public NetworkManager GetNetworkManager() {
        if (this.NetworkManagerInstance == null)
            this.NetworkManagerInstance = new NetworkManager(this.Methods.GetNetworkManager(this.MethodsPtr), this.NetworkEventsPtr, ref this.NetworkEvents);
        return this.NetworkManagerInstance;
    }

    public OverlayManager GetOverlayManager() {
        if (this.OverlayManagerInstance == null)
            this.OverlayManagerInstance = new OverlayManager(this.Methods.GetOverlayManager(this.MethodsPtr), this.OverlayEventsPtr, ref this.OverlayEvents);
        return this.OverlayManagerInstance;
    }

    public StorageManager GetStorageManager() {
        if (this.StorageManagerInstance == null)
            this.StorageManagerInstance = new StorageManager(this.Methods.GetStorageManager(this.MethodsPtr), this.StorageEventsPtr, ref this.StorageEvents);
        return this.StorageManagerInstance;
    }

    public StoreManager GetStoreManager() {
        if (this.StoreManagerInstance == null)
            this.StoreManagerInstance = new StoreManager(this.Methods.GetStoreManager(this.MethodsPtr), this.StoreEventsPtr, ref this.StoreEvents);
        return this.StoreManagerInstance;
    }

    public VoiceManager GetVoiceManager() {
        if (this.VoiceManagerInstance == null)
            this.VoiceManagerInstance = new VoiceManager(this.Methods.GetVoiceManager(this.MethodsPtr), this.VoiceEventsPtr, ref this.VoiceEvents);
        return this.VoiceManagerInstance;
    }

    public AchievementManager GetAchievementManager() {
        if (this.AchievementManagerInstance == null)
            this.AchievementManagerInstance = new AchievementManager(
            this.Methods.GetAchievementManager(this.MethodsPtr),
            this.AchievementEventsPtr,
            ref this.AchievementEvents
            );
        return this.AchievementManagerInstance;
    }
}

internal class MonoPInvokeCallbackAttribute : Attribute {}

public class ApplicationManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {}

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ValidateOrExitCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ValidateOrExitMethod(IntPtr methodsPtr, IntPtr callbackData, ValidateOrExitCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetCurrentLocaleMethod(IntPtr methodsPtr, StringBuilder locale);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetCurrentBranchMethod(IntPtr methodsPtr, StringBuilder branch);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetOAuth2TokenCallback(IntPtr ptr, Result result, ref OAuth2Token oauth2Token);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetOAuth2TokenMethod(IntPtr methodsPtr, IntPtr callbackData, GetOAuth2TokenCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetTicketCallback(IntPtr ptr, Result result, [MarshalAs(UnmanagedType.LPStr)] ref string data);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetTicketMethod(IntPtr methodsPtr, IntPtr callbackData, GetTicketCallback callback);

        internal ValidateOrExitMethod ValidateOrExit;

        internal GetCurrentLocaleMethod GetCurrentLocale;

        internal GetCurrentBranchMethod GetCurrentBranch;

        internal GetOAuth2TokenMethod GetOAuth2Token;

        internal GetTicketMethod GetTicket;
    }

    public delegate void ValidateOrExitHandler(Result result);

    public delegate void GetOAuth2TokenHandler(Result result, ref OAuth2Token oauth2Token);

    public delegate void GetTicketHandler(Result result, ref string data);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    internal ApplicationManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    private static void ValidateOrExitCallbackImpl(IntPtr ptr, Result result) {
        GCHandle              h        = GCHandle.FromIntPtr(ptr);
        ValidateOrExitHandler callback = (ValidateOrExitHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void ValidateOrExit(ValidateOrExitHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ValidateOrExit(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), ValidateOrExitCallbackImpl);
    }

    public string GetCurrentLocale() {
        StringBuilder ret = new(128);
        this.Methods.GetCurrentLocale(this.MethodsPtr, ret);
        return ret.ToString();
    }

    public string GetCurrentBranch() {
        StringBuilder ret = new(4096);
        this.Methods.GetCurrentBranch(this.MethodsPtr, ret);
        return ret.ToString();
    }

    [MonoPInvokeCallback]
    private static void GetOAuth2TokenCallbackImpl(IntPtr ptr, Result result, ref OAuth2Token oauth2Token) {
        GCHandle              h        = GCHandle.FromIntPtr(ptr);
        GetOAuth2TokenHandler callback = (GetOAuth2TokenHandler)h.Target;
        h.Free();
        callback(result, ref oauth2Token);
    }

    public void GetOAuth2Token(GetOAuth2TokenHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.GetOAuth2Token(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), GetOAuth2TokenCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void GetTicketCallbackImpl(IntPtr ptr, Result result, ref string data) {
        GCHandle         h        = GCHandle.FromIntPtr(ptr);
        GetTicketHandler callback = (GetTicketHandler)h.Target;
        h.Free();
        callback(result, ref data);
    }

    public void GetTicket(GetTicketHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.GetTicket(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), GetTicketCallbackImpl);
    }
}

public class UserManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CurrentUserUpdateHandler(IntPtr ptr);

        internal CurrentUserUpdateHandler OnCurrentUserUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetCurrentUserMethod(IntPtr methodsPtr, ref User currentUser);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetUserCallback(IntPtr ptr, Result result, ref User user);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetUserMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, GetUserCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetCurrentUserPremiumTypeMethod(IntPtr methodsPtr, ref PremiumType premiumType);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result CurrentUserHasFlagMethod(IntPtr methodsPtr, UserFlag flag, ref bool hasFlag);

        internal GetCurrentUserMethod GetCurrentUser;

        internal GetUserMethod GetUser;

        internal GetCurrentUserPremiumTypeMethod GetCurrentUserPremiumType;

        internal CurrentUserHasFlagMethod CurrentUserHasFlag;
    }

    public delegate void GetUserHandler(Result result, ref User user);

    public delegate void CurrentUserUpdateHandler();

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event CurrentUserUpdateHandler OnCurrentUserUpdate;

    internal UserManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnCurrentUserUpdate = OnCurrentUserUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public User GetCurrentUser() {
        User   ret = new();
        Result res = this.Methods.GetCurrentUser(this.MethodsPtr, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void GetUserCallbackImpl(IntPtr ptr, Result result, ref User user) {
        GCHandle       h        = GCHandle.FromIntPtr(ptr);
        GetUserHandler callback = (GetUserHandler)h.Target;
        h.Free();
        callback(result, ref user);
    }

    private static readonly FFIMethods.GetUserCallback getUserCallback = GetUserCallbackImpl;
    
    public void GetUser(long userId, GetUserHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.GetUser(this.MethodsPtr, userId, GCHandle.ToIntPtr(wrapped), getUserCallback);
    }

    public PremiumType GetCurrentUserPremiumType() {
        PremiumType ret = new();
        Result      res = this.Methods.GetCurrentUserPremiumType(this.MethodsPtr, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public bool CurrentUserHasFlag(UserFlag flag) {
        bool   ret = new();
        Result res = this.Methods.CurrentUserHasFlag(this.MethodsPtr, flag, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void OnCurrentUserUpdateImpl(IntPtr ptr) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.UserManagerInstance.OnCurrentUserUpdate != null)
            d.UserManagerInstance.OnCurrentUserUpdate.Invoke();
    }
}

public partial class ImageManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {}

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchCallback(IntPtr ptr, Result result, ImageHandle handleResult);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchMethod(IntPtr methodsPtr, ImageHandle handle, bool refresh, IntPtr callbackData, FetchCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetDimensionsMethod(IntPtr methodsPtr, ImageHandle handle, ref ImageDimensions dimensions);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetDataMethod(IntPtr methodsPtr, ImageHandle handle, byte[] data, int dataLen);

        internal FetchMethod Fetch;

        internal GetDimensionsMethod GetDimensions;

        internal GetDataMethod GetData;
    }

    public delegate void FetchHandler(Result result, ImageHandle handleResult);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    internal ImageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    private static void FetchCallbackImpl(IntPtr ptr, Result result, ImageHandle handleResult) {
        GCHandle     h        = GCHandle.FromIntPtr(ptr);
        FetchHandler callback = (FetchHandler)h.Target;
        h.Free();
        callback(result, handleResult);
    }

    public void Fetch(ImageHandle handle, bool refresh, FetchHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.Fetch(this.MethodsPtr, handle, refresh, GCHandle.ToIntPtr(wrapped), FetchCallbackImpl);
    }

    public ImageDimensions GetDimensions(ImageHandle handle) {
        ImageDimensions ret = new();
        Result          res = this.Methods.GetDimensions(this.MethodsPtr, handle, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public void GetData(ImageHandle handle, byte[] data) {
        Result res = this.Methods.GetData(this.MethodsPtr, handle, data, data.Length);
        if (res != Result.Ok)
            throw new ResultException(res);
    }
}

public partial class ActivityManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ActivityJoinHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ActivitySpectateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ActivityJoinRequestHandler(IntPtr ptr, ref User user);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ActivityInviteHandler(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity);

        internal ActivityJoinHandler OnActivityJoin;

        internal ActivitySpectateHandler OnActivitySpectate;

        internal ActivityJoinRequestHandler OnActivityJoinRequest;

        internal ActivityInviteHandler OnActivityInvite;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result RegisterCommandMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string command);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result RegisterSteamMethod(IntPtr methodsPtr, uint steamId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UpdateActivityCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UpdateActivityMethod(IntPtr methodsPtr, ref Activity activity, IntPtr callbackData, UpdateActivityCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ClearActivityCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ClearActivityMethod(IntPtr methodsPtr, IntPtr callbackData, ClearActivityCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SendRequestReplyCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SendRequestReplyMethod(
            IntPtr methodsPtr, long userId, ActivityJoinRequestReply reply, IntPtr callbackData, SendRequestReplyCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SendInviteCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SendInviteMethod(
            IntPtr methodsPtr, long userId, ActivityActionType type, [MarshalAs(UnmanagedType.LPStr)] string content, IntPtr callbackData, SendInviteCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void AcceptInviteCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void AcceptInviteMethod(IntPtr methodsPtr, long userId, IntPtr callbackData, AcceptInviteCallback callback);

        internal RegisterCommandMethod RegisterCommand;

        internal RegisterSteamMethod RegisterSteam;

        internal UpdateActivityMethod UpdateActivity;

        internal ClearActivityMethod ClearActivity;

        internal SendRequestReplyMethod SendRequestReply;

        internal SendInviteMethod SendInvite;

        internal AcceptInviteMethod AcceptInvite;
    }

    public delegate void UpdateActivityHandler(Result result);

    public delegate void ClearActivityHandler(Result result);

    public delegate void SendRequestReplyHandler(Result result);

    public delegate void SendInviteHandler(Result result);

    public delegate void AcceptInviteHandler(Result result);

    public delegate void ActivityJoinHandler(string secret);

    public delegate void ActivitySpectateHandler(string secret);

    public delegate void ActivityJoinRequestHandler(ref User user);

    public delegate void ActivityInviteHandler(ActivityActionType type, ref User user, ref Activity activity);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event ActivityJoinHandler OnActivityJoin;

    public event ActivitySpectateHandler OnActivitySpectate;

    public event ActivityJoinRequestHandler OnActivityJoinRequest;

    public event ActivityInviteHandler OnActivityInvite;

    internal ActivityManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnActivityJoin        = OnActivityJoinImpl;
        events.OnActivitySpectate    = OnActivitySpectateImpl;
        events.OnActivityJoinRequest = OnActivityJoinRequestImpl;
        events.OnActivityInvite      = OnActivityInviteImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public void RegisterCommand(string command) {
        Result res = this.Methods.RegisterCommand(this.MethodsPtr, command);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public void RegisterSteam(uint steamId) {
        Result res = this.Methods.RegisterSteam(this.MethodsPtr, steamId);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    [MonoPInvokeCallback]
    private static void UpdateActivityCallbackImpl(IntPtr ptr, Result result) {
        GCHandle              h        = GCHandle.FromIntPtr(ptr);
        UpdateActivityHandler callback = (UpdateActivityHandler)h.Target;
        h.Free();
        callback(result);
    }

    private static readonly FFIMethods.UpdateActivityCallback UpdateActivityCallback = UpdateActivityCallbackImpl;
    
    public void UpdateActivity(Activity activity, UpdateActivityHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.UpdateActivity(this.MethodsPtr, ref activity, GCHandle.ToIntPtr(wrapped), UpdateActivityCallback);
    }

    [MonoPInvokeCallback]
    private static void ClearActivityCallbackImpl(IntPtr ptr, Result result) {
        GCHandle             h        = GCHandle.FromIntPtr(ptr);
        ClearActivityHandler callback = (ClearActivityHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void ClearActivity(ClearActivityHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ClearActivity(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), ClearActivityCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void SendRequestReplyCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                h        = GCHandle.FromIntPtr(ptr);
        SendRequestReplyHandler callback = (SendRequestReplyHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SendRequestReply(long userId, ActivityJoinRequestReply reply, SendRequestReplyHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SendRequestReply(this.MethodsPtr, userId, reply, GCHandle.ToIntPtr(wrapped), SendRequestReplyCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void SendInviteCallbackImpl(IntPtr ptr, Result result) {
        GCHandle          h        = GCHandle.FromIntPtr(ptr);
        SendInviteHandler callback = (SendInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SendInvite(long userId, ActivityActionType type, string content, SendInviteHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SendInvite(this.MethodsPtr, userId, type, content, GCHandle.ToIntPtr(wrapped), SendInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void AcceptInviteCallbackImpl(IntPtr ptr, Result result) {
        GCHandle            h        = GCHandle.FromIntPtr(ptr);
        AcceptInviteHandler callback = (AcceptInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void AcceptInvite(long userId, AcceptInviteHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.AcceptInvite(this.MethodsPtr, userId, GCHandle.ToIntPtr(wrapped), AcceptInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void OnActivityJoinImpl(IntPtr ptr, string secret) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.ActivityManagerInstance.OnActivityJoin != null)
            d.ActivityManagerInstance.OnActivityJoin.Invoke(secret);
    }

    [MonoPInvokeCallback]
    private static void OnActivitySpectateImpl(IntPtr ptr, string secret) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.ActivityManagerInstance.OnActivitySpectate != null)
            d.ActivityManagerInstance.OnActivitySpectate.Invoke(secret);
    }

    [MonoPInvokeCallback]
    private static void OnActivityJoinRequestImpl(IntPtr ptr, ref User user) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.ActivityManagerInstance.OnActivityJoinRequest != null)
            d.ActivityManagerInstance.OnActivityJoinRequest.Invoke(ref user);
    }

    [MonoPInvokeCallback]
    private static void OnActivityInviteImpl(IntPtr ptr, ActivityActionType type, ref User user, ref Activity activity) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.ActivityManagerInstance.OnActivityInvite != null)
            d.ActivityManagerInstance.OnActivityInvite.Invoke(type, ref user, ref activity);
    }
}

public class RelationshipManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void RefreshHandler(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void RelationshipUpdateHandler(IntPtr ptr, ref Relationship relationship);

        internal RefreshHandler OnRefresh;

        internal RelationshipUpdateHandler OnRelationshipUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate bool FilterCallback(IntPtr ptr, ref Relationship relationship);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FilterMethod(IntPtr methodsPtr, IntPtr callbackData, FilterCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result CountMethod(IntPtr methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetMethod(IntPtr methodsPtr, long userId, ref Relationship relationship);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetAtMethod(IntPtr methodsPtr, uint index, ref Relationship relationship);

        internal FilterMethod Filter;

        internal CountMethod Count;

        internal GetMethod Get;

        internal GetAtMethod GetAt;
    }

    public delegate bool FilterHandler(ref Relationship relationship);

    public delegate void RefreshHandler();

    public delegate void RelationshipUpdateHandler(ref Relationship relationship);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event RefreshHandler OnRefresh;

    public event RelationshipUpdateHandler OnRelationshipUpdate;

    internal RelationshipManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnRefresh            = OnRefreshImpl;
        events.OnRelationshipUpdate = OnRelationshipUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    private static bool FilterCallbackImpl(IntPtr ptr, ref Relationship relationship) {
        GCHandle      h        = GCHandle.FromIntPtr(ptr);
        FilterHandler callback = (FilterHandler)h.Target;
        return callback(ref relationship);
    }

    public void Filter(FilterHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.Filter(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), FilterCallbackImpl);
        wrapped.Free();
    }

    public int Count() {
        int    ret = new();
        Result res = this.Methods.Count(this.MethodsPtr, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public Relationship Get(long userId) {
        Relationship ret = new();
        Result       res = this.Methods.Get(this.MethodsPtr, userId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public Relationship GetAt(uint index) {
        Relationship ret = new();
        Result       res = this.Methods.GetAt(this.MethodsPtr, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void OnRefreshImpl(IntPtr ptr) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.RelationshipManagerInstance.OnRefresh != null)
            d.RelationshipManagerInstance.OnRefresh.Invoke();
    }

    [MonoPInvokeCallback]
    private static void OnRelationshipUpdateImpl(IntPtr ptr, ref Relationship relationship) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.RelationshipManagerInstance.OnRelationshipUpdate != null)
            d.RelationshipManagerInstance.OnRelationshipUpdate.Invoke(ref relationship);
    }
}

public partial class LobbyManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void LobbyUpdateHandler(IntPtr ptr, long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void LobbyDeleteHandler(IntPtr ptr, long lobbyId, uint reason);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void MemberConnectHandler(IntPtr ptr, long lobbyId, long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void MemberUpdateHandler(IntPtr ptr, long lobbyId, long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void MemberDisconnectHandler(IntPtr ptr, long lobbyId, long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void LobbyMessageHandler(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SpeakingHandler(IntPtr ptr, long lobbyId, long userId, bool speaking);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void NetworkMessageHandler(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr, int dataLen);

        internal LobbyUpdateHandler OnLobbyUpdate;

        internal LobbyDeleteHandler OnLobbyDelete;

        internal MemberConnectHandler OnMemberConnect;

        internal MemberUpdateHandler OnMemberUpdate;

        internal MemberDisconnectHandler OnMemberDisconnect;

        internal LobbyMessageHandler OnLobbyMessage;

        internal SpeakingHandler OnSpeaking;

        internal NetworkMessageHandler OnNetworkMessage;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyCreateTransactionMethod(IntPtr methodsPtr, ref IntPtr transaction);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, ref IntPtr transaction);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetMemberUpdateTransactionMethod(IntPtr methodsPtr, long lobbyId, long userId, ref IntPtr transaction);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CreateLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CreateLobbyMethod(IntPtr methodsPtr, IntPtr transaction, IntPtr callbackData, CreateLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UpdateLobbyCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UpdateLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr transaction, IntPtr callbackData, UpdateLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DeleteLobbyCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DeleteLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DeleteLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ConnectLobbyCallback(IntPtr ptr, Result result, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ConnectLobbyMethod(
            IntPtr methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string secret, IntPtr callbackData, ConnectLobbyCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ConnectLobbyWithActivitySecretCallback(IntPtr ptr, Result result, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ConnectLobbyWithActivitySecretMethod(
            IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string activitySecret, IntPtr callbackData, ConnectLobbyWithActivitySecretCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DisconnectLobbyCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DisconnectLobbyMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DisconnectLobbyCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyMethod(IntPtr methodsPtr, long lobbyId, ref Lobby lobby);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyActivitySecretMethod(IntPtr methodsPtr, long lobbyId, StringBuilder secret);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyMetadataValueMethod(IntPtr methodsPtr, long lobbyId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, int index, StringBuilder key);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result LobbyMetadataCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result MemberCountMethod(IntPtr methodsPtr, long lobbyId, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetMemberUserIdMethod(IntPtr methodsPtr, long lobbyId, int index, ref long userId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetMemberUserMethod(IntPtr methodsPtr, long lobbyId, long userId, ref User user);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetMemberMetadataValueMethod(
            IntPtr methodsPtr, long lobbyId, long userId, [MarshalAs(UnmanagedType.LPStr)] string key, StringBuilder value
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetMemberMetadataKeyMethod(IntPtr methodsPtr, long lobbyId, long userId, int index, StringBuilder key);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result MemberMetadataCountMethod(IntPtr methodsPtr, long lobbyId, long userId, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UpdateMemberCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UpdateMemberMethod(
            IntPtr methodsPtr, long lobbyId, long userId, IntPtr transaction, IntPtr callbackData, UpdateMemberCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SendLobbyMessageCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SendLobbyMessageMethod(
            IntPtr methodsPtr, long lobbyId, byte[] data, int dataLen, IntPtr callbackData, SendLobbyMessageCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetSearchQueryMethod(IntPtr methodsPtr, ref IntPtr query);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SearchCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SearchMethod(IntPtr methodsPtr, IntPtr query, IntPtr callbackData, SearchCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void LobbyCountMethod(IntPtr methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLobbyIdMethod(IntPtr methodsPtr, int index, ref long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ConnectVoiceCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ConnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, ConnectVoiceCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DisconnectVoiceCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void DisconnectVoiceMethod(IntPtr methodsPtr, long lobbyId, IntPtr callbackData, DisconnectVoiceCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result ConnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result DisconnectNetworkMethod(IntPtr methodsPtr, long lobbyId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result FlushNetworkMethod(IntPtr methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result OpenNetworkChannelMethod(IntPtr methodsPtr, long lobbyId, byte channelId, bool reliable);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SendNetworkMessageMethod(IntPtr methodsPtr, long lobbyId, long userId, byte channelId, byte[] data, int dataLen);

        internal GetLobbyCreateTransactionMethod GetLobbyCreateTransaction;

        internal GetLobbyUpdateTransactionMethod GetLobbyUpdateTransaction;

        internal GetMemberUpdateTransactionMethod GetMemberUpdateTransaction;

        internal CreateLobbyMethod CreateLobby;

        internal UpdateLobbyMethod UpdateLobby;

        internal DeleteLobbyMethod DeleteLobby;

        internal ConnectLobbyMethod ConnectLobby;

        internal ConnectLobbyWithActivitySecretMethod ConnectLobbyWithActivitySecret;

        internal DisconnectLobbyMethod DisconnectLobby;

        internal GetLobbyMethod GetLobby;

        internal GetLobbyActivitySecretMethod GetLobbyActivitySecret;

        internal GetLobbyMetadataValueMethod GetLobbyMetadataValue;

        internal GetLobbyMetadataKeyMethod GetLobbyMetadataKey;

        internal LobbyMetadataCountMethod LobbyMetadataCount;

        internal MemberCountMethod MemberCount;

        internal GetMemberUserIdMethod GetMemberUserId;

        internal GetMemberUserMethod GetMemberUser;

        internal GetMemberMetadataValueMethod GetMemberMetadataValue;

        internal GetMemberMetadataKeyMethod GetMemberMetadataKey;

        internal MemberMetadataCountMethod MemberMetadataCount;

        internal UpdateMemberMethod UpdateMember;

        internal SendLobbyMessageMethod SendLobbyMessage;

        internal GetSearchQueryMethod GetSearchQuery;

        internal SearchMethod Search;

        internal LobbyCountMethod LobbyCount;

        internal GetLobbyIdMethod GetLobbyId;

        internal ConnectVoiceMethod ConnectVoice;

        internal DisconnectVoiceMethod DisconnectVoice;

        internal ConnectNetworkMethod ConnectNetwork;

        internal DisconnectNetworkMethod DisconnectNetwork;

        internal FlushNetworkMethod FlushNetwork;

        internal OpenNetworkChannelMethod OpenNetworkChannel;

        internal SendNetworkMessageMethod SendNetworkMessage;
    }

    public delegate void CreateLobbyHandler(Result result, ref Lobby lobby);

    public delegate void UpdateLobbyHandler(Result result);

    public delegate void DeleteLobbyHandler(Result result);

    public delegate void ConnectLobbyHandler(Result result, ref Lobby lobby);

    public delegate void ConnectLobbyWithActivitySecretHandler(Result result, ref Lobby lobby);

    public delegate void DisconnectLobbyHandler(Result result);

    public delegate void UpdateMemberHandler(Result result);

    public delegate void SendLobbyMessageHandler(Result result);

    public delegate void SearchHandler(Result result);

    public delegate void ConnectVoiceHandler(Result result);

    public delegate void DisconnectVoiceHandler(Result result);

    public delegate void LobbyUpdateHandler(long lobbyId);

    public delegate void LobbyDeleteHandler(long lobbyId, uint reason);

    public delegate void MemberConnectHandler(long lobbyId, long userId);

    public delegate void MemberUpdateHandler(long lobbyId, long userId);

    public delegate void MemberDisconnectHandler(long lobbyId, long userId);

    public delegate void LobbyMessageHandler(long lobbyId, long userId, byte[] data);

    public delegate void SpeakingHandler(long lobbyId, long userId, bool speaking);

    public delegate void NetworkMessageHandler(long lobbyId, long userId, byte channelId, byte[] data);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event LobbyUpdateHandler OnLobbyUpdate;

    public event LobbyDeleteHandler OnLobbyDelete;

    public event MemberConnectHandler OnMemberConnect;

    public event MemberUpdateHandler OnMemberUpdate;

    public event MemberDisconnectHandler OnMemberDisconnect;

    public event LobbyMessageHandler OnLobbyMessage;

    public event SpeakingHandler OnSpeaking;

    public event NetworkMessageHandler OnNetworkMessage;

    internal LobbyManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnLobbyUpdate      = OnLobbyUpdateImpl;
        events.OnLobbyDelete      = OnLobbyDeleteImpl;
        events.OnMemberConnect    = OnMemberConnectImpl;
        events.OnMemberUpdate     = OnMemberUpdateImpl;
        events.OnMemberDisconnect = OnMemberDisconnectImpl;
        events.OnLobbyMessage     = OnLobbyMessageImpl;
        events.OnSpeaking         = OnSpeakingImpl;
        events.OnNetworkMessage   = OnNetworkMessageImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public LobbyTransaction GetLobbyCreateTransaction() {
        LobbyTransaction ret = new();
        Result           res = this.Methods.GetLobbyCreateTransaction(this.MethodsPtr, ref ret.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public LobbyTransaction GetLobbyUpdateTransaction(long lobbyId) {
        LobbyTransaction ret = new();
        Result           res = this.Methods.GetLobbyUpdateTransaction(this.MethodsPtr, lobbyId, ref ret.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public LobbyMemberTransaction GetMemberUpdateTransaction(long lobbyId, long userId) {
        LobbyMemberTransaction ret = new();
        Result                 res = this.Methods.GetMemberUpdateTransaction(this.MethodsPtr, lobbyId, userId, ref ret.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void CreateLobbyCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby) {
        GCHandle           h        = GCHandle.FromIntPtr(ptr);
        CreateLobbyHandler callback = (CreateLobbyHandler)h.Target;
        h.Free();
        callback(result, ref lobby);
    }

    private static readonly FFIMethods.CreateLobbyCallback createLobbyCallback = CreateLobbyCallbackImpl;

    public void CreateLobby(LobbyTransaction transaction, CreateLobbyHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.CreateLobby(this.MethodsPtr, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), createLobbyCallback);
        transaction.MethodsPtr = IntPtr.Zero;
    }

    [MonoPInvokeCallback]
    private static void UpdateLobbyCallbackImpl(IntPtr ptr, Result result) {
        GCHandle           h        = GCHandle.FromIntPtr(ptr);
        UpdateLobbyHandler callback = (UpdateLobbyHandler)h.Target;
        h.Free();
        callback(result);
    }

    private static readonly FFIMethods.UpdateLobbyCallback updateLobbyCallback = UpdateLobbyCallbackImpl;

    public void UpdateLobby(long lobbyId, LobbyTransaction transaction, UpdateLobbyHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.UpdateLobby(this.MethodsPtr, lobbyId, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), updateLobbyCallback);
        transaction.MethodsPtr = IntPtr.Zero;
    }

    [MonoPInvokeCallback]
    private static void DeleteLobbyCallbackImpl(IntPtr ptr, Result result) {
        GCHandle           h        = GCHandle.FromIntPtr(ptr);
        DeleteLobbyHandler callback = (DeleteLobbyHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void DeleteLobby(long lobbyId, DeleteLobbyHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.DeleteLobby(this.MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DeleteLobbyCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void ConnectLobbyCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby) {
        GCHandle            h        = GCHandle.FromIntPtr(ptr);
        ConnectLobbyHandler callback = (ConnectLobbyHandler)h.Target;
        h.Free();
        callback(result, ref lobby);
    }

    public void ConnectLobby(long lobbyId, string secret, ConnectLobbyHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ConnectLobby(this.MethodsPtr, lobbyId, secret, GCHandle.ToIntPtr(wrapped), ConnectLobbyCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void ConnectLobbyWithActivitySecretCallbackImpl(IntPtr ptr, Result result, ref Lobby lobby) {
        GCHandle                              h        = GCHandle.FromIntPtr(ptr);
        ConnectLobbyWithActivitySecretHandler callback = (ConnectLobbyWithActivitySecretHandler)h.Target;
        h.Free();
        callback(result, ref lobby);
    }

    private static readonly FFIMethods.ConnectLobbyWithActivitySecretCallback ConnectLobbyWithActivitySecretCallback = ConnectLobbyWithActivitySecretCallbackImpl;
    
    public void ConnectLobbyWithActivitySecret(string activitySecret, ConnectLobbyWithActivitySecretHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ConnectLobbyWithActivitySecret(this.MethodsPtr, activitySecret, GCHandle.ToIntPtr(wrapped), ConnectLobbyWithActivitySecretCallback);
    }

    [MonoPInvokeCallback]
    private static void DisconnectLobbyCallbackImpl(IntPtr ptr, Result result) {
        GCHandle               h        = GCHandle.FromIntPtr(ptr);
        DisconnectLobbyHandler callback = (DisconnectLobbyHandler)h.Target;
        h.Free();
        callback(result);
    }

    private static readonly FFIMethods.DisconnectLobbyCallback disconnectLobbyCallback = DisconnectLobbyCallbackImpl;

    public void DisconnectLobby(long lobbyId, DisconnectLobbyHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.DisconnectLobby(this.MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), disconnectLobbyCallback);
    }

    public Lobby GetLobby(long lobbyId) {
        Lobby  ret = new();
        Result res = this.Methods.GetLobby(this.MethodsPtr, lobbyId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public string GetLobbyActivitySecret(long lobbyId) {
        StringBuilder ret = new(128);
        Result        res = this.Methods.GetLobbyActivitySecret(this.MethodsPtr, lobbyId, ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret.ToString();
    }

    public string GetLobbyMetadataValue(long lobbyId, string key) {
        StringBuilder ret = new(4096);
        Result        res = this.Methods.GetLobbyMetadataValue(this.MethodsPtr, lobbyId, key, ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret.ToString();
    }

    public string GetLobbyMetadataKey(long lobbyId, int index) {
        StringBuilder ret = new(256);
        Result        res = this.Methods.GetLobbyMetadataKey(this.MethodsPtr, lobbyId, index, ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret.ToString();
    }

    public int LobbyMetadataCount(long lobbyId) {
        int    ret = new();
        Result res = this.Methods.LobbyMetadataCount(this.MethodsPtr, lobbyId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public int MemberCount(long lobbyId) {
        int    ret = new();
        Result res = this.Methods.MemberCount(this.MethodsPtr, lobbyId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public long GetMemberUserId(long lobbyId, int index) {
        long   ret = new();
        Result res = this.Methods.GetMemberUserId(this.MethodsPtr, lobbyId, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public User GetMemberUser(long lobbyId, long userId) {
        User   ret = new();
        Result res = this.Methods.GetMemberUser(this.MethodsPtr, lobbyId, userId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public string GetMemberMetadataValue(long lobbyId, long userId, string key) {
        StringBuilder ret = new(4096);
        Result        res = this.Methods.GetMemberMetadataValue(this.MethodsPtr, lobbyId, userId, key, ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret.ToString();
    }

    public string GetMemberMetadataKey(long lobbyId, long userId, int index) {
        StringBuilder ret = new(256);
        Result        res = this.Methods.GetMemberMetadataKey(this.MethodsPtr, lobbyId, userId, index, ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret.ToString();
    }

    public int MemberMetadataCount(long lobbyId, long userId) {
        int    ret = new();
        Result res = this.Methods.MemberMetadataCount(this.MethodsPtr, lobbyId, userId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void UpdateMemberCallbackImpl(IntPtr ptr, Result result) {
        GCHandle            h        = GCHandle.FromIntPtr(ptr);
        UpdateMemberHandler callback = (UpdateMemberHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void UpdateMember(long lobbyId, long userId, LobbyMemberTransaction transaction, UpdateMemberHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.UpdateMember(this.MethodsPtr, lobbyId, userId, transaction.MethodsPtr, GCHandle.ToIntPtr(wrapped), UpdateMemberCallbackImpl);
        transaction.MethodsPtr = IntPtr.Zero;
    }

    [MonoPInvokeCallback]
    private static void SendLobbyMessageCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                h        = GCHandle.FromIntPtr(ptr);
        SendLobbyMessageHandler callback = (SendLobbyMessageHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SendLobbyMessage(long lobbyId, byte[] data, SendLobbyMessageHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SendLobbyMessage(this.MethodsPtr, lobbyId, data, data.Length, GCHandle.ToIntPtr(wrapped), SendLobbyMessageCallbackImpl);
    }

    public LobbySearchQuery GetSearchQuery() {
        LobbySearchQuery ret = new();
        Result           res = this.Methods.GetSearchQuery(this.MethodsPtr, ref ret.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void SearchCallbackImpl(IntPtr ptr, Result result) {
        GCHandle      h        = GCHandle.FromIntPtr(ptr);
        SearchHandler callback = (SearchHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void Search(LobbySearchQuery query, SearchHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.Search(this.MethodsPtr, query.MethodsPtr, GCHandle.ToIntPtr(wrapped), SearchCallbackImpl);
        query.MethodsPtr = IntPtr.Zero;
    }

    public int LobbyCount() {
        int ret = new();
        this.Methods.LobbyCount(this.MethodsPtr, ref ret);
        return ret;
    }

    public long GetLobbyId(int index) {
        long   ret = new();
        Result res = this.Methods.GetLobbyId(this.MethodsPtr, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void ConnectVoiceCallbackImpl(IntPtr ptr, Result result) {
        GCHandle            h        = GCHandle.FromIntPtr(ptr);
        ConnectVoiceHandler callback = (ConnectVoiceHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void ConnectVoice(long lobbyId, ConnectVoiceHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ConnectVoice(this.MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), ConnectVoiceCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void DisconnectVoiceCallbackImpl(IntPtr ptr, Result result) {
        GCHandle               h        = GCHandle.FromIntPtr(ptr);
        DisconnectVoiceHandler callback = (DisconnectVoiceHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void DisconnectVoice(long lobbyId, DisconnectVoiceHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.DisconnectVoice(this.MethodsPtr, lobbyId, GCHandle.ToIntPtr(wrapped), DisconnectVoiceCallbackImpl);
    }

    public void ConnectNetwork(long lobbyId) {
        Result res = this.Methods.ConnectNetwork(this.MethodsPtr, lobbyId);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public void DisconnectNetwork(long lobbyId) {
        Result res = this.Methods.DisconnectNetwork(this.MethodsPtr, lobbyId);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public void FlushNetwork() {
        Result res = this.Methods.FlushNetwork(this.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public void OpenNetworkChannel(long lobbyId, byte channelId, bool reliable) {
        Result res = this.Methods.OpenNetworkChannel(this.MethodsPtr, lobbyId, channelId, reliable);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public void SendNetworkMessage(long lobbyId, long userId, byte channelId, byte[] data) {
        Result res = this.Methods.SendNetworkMessage(this.MethodsPtr, lobbyId, userId, channelId, data, data.Length);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    [MonoPInvokeCallback]
    private static void OnLobbyUpdateImpl(IntPtr ptr, long lobbyId) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnLobbyUpdate != null)
            d.LobbyManagerInstance.OnLobbyUpdate.Invoke(lobbyId);
    }

    [MonoPInvokeCallback]
    private static void OnLobbyDeleteImpl(IntPtr ptr, long lobbyId, uint reason) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnLobbyDelete != null)
            d.LobbyManagerInstance.OnLobbyDelete.Invoke(lobbyId, reason);
    }

    [MonoPInvokeCallback]
    private static void OnMemberConnectImpl(IntPtr ptr, long lobbyId, long userId) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnMemberConnect != null)
            d.LobbyManagerInstance.OnMemberConnect.Invoke(lobbyId, userId);
    }

    [MonoPInvokeCallback]
    private static void OnMemberUpdateImpl(IntPtr ptr, long lobbyId, long userId) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnMemberUpdate != null)
            d.LobbyManagerInstance.OnMemberUpdate.Invoke(lobbyId, userId);
    }

    [MonoPInvokeCallback]
    private static void OnMemberDisconnectImpl(IntPtr ptr, long lobbyId, long userId) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnMemberDisconnect != null)
            d.LobbyManagerInstance.OnMemberDisconnect.Invoke(lobbyId, userId);
    }

    [MonoPInvokeCallback]
    private static void OnLobbyMessageImpl(IntPtr ptr, long lobbyId, long userId, IntPtr dataPtr, int dataLen) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnLobbyMessage != null) {
            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            d.LobbyManagerInstance.OnLobbyMessage.Invoke(lobbyId, userId, data);
        }
    }

    [MonoPInvokeCallback]
    private static void OnSpeakingImpl(IntPtr ptr, long lobbyId, long userId, bool speaking) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnSpeaking != null)
            d.LobbyManagerInstance.OnSpeaking.Invoke(lobbyId, userId, speaking);
    }

    [MonoPInvokeCallback]
    private static void OnNetworkMessageImpl(IntPtr ptr, long lobbyId, long userId, byte channelId, IntPtr dataPtr, int dataLen) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.LobbyManagerInstance.OnNetworkMessage != null) {
            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            d.LobbyManagerInstance.OnNetworkMessage.Invoke(lobbyId, userId, channelId, data);
        }
    }
}

public class NetworkManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void MessageHandler(IntPtr ptr, ulong peerId, byte channelId, IntPtr dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void RouteUpdateHandler(IntPtr ptr, [MarshalAs(UnmanagedType.LPStr)] string routeData);

        internal MessageHandler OnMessage;

        internal RouteUpdateHandler OnRouteUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void GetPeerIdMethod(IntPtr methodsPtr, ref ulong peerId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result FlushMethod(IntPtr methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result OpenPeerMethod(IntPtr methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result UpdatePeerMethod(IntPtr methodsPtr, ulong peerId, [MarshalAs(UnmanagedType.LPStr)] string routeData);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result ClosePeerMethod(IntPtr methodsPtr, ulong peerId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result OpenChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId, bool reliable);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result CloseChannelMethod(IntPtr methodsPtr, ulong peerId, byte channelId);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SendMessageMethod(IntPtr methodsPtr, ulong peerId, byte channelId, byte[] data, int dataLen);

        internal GetPeerIdMethod GetPeerId;

        internal FlushMethod Flush;

        internal OpenPeerMethod OpenPeer;

        internal UpdatePeerMethod UpdatePeer;

        internal ClosePeerMethod ClosePeer;

        internal OpenChannelMethod OpenChannel;

        internal CloseChannelMethod CloseChannel;

        internal SendMessageMethod SendMessage;
    }

    public delegate void MessageHandler(ulong peerId, byte channelId, byte[] data);

    public delegate void RouteUpdateHandler(string routeData);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event MessageHandler OnMessage;

    public event RouteUpdateHandler OnRouteUpdate;

    internal NetworkManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnMessage     = OnMessageImpl;
        events.OnRouteUpdate = OnRouteUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    /// <summary>
    ///     Get the local peer ID for this process.
    /// </summary>
    public ulong GetPeerId() {
        ulong ret = new();
        this.Methods.GetPeerId(this.MethodsPtr, ref ret);
        return ret;
    }

    /// <summary>
    ///     Send pending network messages.
    /// </summary>
    public void Flush() {
        Result res = this.Methods.Flush(this.MethodsPtr);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    /// <summary>
    ///     Open a connection to a remote peer.
    /// </summary>
    public void OpenPeer(ulong peerId, string routeData) {
        Result res = this.Methods.OpenPeer(this.MethodsPtr, peerId, routeData);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    /// <summary>
    ///     Update the route data for a connected peer.
    /// </summary>
    public void UpdatePeer(ulong peerId, string routeData) {
        Result res = this.Methods.UpdatePeer(this.MethodsPtr, peerId, routeData);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    /// <summary>
    ///     Close the connection to a remote peer.
    /// </summary>
    public void ClosePeer(ulong peerId) {
        Result res = this.Methods.ClosePeer(this.MethodsPtr, peerId);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    /// <summary>
    ///     Open a message channel to a connected peer.
    /// </summary>
    public void OpenChannel(ulong peerId, byte channelId, bool reliable) {
        Result res = this.Methods.OpenChannel(this.MethodsPtr, peerId, channelId, reliable);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    /// <summary>
    ///     Close a message channel to a connected peer.
    /// </summary>
    public void CloseChannel(ulong peerId, byte channelId) {
        Result res = this.Methods.CloseChannel(this.MethodsPtr, peerId, channelId);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    /// <summary>
    ///     Send a message to a connected peer over an opened message channel.
    /// </summary>
    public void SendMessage(ulong peerId, byte channelId, byte[] data) {
        Result res = this.Methods.SendMessage(this.MethodsPtr, peerId, channelId, data, data.Length);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    [MonoPInvokeCallback]
    private static void OnMessageImpl(IntPtr ptr, ulong peerId, byte channelId, IntPtr dataPtr, int dataLen) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.NetworkManagerInstance.OnMessage != null) {
            byte[] data = new byte[dataLen];
            Marshal.Copy(dataPtr, data, 0, dataLen);
            d.NetworkManagerInstance.OnMessage.Invoke(peerId, channelId, data);
        }
    }

    [MonoPInvokeCallback]
    private static void OnRouteUpdateImpl(IntPtr ptr, string routeData) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.NetworkManagerInstance.OnRouteUpdate != null)
            d.NetworkManagerInstance.OnRouteUpdate.Invoke(routeData);
    }
}

public class OverlayManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ToggleHandler(IntPtr ptr, bool locked);

        internal ToggleHandler OnToggle;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void IsEnabledMethod(IntPtr methodsPtr, ref bool enabled);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void IsLockedMethod(IntPtr methodsPtr, ref bool locked);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetLockedCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetLockedMethod(IntPtr methodsPtr, bool locked, IntPtr callbackData, SetLockedCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OpenActivityInviteCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OpenActivityInviteMethod(IntPtr methodsPtr, ActivityActionType type, IntPtr callbackData, OpenActivityInviteCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OpenGuildInviteCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OpenGuildInviteMethod(
            IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string code, IntPtr callbackData, OpenGuildInviteCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OpenVoiceSettingsCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OpenVoiceSettingsMethod(IntPtr methodsPtr, IntPtr callbackData, OpenVoiceSettingsCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result InitDrawingDxgiMethod(IntPtr methodsPtr, IntPtr swapchain, bool useMessageForwarding);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void OnPresentMethod(IntPtr methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ForwardMessageMethod(IntPtr methodsPtr, IntPtr message);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void KeyEventMethod(IntPtr methodsPtr, bool down, [MarshalAs(UnmanagedType.LPStr)] string keyCode, KeyVariant variant);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CharEventMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string character);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void MouseButtonEventMethod(IntPtr methodsPtr, byte down, int clickCount, MouseButton which, int x, int y);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void MouseMotionEventMethod(IntPtr methodsPtr, int x, int y);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ImeCommitTextMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string text);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ImeSetCompositionMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string text, ref ImeUnderline underlines, int from, int to);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ImeCancelCompositionMethod(IntPtr methodsPtr);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetImeCompositionRangeCallbackCallback(IntPtr ptr, int from, int to, ref Rect bounds);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetImeCompositionRangeCallbackMethod(IntPtr methodsPtr, IntPtr callbackData, SetImeCompositionRangeCallbackCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetImeSelectionBoundsCallbackCallback(IntPtr ptr, Rect anchor, Rect focus, bool isAnchorFirst);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetImeSelectionBoundsCallbackMethod(IntPtr methodsPtr, IntPtr callbackData, SetImeSelectionBoundsCallbackCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate bool IsPointInsideClickZoneMethod(IntPtr methodsPtr, int x, int y);

        internal IsEnabledMethod IsEnabled;

        internal IsLockedMethod IsLocked;

        internal SetLockedMethod SetLocked;

        internal OpenActivityInviteMethod OpenActivityInvite;

        internal OpenGuildInviteMethod OpenGuildInvite;

        internal OpenVoiceSettingsMethod OpenVoiceSettings;

        internal InitDrawingDxgiMethod InitDrawingDxgi;

        internal OnPresentMethod OnPresent;

        internal ForwardMessageMethod ForwardMessage;

        internal KeyEventMethod KeyEvent;

        internal CharEventMethod CharEvent;

        internal MouseButtonEventMethod MouseButtonEvent;

        internal MouseMotionEventMethod MouseMotionEvent;

        internal ImeCommitTextMethod ImeCommitText;

        internal ImeSetCompositionMethod ImeSetComposition;

        internal ImeCancelCompositionMethod ImeCancelComposition;

        internal SetImeCompositionRangeCallbackMethod SetImeCompositionRangeCallback;

        internal SetImeSelectionBoundsCallbackMethod SetImeSelectionBoundsCallback;

        internal IsPointInsideClickZoneMethod IsPointInsideClickZone;
    }

    public delegate void SetLockedHandler(Result result);

    public delegate void OpenActivityInviteHandler(Result result);

    public delegate void OpenGuildInviteHandler(Result result);

    public delegate void OpenVoiceSettingsHandler(Result result);

    public delegate void SetImeCompositionRangeCallbackHandler(int from, int to, ref Rect bounds);

    public delegate void SetImeSelectionBoundsCallbackHandler(Rect anchor, Rect focus, bool isAnchorFirst);

    public delegate void ToggleHandler(bool locked);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event ToggleHandler OnToggle;

    internal OverlayManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnToggle = OnToggleImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public bool IsEnabled() {
        bool ret = new();
        this.Methods.IsEnabled(this.MethodsPtr, ref ret);
        return ret;
    }

    public bool IsLocked() {
        bool ret = new();
        this.Methods.IsLocked(this.MethodsPtr, ref ret);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void SetLockedCallbackImpl(IntPtr ptr, Result result) {
        GCHandle         h        = GCHandle.FromIntPtr(ptr);
        SetLockedHandler callback = (SetLockedHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SetLocked(bool locked, SetLockedHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SetLocked(this.MethodsPtr, locked, GCHandle.ToIntPtr(wrapped), SetLockedCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void OpenActivityInviteCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                  h        = GCHandle.FromIntPtr(ptr);
        OpenActivityInviteHandler callback = (OpenActivityInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void OpenActivityInvite(ActivityActionType type, OpenActivityInviteHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.OpenActivityInvite(this.MethodsPtr, type, GCHandle.ToIntPtr(wrapped), OpenActivityInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void OpenGuildInviteCallbackImpl(IntPtr ptr, Result result) {
        GCHandle               h        = GCHandle.FromIntPtr(ptr);
        OpenGuildInviteHandler callback = (OpenGuildInviteHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void OpenGuildInvite(string code, OpenGuildInviteHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.OpenGuildInvite(this.MethodsPtr, code, GCHandle.ToIntPtr(wrapped), OpenGuildInviteCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void OpenVoiceSettingsCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                 h        = GCHandle.FromIntPtr(ptr);
        OpenVoiceSettingsHandler callback = (OpenVoiceSettingsHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void OpenVoiceSettings(OpenVoiceSettingsHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.OpenVoiceSettings(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), OpenVoiceSettingsCallbackImpl);
    }

    public void InitDrawingDxgi(IntPtr swapchain, bool useMessageForwarding) {
        Result res = this.Methods.InitDrawingDxgi(this.MethodsPtr, swapchain, useMessageForwarding);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public void OnPresent() {
        this.Methods.OnPresent(this.MethodsPtr);
    }

    public void ForwardMessage(IntPtr message) {
        this.Methods.ForwardMessage(this.MethodsPtr, message);
    }

    public void KeyEvent(bool down, string keyCode, KeyVariant variant) {
        this.Methods.KeyEvent(this.MethodsPtr, down, keyCode, variant);
    }

    public void CharEvent(string character) {
        this.Methods.CharEvent(this.MethodsPtr, character);
    }

    public void MouseButtonEvent(byte down, int clickCount, MouseButton which, int x, int y) {
        this.Methods.MouseButtonEvent(this.MethodsPtr, down, clickCount, which, x, y);
    }

    public void MouseMotionEvent(int x, int y) {
        this.Methods.MouseMotionEvent(this.MethodsPtr, x, y);
    }

    public void ImeCommitText(string text) {
        this.Methods.ImeCommitText(this.MethodsPtr, text);
    }

    public void ImeSetComposition(string text, ImeUnderline underlines, int from, int to) {
        this.Methods.ImeSetComposition(this.MethodsPtr, text, ref underlines, from, to);
    }

    public void ImeCancelComposition() {
        this.Methods.ImeCancelComposition(this.MethodsPtr);
    }

    [MonoPInvokeCallback]
    private static void SetImeCompositionRangeCallbackCallbackImpl(IntPtr ptr, int from, int to, ref Rect bounds) {
        GCHandle                              h        = GCHandle.FromIntPtr(ptr);
        SetImeCompositionRangeCallbackHandler callback = (SetImeCompositionRangeCallbackHandler)h.Target;
        h.Free();
        callback(from, to, ref bounds);
    }

    public void SetImeCompositionRangeCallback(SetImeCompositionRangeCallbackHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SetImeCompositionRangeCallback(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), SetImeCompositionRangeCallbackCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void SetImeSelectionBoundsCallbackCallbackImpl(IntPtr ptr, Rect anchor, Rect focus, bool isAnchorFirst) {
        GCHandle                             h        = GCHandle.FromIntPtr(ptr);
        SetImeSelectionBoundsCallbackHandler callback = (SetImeSelectionBoundsCallbackHandler)h.Target;
        h.Free();
        callback(anchor, focus, isAnchorFirst);
    }

    public void SetImeSelectionBoundsCallback(SetImeSelectionBoundsCallbackHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SetImeSelectionBoundsCallback(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), SetImeSelectionBoundsCallbackCallbackImpl);
    }

    public bool IsPointInsideClickZone(int x, int y) => this.Methods.IsPointInsideClickZone(this.MethodsPtr, x, y);

    [MonoPInvokeCallback]
    private static void OnToggleImpl(IntPtr ptr, bool locked) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.OverlayManagerInstance.OnToggle != null)
            d.OverlayManagerInstance.OnToggle.Invoke(locked);
    }
}

public partial class StorageManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {}

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result ReadMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen, ref uint read);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ReadAsyncCallback(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ReadAsyncMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, IntPtr callbackData, ReadAsyncCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ReadAsyncPartialCallback(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void ReadAsyncPartialMethod(
            IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ulong offset, ulong length, IntPtr callbackData, ReadAsyncPartialCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result WriteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void WriteAsyncCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void WriteAsyncMethod(
            IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, byte[] data, int dataLen, IntPtr callbackData, WriteAsyncCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result DeleteMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result ExistsMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ref bool exists);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CountMethod(IntPtr methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result StatMethod(IntPtr methodsPtr, [MarshalAs(UnmanagedType.LPStr)] string name, ref FileStat stat);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result StatAtMethod(IntPtr methodsPtr, int index, ref FileStat stat);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetPathMethod(IntPtr methodsPtr, StringBuilder path);

        internal ReadMethod Read;

        internal ReadAsyncMethod ReadAsync;

        internal ReadAsyncPartialMethod ReadAsyncPartial;

        internal WriteMethod Write;

        internal WriteAsyncMethod WriteAsync;

        internal DeleteMethod Delete;

        internal ExistsMethod Exists;

        internal CountMethod Count;

        internal StatMethod Stat;

        internal StatAtMethod StatAt;

        internal GetPathMethod GetPath;
    }

    public delegate void ReadAsyncHandler(Result result, byte[] data);

    public delegate void ReadAsyncPartialHandler(Result result, byte[] data);

    public delegate void WriteAsyncHandler(Result result);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    internal StorageManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public uint Read(string name, byte[] data) {
        uint   ret = new();
        Result res = this.Methods.Read(this.MethodsPtr, name, data, data.Length, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void ReadAsyncCallbackImpl(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen) {
        GCHandle         h        = GCHandle.FromIntPtr(ptr);
        ReadAsyncHandler callback = (ReadAsyncHandler)h.Target;
        h.Free();
        byte[] data = new byte[dataLen];
        Marshal.Copy(dataPtr, data, 0, dataLen);
        callback(result, data);
    }

    public void ReadAsync(string name, ReadAsyncHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ReadAsync(this.MethodsPtr, name, GCHandle.ToIntPtr(wrapped), ReadAsyncCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void ReadAsyncPartialCallbackImpl(IntPtr ptr, Result result, IntPtr dataPtr, int dataLen) {
        GCHandle                h        = GCHandle.FromIntPtr(ptr);
        ReadAsyncPartialHandler callback = (ReadAsyncPartialHandler)h.Target;
        h.Free();
        byte[] data = new byte[dataLen];
        Marshal.Copy(dataPtr, data, 0, dataLen);
        callback(result, data);
    }

    public void ReadAsyncPartial(string name, ulong offset, ulong length, ReadAsyncPartialHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.ReadAsyncPartial(this.MethodsPtr, name, offset, length, GCHandle.ToIntPtr(wrapped), ReadAsyncPartialCallbackImpl);
    }

    public void Write(string name, byte[] data) {
        Result res = this.Methods.Write(this.MethodsPtr, name, data, data.Length);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    [MonoPInvokeCallback]
    private static void WriteAsyncCallbackImpl(IntPtr ptr, Result result) {
        GCHandle          h        = GCHandle.FromIntPtr(ptr);
        WriteAsyncHandler callback = (WriteAsyncHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void WriteAsync(string name, byte[] data, WriteAsyncHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.WriteAsync(this.MethodsPtr, name, data, data.Length, GCHandle.ToIntPtr(wrapped), WriteAsyncCallbackImpl);
    }

    public void Delete(string name) {
        Result res = this.Methods.Delete(this.MethodsPtr, name);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public bool Exists(string name) {
        bool   ret = new();
        Result res = this.Methods.Exists(this.MethodsPtr, name, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public int Count() {
        int ret = new();
        this.Methods.Count(this.MethodsPtr, ref ret);
        return ret;
    }

    public FileStat Stat(string name) {
        FileStat ret = new();
        Result   res = this.Methods.Stat(this.MethodsPtr, name, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public FileStat StatAt(int index) {
        FileStat ret = new();
        Result   res = this.Methods.StatAt(this.MethodsPtr, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public string GetPath() {
        StringBuilder ret = new(4096);
        Result        res = this.Methods.GetPath(this.MethodsPtr, ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret.ToString();
    }
}

public partial class StoreManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void EntitlementCreateHandler(IntPtr ptr, ref Entitlement entitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void EntitlementDeleteHandler(IntPtr ptr, ref Entitlement entitlement);

        internal EntitlementCreateHandler OnEntitlementCreate;

        internal EntitlementDeleteHandler OnEntitlementDelete;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchSkusCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchSkusMethod(IntPtr methodsPtr, IntPtr callbackData, FetchSkusCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CountSkusMethod(IntPtr methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetSkuMethod(IntPtr methodsPtr, long skuId, ref Sku sku);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetSkuAtMethod(IntPtr methodsPtr, int index, ref Sku sku);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchEntitlementsCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchEntitlementsMethod(IntPtr methodsPtr, IntPtr callbackData, FetchEntitlementsCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CountEntitlementsMethod(IntPtr methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetEntitlementMethod(IntPtr methodsPtr, long entitlementId, ref Entitlement entitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetEntitlementAtMethod(IntPtr methodsPtr, int index, ref Entitlement entitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result HasSkuEntitlementMethod(IntPtr methodsPtr, long skuId, ref bool hasEntitlement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void StartPurchaseCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void StartPurchaseMethod(IntPtr methodsPtr, long skuId, IntPtr callbackData, StartPurchaseCallback callback);

        internal FetchSkusMethod FetchSkus;

        internal CountSkusMethod CountSkus;

        internal GetSkuMethod GetSku;

        internal GetSkuAtMethod GetSkuAt;

        internal FetchEntitlementsMethod FetchEntitlements;

        internal CountEntitlementsMethod CountEntitlements;

        internal GetEntitlementMethod GetEntitlement;

        internal GetEntitlementAtMethod GetEntitlementAt;

        internal HasSkuEntitlementMethod HasSkuEntitlement;

        internal StartPurchaseMethod StartPurchase;
    }

    public delegate void FetchSkusHandler(Result result);

    public delegate void FetchEntitlementsHandler(Result result);

    public delegate void StartPurchaseHandler(Result result);

    public delegate void EntitlementCreateHandler(ref Entitlement entitlement);

    public delegate void EntitlementDeleteHandler(ref Entitlement entitlement);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event EntitlementCreateHandler OnEntitlementCreate;

    public event EntitlementDeleteHandler OnEntitlementDelete;

    internal StoreManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnEntitlementCreate = OnEntitlementCreateImpl;
        events.OnEntitlementDelete = OnEntitlementDeleteImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    private static void FetchSkusCallbackImpl(IntPtr ptr, Result result) {
        GCHandle         h        = GCHandle.FromIntPtr(ptr);
        FetchSkusHandler callback = (FetchSkusHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void FetchSkus(FetchSkusHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.FetchSkus(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchSkusCallbackImpl);
    }

    public int CountSkus() {
        int ret = new();
        this.Methods.CountSkus(this.MethodsPtr, ref ret);
        return ret;
    }

    public Sku GetSku(long skuId) {
        Sku    ret = new();
        Result res = this.Methods.GetSku(this.MethodsPtr, skuId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public Sku GetSkuAt(int index) {
        Sku    ret = new();
        Result res = this.Methods.GetSkuAt(this.MethodsPtr, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void FetchEntitlementsCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                 h        = GCHandle.FromIntPtr(ptr);
        FetchEntitlementsHandler callback = (FetchEntitlementsHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void FetchEntitlements(FetchEntitlementsHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.FetchEntitlements(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchEntitlementsCallbackImpl);
    }

    public int CountEntitlements() {
        int ret = new();
        this.Methods.CountEntitlements(this.MethodsPtr, ref ret);
        return ret;
    }

    public Entitlement GetEntitlement(long entitlementId) {
        Entitlement ret = new();
        Result      res = this.Methods.GetEntitlement(this.MethodsPtr, entitlementId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public Entitlement GetEntitlementAt(int index) {
        Entitlement ret = new();
        Result      res = this.Methods.GetEntitlementAt(this.MethodsPtr, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public bool HasSkuEntitlement(long skuId) {
        bool   ret = new();
        Result res = this.Methods.HasSkuEntitlement(this.MethodsPtr, skuId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void StartPurchaseCallbackImpl(IntPtr ptr, Result result) {
        GCHandle             h        = GCHandle.FromIntPtr(ptr);
        StartPurchaseHandler callback = (StartPurchaseHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void StartPurchase(long skuId, StartPurchaseHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.StartPurchase(this.MethodsPtr, skuId, GCHandle.ToIntPtr(wrapped), StartPurchaseCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void OnEntitlementCreateImpl(IntPtr ptr, ref Entitlement entitlement) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.StoreManagerInstance.OnEntitlementCreate != null)
            d.StoreManagerInstance.OnEntitlementCreate.Invoke(ref entitlement);
    }

    [MonoPInvokeCallback]
    private static void OnEntitlementDeleteImpl(IntPtr ptr, ref Entitlement entitlement) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.StoreManagerInstance.OnEntitlementDelete != null)
            d.StoreManagerInstance.OnEntitlementDelete.Invoke(ref entitlement);
    }
}

public class VoiceManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SettingsUpdateHandler(IntPtr ptr);

        internal SettingsUpdateHandler OnSettingsUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetInputModeMethod(IntPtr methodsPtr, ref InputMode inputMode);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetInputModeCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetInputModeMethod(IntPtr methodsPtr, InputMode inputMode, IntPtr callbackData, SetInputModeCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result IsSelfMuteMethod(IntPtr methodsPtr, ref bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetSelfMuteMethod(IntPtr methodsPtr, bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result IsSelfDeafMethod(IntPtr methodsPtr, ref bool deaf);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetSelfDeafMethod(IntPtr methodsPtr, bool deaf);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result IsLocalMuteMethod(IntPtr methodsPtr, long userId, ref bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetLocalMuteMethod(IntPtr methodsPtr, long userId, bool mute);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetLocalVolumeMethod(IntPtr methodsPtr, long userId, ref byte volume);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result SetLocalVolumeMethod(IntPtr methodsPtr, long userId, byte volume);

        internal GetInputModeMethod GetInputMode;

        internal SetInputModeMethod SetInputMode;

        internal IsSelfMuteMethod IsSelfMute;

        internal SetSelfMuteMethod SetSelfMute;

        internal IsSelfDeafMethod IsSelfDeaf;

        internal SetSelfDeafMethod SetSelfDeaf;

        internal IsLocalMuteMethod IsLocalMute;

        internal SetLocalMuteMethod SetLocalMute;

        internal GetLocalVolumeMethod GetLocalVolume;

        internal SetLocalVolumeMethod SetLocalVolume;
    }

    public delegate void SetInputModeHandler(Result result);

    public delegate void SettingsUpdateHandler();

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event SettingsUpdateHandler OnSettingsUpdate;

    internal VoiceManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnSettingsUpdate = OnSettingsUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    public InputMode GetInputMode() {
        InputMode ret = new();
        Result    res = this.Methods.GetInputMode(this.MethodsPtr, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void SetInputModeCallbackImpl(IntPtr ptr, Result result) {
        GCHandle            h        = GCHandle.FromIntPtr(ptr);
        SetInputModeHandler callback = (SetInputModeHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SetInputMode(InputMode inputMode, SetInputModeHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SetInputMode(this.MethodsPtr, inputMode, GCHandle.ToIntPtr(wrapped), SetInputModeCallbackImpl);
    }

    public bool IsSelfMute() {
        bool   ret = new();
        Result res = this.Methods.IsSelfMute(this.MethodsPtr, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public void SetSelfMute(bool mute) {
        Result res = this.Methods.SetSelfMute(this.MethodsPtr, mute);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public bool IsSelfDeaf() {
        bool   ret = new();
        Result res = this.Methods.IsSelfDeaf(this.MethodsPtr, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public void SetSelfDeaf(bool deaf) {
        Result res = this.Methods.SetSelfDeaf(this.MethodsPtr, deaf);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public bool IsLocalMute(long userId) {
        bool   ret = new();
        Result res = this.Methods.IsLocalMute(this.MethodsPtr, userId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public void SetLocalMute(long userId, bool mute) {
        Result res = this.Methods.SetLocalMute(this.MethodsPtr, userId, mute);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    public byte GetLocalVolume(long userId) {
        byte   ret = new();
        Result res = this.Methods.GetLocalVolume(this.MethodsPtr, userId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public void SetLocalVolume(long userId, byte volume) {
        Result res = this.Methods.SetLocalVolume(this.MethodsPtr, userId, volume);
        if (res != Result.Ok)
            throw new ResultException(res);
    }

    [MonoPInvokeCallback]
    private static void OnSettingsUpdateImpl(IntPtr ptr) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.VoiceManagerInstance.OnSettingsUpdate != null)
            d.VoiceManagerInstance.OnSettingsUpdate.Invoke();
    }
}

public class AchievementManager {
    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIEvents {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void UserAchievementUpdateHandler(IntPtr ptr, ref UserAchievement userAchievement);

        internal UserAchievementUpdateHandler OnUserAchievementUpdate;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct FFIMethods {
        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetUserAchievementCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void SetUserAchievementMethod(
            IntPtr methodsPtr, long achievementId, byte percentComplete, IntPtr callbackData, SetUserAchievementCallback callback
        );

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchUserAchievementsCallback(IntPtr ptr, Result result);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void FetchUserAchievementsMethod(IntPtr methodsPtr, IntPtr callbackData, FetchUserAchievementsCallback callback);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate void CountUserAchievementsMethod(IntPtr methodsPtr, ref int count);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetUserAchievementMethod(IntPtr methodsPtr, long userAchievementId, ref UserAchievement userAchievement);

        [UnmanagedFunctionPointer(CallingConvention.Winapi)]
        internal delegate Result GetUserAchievementAtMethod(IntPtr methodsPtr, int index, ref UserAchievement userAchievement);

        internal SetUserAchievementMethod SetUserAchievement;

        internal FetchUserAchievementsMethod FetchUserAchievements;

        internal CountUserAchievementsMethod CountUserAchievements;

        internal GetUserAchievementMethod GetUserAchievement;

        internal GetUserAchievementAtMethod GetUserAchievementAt;
    }

    public delegate void SetUserAchievementHandler(Result result);

    public delegate void FetchUserAchievementsHandler(Result result);

    public delegate void UserAchievementUpdateHandler(ref UserAchievement userAchievement);

    private readonly IntPtr MethodsPtr;

    private object MethodsStructure;

    private FFIMethods Methods {
        get {
            if (this.MethodsStructure == null)
                this.MethodsStructure = Marshal.PtrToStructure(this.MethodsPtr, typeof(FFIMethods));
            return (FFIMethods)this.MethodsStructure;
        }

    }

    public event UserAchievementUpdateHandler OnUserAchievementUpdate;

    internal AchievementManager(IntPtr ptr, IntPtr eventsPtr, ref FFIEvents events) {
        if (eventsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
        this.InitEvents(eventsPtr, ref events);
        this.MethodsPtr = ptr;
        if (this.MethodsPtr == IntPtr.Zero)
            throw new ResultException(Result.InternalError);
    }

    private void InitEvents(IntPtr eventsPtr, ref FFIEvents events) {
        events.OnUserAchievementUpdate = OnUserAchievementUpdateImpl;
        Marshal.StructureToPtr(events, eventsPtr, false);
    }

    [MonoPInvokeCallback]
    private static void SetUserAchievementCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                  h        = GCHandle.FromIntPtr(ptr);
        SetUserAchievementHandler callback = (SetUserAchievementHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void SetUserAchievement(long achievementId, byte percentComplete, SetUserAchievementHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.SetUserAchievement(this.MethodsPtr, achievementId, percentComplete, GCHandle.ToIntPtr(wrapped), SetUserAchievementCallbackImpl);
    }

    [MonoPInvokeCallback]
    private static void FetchUserAchievementsCallbackImpl(IntPtr ptr, Result result) {
        GCHandle                     h        = GCHandle.FromIntPtr(ptr);
        FetchUserAchievementsHandler callback = (FetchUserAchievementsHandler)h.Target;
        h.Free();
        callback(result);
    }

    public void FetchUserAchievements(FetchUserAchievementsHandler callback) {
        GCHandle wrapped = GCHandle.Alloc(callback);
        this.Methods.FetchUserAchievements(this.MethodsPtr, GCHandle.ToIntPtr(wrapped), FetchUserAchievementsCallbackImpl);
    }

    public int CountUserAchievements() {
        int ret = new();
        this.Methods.CountUserAchievements(this.MethodsPtr, ref ret);
        return ret;
    }

    public UserAchievement GetUserAchievement(long userAchievementId) {
        UserAchievement ret = new();
        Result          res = this.Methods.GetUserAchievement(this.MethodsPtr, userAchievementId, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    public UserAchievement GetUserAchievementAt(int index) {
        UserAchievement ret = new();
        Result          res = this.Methods.GetUserAchievementAt(this.MethodsPtr, index, ref ret);
        if (res != Result.Ok)
            throw new ResultException(res);
        return ret;
    }

    [MonoPInvokeCallback]
    private static void OnUserAchievementUpdateImpl(IntPtr ptr, ref UserAchievement userAchievement) {
        GCHandle h = GCHandle.FromIntPtr(ptr);
        Discord  d = (Discord)h.Target;
        if (d.AchievementManagerInstance.OnUserAchievementUpdate != null)
            d.AchievementManagerInstance.OnUserAchievementUpdate.Invoke(ref userAchievement);
    }
}
