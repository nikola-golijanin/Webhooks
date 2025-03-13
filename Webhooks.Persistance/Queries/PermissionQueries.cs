namespace Webhooks.Persistance.Queries;

public static class PermissionQueries
{
    public static FormattableString GetUserPermissions(int userId) =>
        $"""
         SELECT 
            p.Name 
         FROM permissions p 
         INNER JOIN profiles_permissions pp ON 
            p.id = pp.permission_id 
         INNER JOIN profiles_users pu  ON 
            pp.profile_id = pu.profile_id 
         WHERE pu.user_id = {userId}
         """;
}