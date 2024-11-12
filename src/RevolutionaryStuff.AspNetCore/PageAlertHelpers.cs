using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace RevolutionaryStuff.AspNetCore;

public static class PageAlertHelpers
{
    private const string PageAlertsKey = "PageAlerts";

    public static void AddPageAlert(this ITempDataDictionary tdd, PageAlert pa)
    {
        if (pa == null || string.IsNullOrEmpty(pa.Message)) return;
        var pageAlerts = tdd.GetPageAlerts(false);
        pageAlerts ??= [];
        pageAlerts.Add(pa);
        tdd.SetPageAlerts(pageAlerts);
    }

    public static IList<PageAlert> GetPageAlerts(this ITempDataDictionary tdd, bool clear)
    {
        var alerts = new List<PageAlert>();
        if (tdd.Peek(PageAlertsKey) is string json && json != "")
        {
            var z = JsonHelpers.FromJson<IList<PageAlert>>(json);
            alerts.AddRange(z);
        }
        if (clear)
        {
            tdd.Remove(PageAlertsKey);
        }
        return alerts;
    }

    public static void SetPageAlerts(this ITempDataDictionary tdd, IList<PageAlert> alerts)
    {
        var json = JsonHelpers.ToJson(alerts ?? PageAlert.None);
        tdd[PageAlertsKey] = json;
    }
}
