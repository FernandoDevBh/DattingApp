using AutoMapper;
using API.Interfaces;
using API.Extensions;
using Microsoft.AspNetCore.SignalR;
using API.DTOs;
using API.Entities;

namespace API.SignalR;

public class MessageHub : Hub
{
    private const string ReceiveMessageThread = "ReceiveMessageThread";
    private const string NewMessage = "NewMessage";
    private const string NewMessageReceived = "NewMessageReceived";
    private const string UpdatedGroup = "UpdatedGroup";
    private readonly IMessageRepository _messageRepository;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IHubContext<PresenceHub> _presenceHub;
    private readonly PresenceTracker _tracker;

    public MessageHub(IMessageRepository messageRepository,
                      IMapper mapper,
                      IUserRepository userRepository,
                      IHubContext<PresenceHub> presenceHub,
                      PresenceTracker tracker)
    {
        _userRepository = userRepository;
        _presenceHub = presenceHub;
        _tracker = tracker;
        _mapper = mapper;
        _messageRepository = messageRepository;
    }

    public override async Task OnConnectedAsync()
    {
        var username = Context.User.GetUsername();
        var httpContext = Context.GetHttpContext();
        var otherUser = httpContext.Request.Query["user"].ToString();
        var groupName = GetGroupName(username, otherUser);
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        var group = await AddToGroup(groupName);
        await Clients.Group(groupName).SendAsync(UpdatedGroup, group);

        var messages = await _messageRepository.GetMessageThread(username, otherUser);

        await Clients.Caller.SendAsync(ReceiveMessageThread, messages);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group = await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync(UpdatedGroup, group);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {
        var username = Context.User.GetUsername();

        if (username == createMessageDto.recipientUsername.ToLower())
            throw new HubException("You cannot send messages to yourself");

        var sender = await _userRepository.GetByUserNameAsync(username);
        var recipient = await _userRepository.GetByUserNameAsync(createMessageDto.recipientUsername);

        if (recipient == null) throw new HubException("Not found user");

        var message = new Message
        {
            Sender = sender,
            Recipient = recipient,
            SenderUsername = sender.UserName,
            RecipientUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
        
        var groupName = GetGroupName(sender.UserName, recipient.UserName);
        var group = await _messageRepository.GetMessageGroup(groupName);

        if(group.Connections.Any(c => c.Username == recipient.UserName))
        {
            message.DateRead = DateTime.UtcNow;
        }
        else
        {
            var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
            if(connections != null)
            {
                await _presenceHub.Clients.Clients(connections).SendAsync(NewMessageReceived,
                    new { username= sender.UserName, knowAs = sender.KnowAs ?? sender.UserName });
            }
        }

        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync())
        {            
            await Clients.Group(groupName).SendAsync(NewMessage, _mapper.Map<MessageDto>(message));
        }
    }

    private async Task<Group> AddToGroup(string groupName)
    {
        var group = await _messageRepository.GetMessageGroup(groupName);
        var connection = new Connection(Context.ConnectionId, groupName);

        if(group == null)
        {
            group = new Group(groupName);
            _messageRepository.AddGroup(group);
        }

        group.Connections.Add(connection);
        if(await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to join Group");
    }

    private async Task<Group> RemoveFromMessageGroup()
    {
        var group = await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);
        if(await _messageRepository.SaveAllAsync()) return group;

        throw new HubException("Failed to remove from group");
    }

    private string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";

    }
}
