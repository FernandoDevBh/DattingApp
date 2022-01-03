using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class MessageRepository : IMessageRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public MessageRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void AddGroup(Group group)
    {
        _context.Groups.Add(group);
    }

    public void AddMessage(Message message) =>
        _context.Messages.Add(message);

    public void DeleteMessage(Message message) =>
        _context.Messages.Remove(message);

    public async Task<Connection> GetConnection(string connectionId)
    {
        return await _context.Connections.FindAsync(connectionId);
    }

    public async Task<Group> GetGroupForConnection(string connectionId)
    {
        return await _context
                        .Groups
                        .Include(g => g.Connections)
                        .Where(g => g.Connections.Any(c => c.ConnectionId == connectionId))
                        .FirstOrDefaultAsync();
    }

    public async Task<Message> GetMessage(int id) =>
        await _context
                .Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .SingleOrDefaultAsync(m => m.Id == id);

    public async Task<Group> GetMessageGroup(string groupName)
    {
        return await _context
                        .Groups
                        .Include(g => g.Connections)
                        .FirstOrDefaultAsync(g => g.Name == groupName);
    }

    public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
    {
        var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();
        query = messageParams.Container switch
        {
            "Inbox" => query.Where(u => u.Recipient.UserName == messageParams.Username &&
                                        !u.RecipientDeleted),
            "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username &&
                                         !u.SenderDeleted),
            _ => query.Where(u =>u.Recipient.UserName == messageParams.Username &&
                                 !u.RecipientDeleted &&
                                 u.DateRead == null),
        };

        var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

        return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
    }

    public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUsername)
    {
        var messages = await _context
                                .Messages
                                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                                .Where(m => m.Recipient.UserName == currentUserName &&
                                            !m.RecipientDeleted &&
                                            m.Sender.UserName == recipientUsername ||
                                            m.Recipient.UserName == recipientUsername &&
                                            m.Sender.UserName == currentUserName &&
                                            !m.SenderDeleted
                                )
                                .OrderBy(m => m.MessageSent)
                                .ToListAsync();

        var unreadMessages = messages
                                .Where(m => m.DateRead == null && m.Recipient.UserName == currentUserName)
                                .ToList();
                                
        if(unreadMessages.Any())
        {
            foreach(var message in unreadMessages)
            {
                message.DateRead = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();
        }

        return _mapper.Map<IEnumerable<MessageDto>>(messages);
    }

    public void RemoveConnection(Connection connection)
    {
        _context.Connections.Remove(connection);
    }

    public async Task<bool> SaveAllAsync() =>
        await _context.SaveChangesAsync() > 0;
}   
