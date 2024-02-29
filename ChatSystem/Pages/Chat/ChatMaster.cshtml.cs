using AutoMapper;
using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;
using Repository.DTOs;

namespace ChatSystem.Pages.Chat
{
    [Authorize]
    public class ChatMasterModel : PageModel
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        private readonly IUserRepository _userRepository;

        public ChatMasterModel(IConversationRepository conversationRepository,
            IParticipantRepository participantRepository,
            IUserRepository userRepository,
            IMapper mapper, IMessageRepository messageRepository)
        {
            _conversationRepository = conversationRepository;
            _participantRepository = participantRepository;
            _conversationRepository = conversationRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _messageRepository = messageRepository;
        }

        public List<UserDto> GroupChatParticipants
        { get; set; }
        public Conversation currentConversation { get; set; }
        public UserDto UserDto { get; set; }

        [BindProperty(SupportsGet = true)]
        public List<ConversationDto> ConversationDtoList { get; set; } = default!;

        [BindProperty]
        public ConversationDto conversationDto { get; set; }

        [BindProperty]
        public List<MessageDto> MessageDtoList { get; set; } = default!;

        //private Dictionary<int, UserDto> UserDtoDictionary { get; set; }

        public IActionResult OnGet()
        {
            try
            {
                var idClaim = User.Claims.FirstOrDefault(claims => claims.Type == "UserId", null);

                if (idClaim != null)
                {
                    ConversationDtoList = new List<ConversationDto>();
                    int userId = int.Parse(idClaim.Value);

                    List<Conversation> conversationList = _conversationRepository.GetAllUserConversation(userId);
                    List<Conversation> conversationOrderList = conversationList.OrderByDescending(c => _messageRepository.GetLastestMessageFromConversation(c).DateSend.Ticks).ThenByDescending(c => c.CreateAt.Ticks).ToList();
                    //List<Conversation> conversationOrderList = conversationList.OrderByDescending(c => c.CreateAt.Ticks).ToList();

                    foreach (var conversation in conversationOrderList)
                    {
                        ConversationDto conversationDto = MapConversationToDto(conversation, userId);

                        ConversationDtoList.Add(conversationDto);
                    }

                    var conversationIdParam = Request.Query["id"];
                    int conversationId = int.Parse(conversationIdParam);

                    if (conversationId != null)
                    {
                        LoadConversation((int)conversationId);
                    }
                    return Page();
                }
                return Page();

            }
            catch (Exception ex)
            {
                return Page();
            }

        }

        public IActionResult LoadConversation(int conversationId)
        {
            currentConversation = _conversationRepository.GetConversationById(conversationId);

            var idClaim = User.Claims.FirstOrDefault(claims => claims.Type == "UserId", null);
            int userId = int.Parse(idClaim.Value);
            if (currentConversation == null)
            {
                return Page();
            }
            if (!_conversationRepository.IsUserInConversation(conversationId, userId))
            {
                return Page();
            }
            UserDto = _userRepository.GetUserDtoWithPhoto(userId);
            if (UserDto == null)
            {
                return NotFound();
            }

            GetConversationDetail(conversationId);

            conversationDto = MapConversationToDto(currentConversation, UserDto.UserId);

            MessageDtoList = _messageRepository.GetMessagesFromConversation(currentConversation, GroupChatParticipants);


            return Page();
        }


        //public IActionResult OnPostLoadConversation()
        //{
        //    int conversationId = conversationDto.ConversationId;

        //    currentConversation = _conversationRepository.GetConversationById(conversationId);

        //    var idClaim = User.Claims.FirstOrDefault(claims => claims.Type == "UserId", null);
        //    int userId = int.Parse(idClaim.Value);

        //    var user = _userRepository.GetUserWithPhoto(userId);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }
        //    if (user != null)
        //    {
        //        UserDto = new UserDto
        //        {
        //            UserId = user.UserId,
        //            UserName = user.UserName,
        //            DateOfBirth = user.DateOfBirth,
        //            KnownAs = user.KnownAs,
        //            Gender = user.Gender,
        //            Introduction = user.Introduction,
        //            Interest = user.Interest,
        //            City = user.City,
        //            Avatar = user.photos.FirstOrDefault(p => p.isMain)?.PhotoUrl
        //        };
        //    }

        //    if (currentConversation != null)
        //    {
        //        GetConversationDetail(conversationId);

        //        conversationDto = MapConversationToDto(currentConversation, UserDto.UserId);
        //    }

        //    return OnGet(null);
        //}

        private ConversationDto MapConversationToDto(Conversation conversation, int userId)
        {
            ConversationDto conversationDto = _mapper.Map<Conversation, ConversationDto>(conversation);
            if (!conversationDto.isGroup)
            {
                User otherUser = _participantRepository.GetOtherParticipant(conversationDto.ConversationId, userId);
                conversationDto.OtherUserName = otherUser.KnownAs;
                conversationDto.OtherUserId = otherUser.UserId;
                conversationDto.Avatar = otherUser.photos.FirstOrDefault(p => p.isMain)?.PhotoUrl;
            }

            return conversationDto;
        }

        //private MessageDto MapMessageToDto(Message message)
        //{
        //    MessageDto messageDto = _mapper.Map<Message, MessageDto>(message);
        //    if (messageDto != null)
        //    {

        //    }
        //}


        private void GetConversationDetail(int conversationId)
        {
            GroupChatParticipants = _userRepository.GetUserInGroupChat(conversationId);
            currentConversation = _conversationRepository.GetConversationById(conversationId);
        }

    }
}
