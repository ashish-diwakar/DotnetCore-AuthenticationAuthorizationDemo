using Microsoft.AspNetCore.Authorization;

namespace SecurityDemo.Authorization
{
    public class HrManagerProbationRequirement : IAuthorizationRequirement
    {
        public HrManagerProbationRequirement(int requiredMonths) {
            RequiredMonths = requiredMonths;
        }

        public int RequiredMonths { get; }
    }

    public class HrManagerProbationHandler : AuthorizationHandler<HrManagerProbationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HrManagerProbationRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "EmploymentDate"))
            {
                var employmentDateClaim = context.User.FindFirst(c => c.Type == "EmploymentDate")?.Value;
                if (employmentDateClaim != null &&
                    DateTime.TryParse(employmentDateClaim, out DateTime employmentDate)
                )
                {
                    var monthsEmployed = (DateTime.Now - employmentDate).Days / 30;
                    if (monthsEmployed >= requirement.RequiredMonths)
                        context.Succeed(requirement);
                }

            }
            return Task.CompletedTask;
        }
    }
}
