using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers;

namespace ResourcePlanningAccelist.Contracts.RequestModels.ManageUsers
{
    public class UpdateUserPasswordRequest : IRequest<UpdateUserPasswordResponse>
    {
        public Guid UserId { get; set; }
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
