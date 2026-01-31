namespace Sentinel.Core.Enums;

public enum CommandStatus
{
    Queued,
    Claimed,
    Executed,
    Failed,
    Canceled,
    Expired
}
