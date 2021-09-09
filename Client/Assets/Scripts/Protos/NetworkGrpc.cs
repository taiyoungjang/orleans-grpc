// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: network.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace game {
  public static partial class PlayerNetwork
  {
    static readonly string __ServiceName = "playerNetwork.PlayerNetwork";

    static readonly grpc::Marshaller<global::game.AuthRequest> __Marshaller_playerNetwork_AuthRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.AuthRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.UUID> __Marshaller_playerNetwork_UUID = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.UUID.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.PlayerData> __Marshaller_playerNetwork_PlayerData = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.PlayerData.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.AddPointRequest> __Marshaller_playerNetwork_AddPointRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.AddPointRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.AddPointResponse> __Marshaller_playerNetwork_AddPointResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.AddPointResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.GrpcStreamResponse> __Marshaller_playerNetwork_GrpcStreamResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.GrpcStreamResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.JoinRequest> __Marshaller_playerNetwork_JoinRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.JoinRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.JoinResponse> __Marshaller_playerNetwork_JoinResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.JoinResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.ChatRequest> __Marshaller_playerNetwork_ChatRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.ChatRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.ChatResponse> __Marshaller_playerNetwork_ChatResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.ChatResponse.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::game.RoomList> __Marshaller_playerNetwork_RoomList = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::game.RoomList.Parser.ParseFrom);

    static readonly grpc::Method<global::game.AuthRequest, global::game.UUID> __Method_GetAuth = new grpc::Method<global::game.AuthRequest, global::game.UUID>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetAuth",
        __Marshaller_playerNetwork_AuthRequest,
        __Marshaller_playerNetwork_UUID);

    static readonly grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.PlayerData> __Method_GetPlayerData = new grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.PlayerData>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetPlayerData",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_playerNetwork_PlayerData);

    static readonly grpc::Method<global::game.AddPointRequest, global::game.AddPointResponse> __Method_AddPoint = new grpc::Method<global::game.AddPointRequest, global::game.AddPointResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "AddPoint",
        __Marshaller_playerNetwork_AddPointRequest,
        __Marshaller_playerNetwork_AddPointResponse);

    static readonly grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.GrpcStreamResponse> __Method_GetAsyncStreams = new grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.GrpcStreamResponse>(
        grpc::MethodType.ServerStreaming,
        __ServiceName,
        "GetAsyncStreams",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_playerNetwork_GrpcStreamResponse);

    static readonly grpc::Method<global::game.JoinRequest, global::game.JoinResponse> __Method_Join = new grpc::Method<global::game.JoinRequest, global::game.JoinResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Join",
        __Marshaller_playerNetwork_JoinRequest,
        __Marshaller_playerNetwork_JoinResponse);

    static readonly grpc::Method<global::game.ChatRequest, global::game.ChatResponse> __Method_Chat = new grpc::Method<global::game.ChatRequest, global::game.ChatResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Chat",
        __Marshaller_playerNetwork_ChatRequest,
        __Marshaller_playerNetwork_ChatResponse);

    static readonly grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.RoomList> __Method_GetAvailableRoomList = new grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.RoomList>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetAvailableRoomList",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_playerNetwork_RoomList);

    static readonly grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.RoomList> __Method_GetJoinedRoomList = new grpc::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::game.RoomList>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetJoinedRoomList",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_playerNetwork_RoomList);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::game.NetworkReflection.Descriptor.Services[0]; }
    }

    /// <summary>Client for PlayerNetwork</summary>
    public partial class PlayerNetworkClient : grpc::ClientBase<PlayerNetworkClient>
    {
      /// <summary>Creates a new client for PlayerNetwork</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public PlayerNetworkClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for PlayerNetwork that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public PlayerNetworkClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected PlayerNetworkClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected PlayerNetworkClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::game.UUID GetAuth(global::game.AuthRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAuth(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.UUID GetAuth(global::game.AuthRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetAuth, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.UUID> GetAuthAsync(global::game.AuthRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAuthAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.UUID> GetAuthAsync(global::game.AuthRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetAuth, null, options, request);
      }
      public virtual global::game.PlayerData GetPlayerData(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetPlayerData(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.PlayerData GetPlayerData(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetPlayerData, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.PlayerData> GetPlayerDataAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetPlayerDataAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.PlayerData> GetPlayerDataAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetPlayerData, null, options, request);
      }
      public virtual global::game.AddPointResponse AddPoint(global::game.AddPointRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return AddPoint(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.AddPointResponse AddPoint(global::game.AddPointRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_AddPoint, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.AddPointResponse> AddPointAsync(global::game.AddPointRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return AddPointAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.AddPointResponse> AddPointAsync(global::game.AddPointRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_AddPoint, null, options, request);
      }
      public virtual grpc::AsyncServerStreamingCall<global::game.GrpcStreamResponse> GetAsyncStreams(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAsyncStreams(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncServerStreamingCall<global::game.GrpcStreamResponse> GetAsyncStreams(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncServerStreamingCall(__Method_GetAsyncStreams, null, options, request);
      }
      public virtual global::game.JoinResponse Join(global::game.JoinRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Join(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.JoinResponse Join(global::game.JoinRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Join, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.JoinResponse> JoinAsync(global::game.JoinRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return JoinAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.JoinResponse> JoinAsync(global::game.JoinRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Join, null, options, request);
      }
      public virtual global::game.ChatResponse Chat(global::game.ChatRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Chat(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.ChatResponse Chat(global::game.ChatRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Chat, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.ChatResponse> ChatAsync(global::game.ChatRequest request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ChatAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.ChatResponse> ChatAsync(global::game.ChatRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Chat, null, options, request);
      }
      public virtual global::game.RoomList GetAvailableRoomList(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAvailableRoomList(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.RoomList GetAvailableRoomList(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetAvailableRoomList, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.RoomList> GetAvailableRoomListAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetAvailableRoomListAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.RoomList> GetAvailableRoomListAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetAvailableRoomList, null, options, request);
      }
      public virtual global::game.RoomList GetJoinedRoomList(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetJoinedRoomList(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::game.RoomList GetJoinedRoomList(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetJoinedRoomList, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::game.RoomList> GetJoinedRoomListAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return GetJoinedRoomListAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::game.RoomList> GetJoinedRoomListAsync(global::Google.Protobuf.WellKnownTypes.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetJoinedRoomList, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override PlayerNetworkClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new PlayerNetworkClient(configuration);
      }
    }

  }
}
#endregion