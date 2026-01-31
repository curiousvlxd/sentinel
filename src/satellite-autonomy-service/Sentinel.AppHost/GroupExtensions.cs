namespace Sentinel.AppHost;

public static class GroupExtensions
{
    public static IResourceBuilder<GroupResource> AddGroup(this IDistributedApplicationBuilder builder, string name)
    {
        var resource = new GroupResource(name);
        var groupInitialSnapshot = new CustomResourceSnapshot
        {
            ResourceType = "Group",
            Properties = [],
            State = new ResourceStateSnapshot(KnownResourceStates.Running, KnownResourceStateStyles.Success),
            StartTimeStamp = DateTime.UtcNow
        };
        return builder.AddResource(resource).WithInitialState(groupInitialSnapshot);
    }
}
