using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
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

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId); 
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(q => q.Connections)
                .Where(q => q.Connections.Any(q => q.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
                .Include(q => q.Sender)
                .Include(q => q.Recipient)
                .SingleOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Group> GetMessageGroup(string groupname)
        {
            return await _context.Groups
                .Include(q => q.Connections)
                .FirstOrDefaultAsync(q => q.Name == groupname);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(q => q.MessageSent)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(q => q.Recipient.UserName == messageParams.Username 
                    && q.RecipientDeleted == false),
                "Outbox" => query.Where(q => q.Sender.UserName == messageParams.Username 
                    && q.SenderDeleted == false),
                _ => query.Where(q => q.Recipient.UserName == messageParams.Username 
                    && q.RecipientDeleted == false
                    && q.DateRead == null)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThred(string currentUsername, string recipientUsername)
        {
            var messages = await _context.Messages
                .Include(q => q.Sender).ThenInclude(q => q.Photos)
                .Include(q => q.Recipient).ThenInclude(q => q.Photos)
                .Where(q => q.Recipient.UserName == currentUsername
                        && q.Sender.UserName == recipientUsername
                        && q.RecipientDeleted == false
                        || q.Recipient.UserName == recipientUsername
                        && q.Sender.UserName == currentUsername
                        && q.SenderDeleted == false
                )
                .OrderBy(q => q.MessageSent)
                .ToListAsync();

            var unreadMessages = messages
                .Where(q => q.DateRead == null && q.Recipient.UserName == currentUsername)
                .ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
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

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}