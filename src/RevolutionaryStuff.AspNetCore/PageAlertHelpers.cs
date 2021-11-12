using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;

namespace RevolutionaryStuff.AspNetCore;

public static class PageAlertHelpers
{
    private const string PageAlertsKey = "PageAlerts";

    public static void AddPageAlert(this ITempDataDictionary tdd, PageAlert pa)
    {
        if (pa == null || string.IsNullOrEmpty(pa.Message)) return;
        var pageAlerts = tdd.GetPageAlerts(false);
        pageAlerts = pageAlerts ?? new List<PageAlert>();
        pageAlerts.Add(pa);
        tdd.SetPageAlerts(pageAlerts);
    }

    public static IList<PageAlert> GetPageAlerts(this ITempDataDictionary tdd, bool clear)
    {
        var json = tdd.Peek(PageAlertsKey) as string;
        var alerts = new List<PageAlert>();
        if (json != null && json != "")
        {
            var z = JsonConvert.DeserializeObject<IList<PageAlert>>(json);
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
        var json = JsonConvert.SerializeObject(alerts ?? PageAlert.None);
        tdd[PageAlertsKey] = json;
    }
}
