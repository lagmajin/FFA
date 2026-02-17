using System.Collections.Concurrent;
using FFA.Models;

namespace FFA.Services;

public class ChatService
{
    private static int _nextId = 1;
    private readonly object _lock = new();
    
    // チャンネルごとのメッセージ保存（スレッドセーフ）
    private readonly ConcurrentQueue<ChatMessage> _worldMessages = new();
    private readonly ConcurrentQueue<ChatMessage> _guildMessages = new();
    private readonly ConcurrentDictionary<string, ConcurrentQueue<ChatMessage>> _whisperMessages = new();
    
    private const int MaxMessagesPerChannel = 100;
    private const int MaxWhisperMessages = 50;

    public ChatService()
    {
    }

    // 世界チャットにメッセージを送信
    public ChatMessage SendWorldMessage(string username, string message)
    {
        var chatMessage = new ChatMessage
        {
            Id = Interlocked.Increment(ref _nextId),
            Username = username,
            Message = message,
            Channel = ChatChannel.World,
            Timestamp = DateTime.Now
        };

        lock (_lock)
        {
            _worldMessages.Enqueue(chatMessage);
            while (_worldMessages.Count > MaxMessagesPerChannel)
            {
                _worldMessages.TryDequeue(out _);
            }
        }

        return chatMessage;
    }

    // ギルドチャットにメッセージを送信
    public ChatMessage SendGuildMessage(string username, string guildName, string message)
    {
        var chatMessage = new ChatMessage
        {
            Id = Interlocked.Increment(ref _nextId),
            Username = username,
            Message = message,
            Channel = ChatChannel.Guild,
            Timestamp = DateTime.Now,
            GuildName = guildName
        };

        lock (_lock)
        {
            _guildMessages.Enqueue(chatMessage);
            while (_guildMessages.Count > MaxMessagesPerChannel)
            {
                _guildMessages.TryDequeue(out _);
            }
        }

        return chatMessage;
    }

    // 個人チャット（耳打ち）を送信
    public ChatMessage? SendWhisper(string fromUsername, string toUsername, string message)
    {
        // 受信者のメッセージキューを取得または作成
        var toQueue = _whisperMessages.GetOrAdd(toUsername, _ => new ConcurrentQueue<ChatMessage>());
        var fromQueue = _whisperMessages.GetOrAdd(fromUsername, _ => new ConcurrentQueue<ChatMessage>());

        var chatMessage = new ChatMessage
        {
            Id = Interlocked.Increment(ref _nextId),
            Username = fromUsername,
            Message = message,
            Channel = ChatChannel.Whisper,
            Timestamp = DateTime.Now,
            TargetUsername = toUsername
        };

        lock (_lock)
        {
            toQueue.Enqueue(chatMessage);
            fromQueue.Enqueue(chatMessage);

            while (toQueue.Count > MaxWhisperMessages)
            {
                toQueue.TryDequeue(out _);
            }
            while (fromQueue.Count > MaxWhisperMessages)
            {
                fromQueue.TryDequeue(out _);
            }
        }

        return chatMessage;
    }

    // システムメッセージを送信
    public ChatMessage SendSystemMessage(string message)
    {
        var chatMessage = new ChatMessage
        {
            Id = Interlocked.Increment(ref _nextId),
            Username = "システム",
            Message = message,
            Channel = ChatChannel.System,
            Timestamp = DateTime.Now
        };

        lock (_lock)
        {
            _worldMessages.Enqueue(chatMessage);
            _guildMessages.Enqueue(chatMessage);
        }

        return chatMessage;
    }

    // 世界チャットメッセージを取得
    public List<ChatMessage> GetWorldMessages(int limit = 50)
    {
        return _worldMessages.TakeLast(limit).ToList();
    }

    // ギルドチャットメッセージを取得
    public List<ChatMessage> GetGuildMessages(int limit = 50)
    {
        return _guildMessages.TakeLast(limit).ToList();
    }

    // 特定のユーザーの個人チャットメッセージを取得
    public List<ChatMessage> GetWhisperMessages(string username)
    {
        if (_whisperMessages.TryGetValue(username, out var queue))
        {
            return queue.TakeLast(MaxWhisperMessages).ToList();
        }
        return new List<ChatMessage>();
    }

    // ユーザーの未読メッセージ数を取得（簡易実装）
    public int GetUnreadWhisperCount(string username)
    {
        if (_whisperMessages.TryGetValue(username, out var queue))
        {
            // 簡易実装として、全メッセージを返す
            return queue.Count;
        }
        return 0;
    }

    // ユーザーチャットをクリア（ログアウト時など）
    public void ClearUserWhispers(string username)
    {
        _whisperMessages.TryRemove(username, out _);
    }
}
