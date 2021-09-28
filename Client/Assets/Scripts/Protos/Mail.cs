// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: mail.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace game {

  /// <summary>Holder for reflection information generated from mail.proto</summary>
  public static partial class MailReflection {

    #region Descriptor
    /// <summary>File descriptor for mail.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static MailReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "CgptYWlsLnByb3RvGh9nb29nbGUvcHJvdG9idWYvdGltZXN0YW1wLnByb3Rv",
            "GgplbnVtLnByb3RvGgp1dWlkLnByb3RvIkYKDk1haWxSZXdhcmREYXRhEgoK",
            "AmlkGAEgASgFEhkKBHR5cGUYAiABKA4yCy5SZXdhcmRUeXBlEg0KBWNvdW50",
            "GAMgASgFIoABCghNYWlsRGF0YRITCgR1dWlkGAEgASgLMgUuVVVJRBIPCgdt",
            "ZXNzYWdlGAIgASgJEiAKB3Jld2FyZHMYAyADKAsyDy5NYWlsUmV3YXJkRGF0",
            "YRIsCghzZW5kRGF0ZRgEIAEoCzIaLmdvb2dsZS5wcm90b2J1Zi5UaW1lc3Rh",
            "bXAicAoMTWFpbExpc3REYXRhEicKBW1haWxzGAEgAygLMhguTWFpbExpc3RE",
            "YXRhLk1haWxzRW50cnkaNwoKTWFpbHNFbnRyeRILCgNrZXkYASABKAkSGAoF",
            "dmFsdWUYAiABKAsyCS5NYWlsRGF0YToCOAFCB6oCBGdhbWViBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { global::Google.Protobuf.WellKnownTypes.TimestampReflection.Descriptor, global::game.EnumReflection.Descriptor, global::game.UuidReflection.Descriptor, },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::game.MailRewardData), global::game.MailRewardData.Parser, new[]{ "Id", "Type", "Count" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::game.MailData), global::game.MailData.Parser, new[]{ "Uuid", "Message", "Rewards", "SendDate" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::game.MailListData), global::game.MailListData.Parser, new[]{ "Mails" }, null, null, null, new pbr::GeneratedClrTypeInfo[] { null, })
          }));
    }
    #endregion

  }
  #region Messages
  public sealed partial class MailRewardData : pb::IMessage<MailRewardData> {
    private static readonly pb::MessageParser<MailRewardData> _parser = new pb::MessageParser<MailRewardData>(() => new MailRewardData());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MailRewardData> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::game.MailReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailRewardData() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailRewardData(MailRewardData other) : this() {
      id_ = other.id_;
      type_ = other.type_;
      count_ = other.count_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailRewardData Clone() {
      return new MailRewardData(this);
    }

    /// <summary>Field number for the "id" field.</summary>
    public const int IdFieldNumber = 1;
    private int id_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Id {
      get { return id_; }
      set {
        id_ = value;
      }
    }

    /// <summary>Field number for the "type" field.</summary>
    public const int TypeFieldNumber = 2;
    private global::game.RewardType type_ = global::game.RewardType.None;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::game.RewardType Type {
      get { return type_; }
      set {
        type_ = value;
      }
    }

    /// <summary>Field number for the "count" field.</summary>
    public const int CountFieldNumber = 3;
    private int count_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int Count {
      get { return count_; }
      set {
        count_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MailRewardData);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MailRewardData other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Id != other.Id) return false;
      if (Type != other.Type) return false;
      if (Count != other.Count) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Id != 0) hash ^= Id.GetHashCode();
      if (Type != global::game.RewardType.None) hash ^= Type.GetHashCode();
      if (Count != 0) hash ^= Count.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Id != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(Id);
      }
      if (Type != global::game.RewardType.None) {
        output.WriteRawTag(16);
        output.WriteEnum((int) Type);
      }
      if (Count != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(Count);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Id != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Id);
      }
      if (Type != global::game.RewardType.None) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Type);
      }
      if (Count != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(Count);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MailRewardData other) {
      if (other == null) {
        return;
      }
      if (other.Id != 0) {
        Id = other.Id;
      }
      if (other.Type != global::game.RewardType.None) {
        Type = other.Type;
      }
      if (other.Count != 0) {
        Count = other.Count;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Id = input.ReadInt32();
            break;
          }
          case 16: {
            Type = (global::game.RewardType) input.ReadEnum();
            break;
          }
          case 24: {
            Count = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  public sealed partial class MailData : pb::IMessage<MailData> {
    private static readonly pb::MessageParser<MailData> _parser = new pb::MessageParser<MailData>(() => new MailData());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MailData> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::game.MailReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailData() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailData(MailData other) : this() {
      uuid_ = other.uuid_ != null ? other.uuid_.Clone() : null;
      message_ = other.message_;
      rewards_ = other.rewards_.Clone();
      sendDate_ = other.sendDate_ != null ? other.sendDate_.Clone() : null;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailData Clone() {
      return new MailData(this);
    }

    /// <summary>Field number for the "uuid" field.</summary>
    public const int UuidFieldNumber = 1;
    private global::game.UUID uuid_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::game.UUID Uuid {
      get { return uuid_; }
      set {
        uuid_ = value;
      }
    }

    /// <summary>Field number for the "message" field.</summary>
    public const int MessageFieldNumber = 2;
    private string message_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Message {
      get { return message_; }
      set {
        message_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "rewards" field.</summary>
    public const int RewardsFieldNumber = 3;
    private static readonly pb::FieldCodec<global::game.MailRewardData> _repeated_rewards_codec
        = pb::FieldCodec.ForMessage(26, global::game.MailRewardData.Parser);
    private readonly pbc::RepeatedField<global::game.MailRewardData> rewards_ = new pbc::RepeatedField<global::game.MailRewardData>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::game.MailRewardData> Rewards {
      get { return rewards_; }
    }

    /// <summary>Field number for the "sendDate" field.</summary>
    public const int SendDateFieldNumber = 4;
    private global::Google.Protobuf.WellKnownTypes.Timestamp sendDate_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::Google.Protobuf.WellKnownTypes.Timestamp SendDate {
      get { return sendDate_; }
      set {
        sendDate_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MailData);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MailData other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!object.Equals(Uuid, other.Uuid)) return false;
      if (Message != other.Message) return false;
      if(!rewards_.Equals(other.rewards_)) return false;
      if (!object.Equals(SendDate, other.SendDate)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (uuid_ != null) hash ^= Uuid.GetHashCode();
      if (Message.Length != 0) hash ^= Message.GetHashCode();
      hash ^= rewards_.GetHashCode();
      if (sendDate_ != null) hash ^= SendDate.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (uuid_ != null) {
        output.WriteRawTag(10);
        output.WriteMessage(Uuid);
      }
      if (Message.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Message);
      }
      rewards_.WriteTo(output, _repeated_rewards_codec);
      if (sendDate_ != null) {
        output.WriteRawTag(34);
        output.WriteMessage(SendDate);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (uuid_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(Uuid);
      }
      if (Message.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Message);
      }
      size += rewards_.CalculateSize(_repeated_rewards_codec);
      if (sendDate_ != null) {
        size += 1 + pb::CodedOutputStream.ComputeMessageSize(SendDate);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MailData other) {
      if (other == null) {
        return;
      }
      if (other.uuid_ != null) {
        if (uuid_ == null) {
          Uuid = new global::game.UUID();
        }
        Uuid.MergeFrom(other.Uuid);
      }
      if (other.Message.Length != 0) {
        Message = other.Message;
      }
      rewards_.Add(other.rewards_);
      if (other.sendDate_ != null) {
        if (sendDate_ == null) {
          SendDate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
        }
        SendDate.MergeFrom(other.SendDate);
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            if (uuid_ == null) {
              Uuid = new global::game.UUID();
            }
            input.ReadMessage(Uuid);
            break;
          }
          case 18: {
            Message = input.ReadString();
            break;
          }
          case 26: {
            rewards_.AddEntriesFrom(input, _repeated_rewards_codec);
            break;
          }
          case 34: {
            if (sendDate_ == null) {
              SendDate = new global::Google.Protobuf.WellKnownTypes.Timestamp();
            }
            input.ReadMessage(SendDate);
            break;
          }
        }
      }
    }

  }

  public sealed partial class MailListData : pb::IMessage<MailListData> {
    private static readonly pb::MessageParser<MailListData> _parser = new pb::MessageParser<MailListData>(() => new MailListData());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MailListData> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::game.MailReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailListData() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailListData(MailListData other) : this() {
      mails_ = other.mails_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MailListData Clone() {
      return new MailListData(this);
    }

    /// <summary>Field number for the "mails" field.</summary>
    public const int MailsFieldNumber = 1;
    private static readonly pbc::MapField<string, global::game.MailData>.Codec _map_mails_codec
        = new pbc::MapField<string, global::game.MailData>.Codec(pb::FieldCodec.ForString(10, ""), pb::FieldCodec.ForMessage(18, global::game.MailData.Parser), 10);
    private readonly pbc::MapField<string, global::game.MailData> mails_ = new pbc::MapField<string, global::game.MailData>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::MapField<string, global::game.MailData> Mails {
      get { return mails_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MailListData);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MailListData other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (!Mails.Equals(other.Mails)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= Mails.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      mails_.WriteTo(output, _map_mails_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      size += mails_.CalculateSize(_map_mails_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MailListData other) {
      if (other == null) {
        return;
      }
      mails_.Add(other.mails_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            mails_.AddEntriesFrom(input, _map_mails_codec);
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code