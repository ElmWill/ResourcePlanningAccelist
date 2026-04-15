using System;
using System.Collections.Generic;
using System.Text;

namespace ResourcePlanningAccelist.Contracts.ResponseModels.ManageUsers
{
    public class UpdateUserPasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
