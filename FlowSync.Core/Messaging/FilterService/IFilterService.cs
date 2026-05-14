namespace FlowSync.Core.Messaging.FilterService;

public interface IFilterService
{
    void AddFilter(string FlowSyncId);
    string Filter(string obj);
}