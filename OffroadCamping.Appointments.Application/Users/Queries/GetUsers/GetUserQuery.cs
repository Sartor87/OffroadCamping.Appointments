using MediatR;
using OffroadCamping.Appointments.Application.Dtos.Security;

namespace OffroadCamping.Appointments.Application.Users.Queries.GetUsers
{
    public class GetUserQuery : IRequest<UserDto>
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public GetUserQuery(Guid userId, string userName, string userEmail)
        {
            UserId = userId;
            UserName = userName;
            UserEmail = userEmail;
        }
    }
}
