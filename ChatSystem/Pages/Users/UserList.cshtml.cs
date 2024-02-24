using BusinessObject;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Repository;

namespace ChatSystem.Pages.Users
{
    public class UserListModel : PageModel
    {
        private readonly IUserRepository _userRepository;
        private readonly IFriendRepository _friendRepository;
        public IEnumerable<UserProfile> Users { get; set; }

        public UserListModel(IUserRepository userRepository, IFriendRepository friendRepository)
        {
            _userRepository = userRepository;
            _friendRepository = friendRepository;
            Users = new List<UserProfile>();
        }

        public void OnGet()
        {
            var idClaim = User.Claims.FirstOrDefault(claims => claims.Type == "UserId", null);

            IEnumerable<User> users;
            if (idClaim != null)
            {
                int userId = int.Parse(idClaim.Value);
                users = _userRepository.GetUsers().Where(u => u.UserId != userId);
            }
            else
            {
                users = _userRepository.GetUsers();
            }

            Users = users.Select(user => new UserProfile
            {
                UserId = user.UserId,
                UserName = user.UserName,
                DateOfBirth = user.DateOfBirth,
                KnownAs = user.KnownAs,
                Gender = user.Gender,
                Introduction = user.Introduction,
                Interest = user.Interest,
                City = user.City,
                Avatar = user.photos.FirstOrDefault(p => p.isMain)?.PhotoUrl
            }).ToList();
        }

        public async Task<IActionResult> OnPost(int userId)
        {

            int? currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId != null)
            {
                var senderId = (int)currentUserId;
                var recipientUserName = _userRepository.GetUser(userId).UserName;
                var senderUserName = _userRepository.GetUser(senderId).UserName;
                await _friendRepository.SendFriendRequest(senderId, userId, senderUserName, recipientUserName);
            }
            return RedirectToPage("/Users/UserList");
        }
    }
}
